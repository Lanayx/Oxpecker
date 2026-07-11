namespace Oxpecker

open System
open System.Collections.Generic
open System.Globalization
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open TypeShape.Core.Utils

/// <summary>
/// Configuration options for the default <see cref="Oxpecker.ModelBinder"/>
/// </summary>
type ModelBinderOptions = {
    CultureInfo: CultureInfo
    CaseInsensitiveMatching: bool
    /// Upper bound on the index used when binding indexed collections (e.g. "Items[N]" or "Items[N].Field").
    /// The index comes from the (untrusted) request key; indices that reach or exceed this limit
    /// (or overflow Int32) raise <see cref="ModelBindException"/>.
    /// Defaults to 1024 (same as ASP.NET Core's MaxModelBindingCollectionSize).
    MaxCollectionSize: int
} with
    static member Default = {
        CultureInfo = CultureInfo.InvariantCulture
        CaseInsensitiveMatching = false
        MaxCollectionSize = 1024
    }

/// <summary>
/// Interface defining Form and Query parsing methods.
/// Use this interface to customize Form and Query parsing in Oxpecker.
/// </summary>
type IModelBinder =
    abstract member Bind<'T> : seq<KeyValuePair<string, StringValues>> -> 'T

[<Struct>]
type internal ComplexData = {
    Offset: int
    Data: Dictionary<string, StringValues>
}

[<Struct>]
type internal RawData =
    | SimpleData of rawValue: StringValues
    | ComplexData of rawData: ComplexData

module internal RawData =
    let initComplexData data = ComplexData { Offset = 0; Data = data }

[<AbstractClass>]
type private PooledDictionary<'Key, 'Value when 'Key: not null and 'Key: equality>() =
    inherit Dictionary<'Key, 'Value>()

    abstract member Dispose: unit -> unit

    interface IDisposable with
        member this.Dispose() = this.Dispose()

module private DictionaryPool =

    open Microsoft.Extensions.ObjectPool

    let private maximumRetained = Environment.ProcessorCount * 2
    /// Dictionaries grown by a single large request are dropped instead of pooled:
    /// Clear() keeps the grown bucket arrays, so retaining them would pin that
    /// capacity in the pool for the process lifetime.
    let private maximumRetainedCount = 256

    type private DictionaryPool<'Key, 'Value when 'Key: not null and 'Key: equality>() as that =
        inherit
            DefaultObjectPool<PooledDictionary<'Key, 'Value>>(
                { new IPooledObjectPolicy<_> with
                    member _.Create() =
                        { new PooledDictionary<_, _>() with
                            member this.Dispose() = that.Return(this)
                        }
                    member _.Return(dict) =
                        if dict.Count > maximumRetainedCount then
                            false
                        else
                            dict.Clear()
                            true
                },
                maximumRetained
            )

    let get = DictionaryPool<string, StringValues>().Get
    let getIndexedValues = DictionaryPool<int, StringValues>().Get
    let getIndexed =
        DictionaryPool<int, struct (int * PooledDictionary<string, StringValues>)>().Get

[<AutoOpen>]
module internal TypeShapeImpl =
    type IParsableVisitor<'R> =
        abstract Visit<'T when IParsable<'T>> : unit -> 'R

    type IShapeParsable =
        abstract Accept: IParsableVisitor<'R> -> 'R

    type ShapeParsable<'T when IParsable<'T>>() =
        interface IShapeParsable with
            member _.Accept v = v.Visit<'T>()

#nowarn 3536
module internal Shape =

    open TypeShape.Core

    let private implements<'T> (ty: Type) =
        typedefof<'T>.FullName |> Unchecked.nonNull |> ty.GetInterface |> isNull |> not

    type private Any = int // represents any type that implements IParsable<_>

    let (|Parsable|_|) (shape: TypeShape) =
        if shape.Type |> implements<IParsable<Any>> then
            Activator.CreateInstanceGeneric<ShapeParsable<Any>>([| shape.Type |]) :?> IShapeParsable
            |> Some
        else
            None

type internal UnsupportedTypeException(ty: Type) =
    inherit exn($"Unsupported type '{ty}'.")

type internal NotParsedException(value: string, ty: Type) =
    inherit exn($"Could not parse value '%s{value}' to type '{ty}'.")

type internal MaxCollectionSizeExceededException(maxCollectionSize: int) =
    inherit
        exn(
            $"The collection index reached or exceeded the maximum allowed value of %i{maxCollectionSize}."
        )

/// <summary>
/// Module for parsing models from a generic data set.
/// </summary>
module internal ModelParser =

    open TypeShape.Core

    let private (|RawValue|_|) (rawValue: StringValues) =
        if rawValue.Count = 0 then ValueSome null
        elif rawValue.Count = 1 then ValueSome rawValue[0]
        else ValueNone

    /// Parses a "[index]" prefix of the span, returning the index and the position just after ']'.
    /// All span accesses are bounds-checked so that malformed keys (e.g. "[", "[]x") fail the
    /// match instead of throwing. A well-formed index that reaches or exceeds maxCollectionSize
    /// (or overflows Int32) yields the sentinel index -1 so the caller can reject the request,
    /// mirroring ASP.NET Core's MaxModelBindingCollectionSize behaviour.
    let inline private tryParseIndex (maxCollectionSize: int) (key: ReadOnlySpan<char>) =
        if key.Length > 2 && key[0] = '[' then
            let mutable currentIndex = 1
            let mutable index = 0L
            while currentIndex < key.Length && Char.IsAsciiDigit key[currentIndex] do
                // Once the value is out of range the accumulation stops, so an absurdly long
                // digit run cannot overflow Int64 while the shape is still being scanned.
                if index <= int64 Int32.MaxValue then
                    index <- index * 10L + int64(key[currentIndex] - '0')
                currentIndex <- currentIndex + 1
            if
                currentIndex > 1 // at least one digit
                && currentIndex < key.Length
                && key[currentIndex] = ']'
            then
                if index < int64 maxCollectionSize then
                    ValueSome(struct (int index, currentIndex + 1))
                else
                    ValueSome(struct (-1, currentIndex + 1))
            else
                ValueNone
        else
            ValueNone

    /// Active pattern for parsing keys in the format "[index].subKey".
    /// Index -1 means the index reached MaxCollectionSize, regardless of what follows the bracket.
    let private (|IndexAccess|_|) (offset: int) (maxCollectionSize: int) (key: string) =
        let key = key.AsSpan(offset)
        match tryParseIndex maxCollectionSize key with
        | ValueSome(struct (index, afterBracket)) when
            index < 0 || (afterBracket + 1 < key.Length && key[afterBracket] = '.')
            ->
            ValueSome(struct (index, offset + afterBracket + 1))
        | _ -> ValueNone

    /// Active pattern for parsing keys in the format "[index]".
    /// Index -1 means the index reached MaxCollectionSize, regardless of what follows the bracket.
    let private (|IndexedValue|_|) (offset: int) (maxCollectionSize: int) (key: string) =
        let key = key.AsSpan(offset)
        match tryParseIndex maxCollectionSize key with
        | ValueSome(struct (index, afterBracket)) when index < 0 || afterBracket = key.Length -> ValueSome index
        | _ -> ValueNone

    let private (|IndexedValues|) (maxCollectionSize: int) { Offset = offset; Data = data } =
        let matchedData = DictionaryPool.getIndexedValues()
        let mutable overLimit = false
        for KeyValue(key, value) in data do
            match key with
            | IndexedValue offset maxCollectionSize index ->
                if index < 0 then
                    overLimit <- true
                else
                    // Distinct key spellings of the same index (e.g. "[1]" and "[01]") keep the first value.
                    matchedData.TryAdd(index, value) |> ignore
            | _ -> ()
        if overLimit then
            // The rented dictionary must be returned before rejecting the request,
            // otherwise hostile requests would drain the pool.
            matchedData.Dispose()
            raise <| MaxCollectionSizeExceededException maxCollectionSize
        matchedData

    let private (|ComplexArray|) (maxCollectionSize: int) { Offset = offset; Data = data } =
        let matchedData = DictionaryPool.getIndexed()
        let mutable overLimit = false
        for KeyValue(key, value) in data do
            match key with
            | IndexAccess offset maxCollectionSize (index, newOffset) ->
                if index < 0 then
                    overLimit <- true
                else
                    match matchedData.TryGetValue(index) with
                    | true, struct (_, subdict) -> subdict[key] <- value
                    | false, _ ->
                        let subdict = DictionaryPool.get()
                        subdict[key] <- value
                        matchedData[index] <- struct (newOffset, subdict)
            | _ -> ()
        if overLimit then
            // The rented dictionaries must be returned before rejecting the request,
            // otherwise hostile requests would drain the pool.
            for struct (_, subdict) in matchedData.Values do
                subdict.Dispose()
            matchedData.Dispose()
            raise <| MaxCollectionSizeExceededException maxCollectionSize
        matchedData

    let private (|ExactMatch|_|) (memberName: string) (ignoreCase: bool) { Offset = offset; Data = data } =
        if offset = 0 && (not ignoreCase) then
            match data.TryGetValue(memberName) with
            | true, values -> ValueSome values
            | _ -> ValueNone
        else
            let mutable result = ValueNone
            use mutable enumerator = data.GetEnumerator()
            let comparisonType =
                if ignoreCase then
                    StringComparison.OrdinalIgnoreCase
                else
                    StringComparison.Ordinal
            let candidate = memberName.AsSpan()
            while result.IsValueNone && enumerator.MoveNext() do
                let (KeyValue(key, value)) = enumerator.Current
                let mutable current = key.AsSpan(offset)
                // At nested levels the key still carries its '.' separator; skip it before comparing.
                if offset > 0 && current.Length > 0 && current[0] = '.' then
                    current <- current.Slice(1)
                if MemoryExtensions.Equals(current, candidate, comparisonType) then
                    result <- ValueSome value
            result

    let private (|PrefixMatch|) (prefix: string) (ignoreCase: bool) { Offset = offset; Data = data } =
        let matchedData = DictionaryPool.get()
        let mutable nextOffset = 0
        let comparisonType =
            if ignoreCase then
                StringComparison.OrdinalIgnoreCase
            else
                StringComparison.Ordinal
        for KeyValue(key, value) in data do
            let mutable keySpan = key.AsSpan(offset)
            let mutable separatorLength = 0
            // At nested levels the key still carries its '.' separator; skip it before comparing.
            // The separator is NOT consumed into nextOffset: keys sharing a prefix can mix '.'
            // and '[' shapes, so consuming it would make the offset depend on key order.
            if offset > 0 && keySpan.Length > 0 && keySpan[0] = '.' then
                keySpan <- keySpan.Slice(1)
                separatorLength <- 1
            if keySpan.StartsWith(prefix, comparisonType) then
                let candidateOffset = offset + separatorLength + prefix.Length
                if
                    candidateOffset < key.Length
                    && (key[candidateOffset] = '.' // property access
                        || key[candidateOffset] = '[') // index access
                then
                    nextOffset <- candidateOffset
                    matchedData[key] <- value
        struct (nextOffset, matchedData)

    let (|UnionCase|_|) (shape: ShapeFSharpUnion<'T>) (caseName: string) =
        let unionCaseExists caseName (case: ShapeFSharpUnionCase<'T>) =
            String.Equals(case.CaseInfo.Name, caseName, StringComparison.OrdinalIgnoreCase)
        shape.UnionCases |> Array.tryFind(unionCaseExists caseName)

    type private Struct<'T when 'T: (new: unit -> 'T) and 'T: struct and 'T :> ValueType> = 'T

    type private Enum<'T, 'U when Struct<'T> and 'T: enum<'U>> = 'T

    type private Nullable<'T when Struct<'T>> = 'T

    type private Parser<'T> = RawData -> 'T

    type private MemberSetter<'T> = delegate of RawData * 'T byref -> unit

    type private MemberParser<'T> = IShapeMember<'T> -> MemberSetter<'T>

    let private unsupported ty = raise <| UnsupportedTypeException ty

    let private notParsed rawData : 'T =
        let value =
            match rawData with
            | SimpleData(RawValue Null) -> "<null>"
            | SimpleData rawValue -> $"{rawValue}"
            | ComplexData { Data = rawData } -> $"%A{rawData}"

        raise <| NotParsedException(value, typeof<'T>)

    /// Determines whether a collection element of the given type is parsed from a single flat
    /// value (the indexed "simple value" path, e.g. Items[0]=1) rather than from a nested object.
    /// The dispatch deliberately mirrors the simple-data branches of createParser (same TypeShape
    /// patterns, same order) so the two cannot drift apart. Unions qualify because they are always
    /// bound from a single case-name string, and wrappers (nullable/option/collections) delegate
    /// to their element type.
    let rec private supportsSimpleData (ty: Type) : bool =
        match TypeShape.Create ty with
        | Shape.String -> true
        | Shape.Parsable _ -> true
        | Shape.Enum _ -> true
        | Shape.Nullable _ -> supportsSimpleData(ty.GetGenericArguments()[0])
        | Shape.FSharpOption shape -> supportsSimpleData shape.Element.Type
        | Shape.FSharpList shape -> supportsSimpleData shape.Element.Type
        | Shape.Array shape when shape.Rank = 1 -> supportsSimpleData shape.Element.Type
        | Shape.ResizeArray shape -> supportsSimpleData shape.Element.Type
        | Shape.Enumerable shape -> supportsSimpleData shape.Element.Type
        | Shape.FSharpUnion _ -> true
        | _ -> false

    let rec private getOrCreateParser<'T> (cache: TypeCache) (options: ModelBinderOptions) : Parser<'T> =
        match cache.TryFind() with
        | Some x -> x
        | None ->
            use ctx = cache.CreateGenerationContext()
            getOrCacheParser<'T> ctx options

    and private getOrCacheParser<'T> (ctx: TypeGenerationContext) (options: ModelBinderOptions) : Parser<'T> =
        match ctx.InitOrGetCachedValue<Parser<'T>>(fun cell state -> cell.Value state) with
        | Cached(value = v) -> v
        | NotCached t ->
            let v = createParser<'T> ctx options
            ctx.Commit t v

    and private createEnumerableParser<'Element>
        (ctx: TypeGenerationContext)
        (options: ModelBinderOptions)
        : Parser<'Element seq> =
        let parser = getOrCacheParser<'Element> ctx options

        let parseSimpleValues (values: StringValues) =
            let res = Array.zeroCreate(values.Count)
            for i in 0 .. values.Count - 1 do
                res[i] <-
                    let rawData = SimpleData(StringValues values[i])
                    parser rawData
            res

        // Elements are bound sequentially from index 0; binding stops at the first missing
        // index and later indices are ignored, matching ASP.NET Core's collection binding.
        if supportsSimpleData typeof<'Element> then
            function
            | SimpleData values -> parseSimpleValues values
            | ComplexData(IndexedValues options.MaxCollectionSize indexedValues) ->
                use indexedValues = indexedValues
                let res = ResizeArray(indexedValues.Count)
                let mutable proceed = true
                while proceed do
                    match indexedValues.TryGetValue res.Count with
                    | true, values ->
                        // A multi-valued index (e.g. the checkbox + hidden-input fallback idiom)
                        // binds its first value.
                        let values = if values.Count > 1 then StringValues values[0] else values
                        res.Add(parser(SimpleData values))
                    | false, _ -> proceed <- false
                res
        else
            function
            | SimpleData values -> parseSimpleValues values
            | ComplexData(ComplexArray options.MaxCollectionSize indexedDicts) ->
                use indexedDicts = indexedDicts
                try
                    let res = ResizeArray(indexedDicts.Count)
                    let mutable proceed = true
                    while proceed do
                        match indexedDicts.TryGetValue res.Count with
                        | true, struct (offset, dict) -> res.Add(parser(ComplexData { Offset = offset; Data = dict }))
                        | false, _ -> proceed <- false
                    res
                finally
                    // Subdictionaries after a gap are never visited, so they are returned
                    // here rather than one by one inside the loop.
                    for struct (_, subdict) in indexedDicts.Values do
                        subdict.Dispose()

    and private createMemberParser (ctx: TypeGenerationContext) (options: ModelBinderOptions) : MemberParser<'T> =
        fun shape ->
            shape.Accept
                { new IMemberVisitor<_, _> with
                    member _.Visit<'Member>(memberShape) =
                        let parser = getOrCacheParser<'Member> ctx options
                        MemberSetter(fun state instance ->
                            match state with
                            | ComplexData(ExactMatch memberShape.Label options.CaseInsensitiveMatching rawValues) ->
                                let rawData = SimpleData rawValues
                                let memberValue = parser rawData
                                memberShape.SetByRef(&instance, memberValue)
                            | ComplexData(PrefixMatch memberShape.Label options.CaseInsensitiveMatching (offset,
                                                                                                         matchedData)) ->
                                use matchedData = matchedData
                                if matchedData.Count > 0 then
                                    let rawData = ComplexData { Offset = offset; Data = matchedData }
                                    let memberValue = parser rawData
                                    memberShape.SetByRef(&instance, memberValue)
                            | _ -> ())
                }

    and private createParser<'T> (ctx: TypeGenerationContext) (options: ModelBinderOptions) : Parser<'T> =
        let wrap (v: Parser<'t>) = unbox<Parser<'T>> v

        match shapeof<'T> with
        | Shape.String ->
            function
            | SimpleData(RawValue value) -> value
            | state -> notParsed state
            |> wrap

        | Shape.Parsable shape ->
            shape.Accept
                { new IParsableVisitor<_> with
                    member _.Visit<'t when 't :> IParsable<'t>>() =
                        let parser = getOrCacheParser<string | null> ctx options
                        fun state ->
                            try
                                let rawValue = parser state
                                match 't.TryParse(rawValue, options.CultureInfo) with
                                | true, value -> value
                                | false, _ -> notParsed state
                            with _ ->
                                notParsed state
                        |> wrap
                }

        | Shape.Enum shape ->
            shape.Accept
                { new IEnumVisitor<_> with
                    member _.Visit<'t, 'u when Enum<'t, 'u>>() = // 'T = enum 't: 'u
                        let parser = getOrCacheParser<string | null> ctx options
                        fun state ->
                            try
                                let rawValue = parser state
                                match Enum.TryParse<'t>(rawValue, ignoreCase = true) with
                                | true, value -> value
                                | false, _ -> notParsed state
                            with _ ->
                                notParsed state
                        |> wrap
                }

        | Shape.Nullable shape ->
            shape.Accept
                { new INullableVisitor<_> with
                    member _.Visit<'t when Nullable<'t>>() = // 'T = Nullable<'t>
                        let parser = getOrCacheParser<'t> ctx options
                        function
                        | SimpleData(RawValue Null) -> Nullable()
                        | state -> parser state |> Nullable
                        |> wrap
                }

        | Shape.FSharpOption shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't option
                        let parser = getOrCacheParser<'t> ctx options
                        function
                        | SimpleData(RawValue Null) -> None
                        | state -> parser state |> Some
                        |> wrap
                }

        | Shape.FSharpList shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't list
                        let parser = getOrCacheParser<'t seq> ctx options
                        fun state -> parser state |> Seq.toList
                        |> wrap
                }

        | Shape.Array shape when shape.Rank = 1 ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't array
                        let parser = getOrCacheParser<'t seq> ctx options
                        fun state -> parser state |> Seq.toArray
                        |> wrap
                }

        | Shape.ResizeArray shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = ResizeArray<'t>
                        let parser = getOrCacheParser<'t seq> ctx options
                        fun state -> parser state |> ResizeArray
                        |> wrap
                }

        | Shape.Enumerable shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't seq
                        if Type.(<>)(typeof<'T>, typeof<'t seq>) then
                            unsupported typeof<'T>
                        createEnumerableParser<'t> ctx options |> wrap
                }

        | Shape.FSharpUnion(:? ShapeFSharpUnion<'T> as shape) ->
            let parser = getOrCacheParser<string | null> ctx options
            fun state ->
                try
                    match parser state with
                    | Null when not shape.IsStructUnion -> Unchecked.defaultof<_>
                    | NonNull(UnionCase shape case) -> case.CreateUninitialized()
                    | _ -> notParsed state
                with _ ->
                    notParsed state
            |> wrap

        | Shape.FSharpRecord(:? ShapeFSharpRecord<'T> as shape) ->
            let fieldSetters = shape.Fields |> Array.map(createMemberParser ctx options)
            fun state ->
                let mutable instance = shape.CreateUninitialized()
                for fieldSetter in fieldSetters do
                    fieldSetter.Invoke(state, &instance)
                instance

        | Shape.CliMutable(:? ShapeCliMutable<'T> as shape) ->
            let propertySetters = shape.Properties |> Array.map(createMemberParser ctx options)
            fun state ->
                let mutable instance = shape.CreateUninitialized()
                for propertySetter in propertySetters do
                    propertySetter.Invoke(state, &instance)
                instance

        | _ -> unsupported typeof<'T>

    let rec internal parseModel<'T> cache options rawData =
        let parser = getOrCreateParser<'T> cache options
        parser rawData

[<AutoOpen>]
module private DictionaryLikeCollectionHelper =

    open System.Linq.Expressions

    type DictionaryLikeCollection<'T
        when 'T :> IEnumerable<KeyValuePair<string, StringValues>>
        and 'T: (member Keys: ICollection<string>)
        and 'T: (member get_Item: string -> StringValues)
        and 'T: (member ContainsKey: string -> bool)
        and 'T: (member TryGetValue: string * byref<StringValues> -> bool)> = 'T

    let inline private getUnderlyingDict<'T when DictionaryLikeCollection<'T>> =
        let param = Expression.Parameter(typeof<'T>)
        let storeProp = Expression.Property(param, "Store")
        let getStoreExpr = Expression.Lambda<_>(storeProp, param)
        let getStore: Func<'T, Dictionary<string, StringValues>> = getStoreExpr.Compile()
        fun collection -> getStore.Invoke(collection)

    let formCollectionDict = getUnderlyingDict<FormCollection>
    let queryCollectionDict = getUnderlyingDict<QueryCollection>

/// Default implementation of the <see cref="Oxpecker.IModelBinder"/>
type ModelBinder(?options: ModelBinderOptions) =
    let options = defaultArg options <| ModelBinderOptions.Default
    let cache = TypeCache()

    interface IModelBinder with
        /// <summary>
        /// Tries to create an instance of type 'T from a given set of data.
        /// It will try to match each property of 'T with a key from the data dictionary and parse the associated value to the value of 'T's property.
        /// </summary>
        member this.Bind<'T>(data) =
            let dictionary =
                match data with
                | :? FormCollection as formCollection -> formCollection |> formCollectionDict
                | :? QueryCollection as queryCollection -> queryCollection |> queryCollectionDict
                | _ -> Dictionary data
            ModelParser.parseModel<'T> cache options (RawData.initComplexData dictionary)
