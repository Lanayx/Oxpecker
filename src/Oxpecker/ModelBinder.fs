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
} with
    static member Default = {
        CultureInfo = CultureInfo.InvariantCulture
        CaseInsensitiveMatching = false
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

    type private DictionaryPool<'Key, 'Value when 'Key: not null and 'Key: equality>() as that =
        inherit
            DefaultObjectPool<PooledDictionary<'Key, 'Value>>(
                { new IPooledObjectPolicy<_> with
                    member _.Create() =
                        { new PooledDictionary<_, _>() with
                            member this.Dispose() = that.Return(this)
                        }
                    member _.Return(dict) =
                        dict.Clear()
                        dict.Count = 0
                },
                maximumRetained
            )

    let get = DictionaryPool<string, StringValues>().Get
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

/// <summary>
/// Module for parsing models from a generic data set.
/// </summary>
module internal ModelParser =

    open TypeShape.Core

    let private (|RawValue|_|) (rawValue: StringValues) =
        if rawValue.Count = 0 then ValueSome null
        elif rawValue.Count = 1 then ValueSome rawValue[0]
        else ValueNone

    /// Active pattern for parsing keys in the format "[index].subKey".
    let private (|IndexAccess|_|) (offset: int) (key: string) =
        let key = key.AsSpan(offset)
        if key[0] = '[' then
            let lastIndex = key.Length - 1
            let mutable currentIndex = 1
            while key[currentIndex] |> Char.IsDigit do
                currentIndex <- currentIndex + 1
            if
                currentIndex > 1 // at least one digit
                && key[currentIndex] = ']'
                && key[currentIndex + 1] = '.'
                && currentIndex + 2 < lastIndex // at least one symbol after '].'
            then
                let index = Int32.Parse(key.Slice(1, currentIndex - 1))
                let newOffset = offset + currentIndex + 2
                ValueSome(struct (index, newOffset))
            else
                ValueNone
        else
            ValueNone

    let private (|ComplexArray|) { Offset = offset; Data = data } =
        let matchedData = DictionaryPool.getIndexed()
        for KeyValue(key, value) in data do
            match key with
            | IndexAccess offset (index, newOffset) ->
                match matchedData.TryGetValue(index) with
                | true, struct (_, subdict) -> subdict[key] <- value
                | false, _ ->
                    let subdict = DictionaryPool.get()
                    subdict[key] <- value
                    matchedData[index] <- struct (newOffset, subdict)
            | _ -> ()
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
                let current = key.AsSpan(offset)
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
            if key.AsSpan(offset).StartsWith(prefix, comparisonType) then
                nextOffset <- offset + prefix.Length
                if key[nextOffset] = '.' then // property access
                    nextOffset <- nextOffset + 1
                    matchedData[key] <- value
                elif key[nextOffset] = '[' then // index access
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
        function
        | SimpleData values ->
            let res = Array.zeroCreate(values.Count)
            for i in 0 .. values.Count - 1 do
                res[i] <-
                    let rawData = SimpleData(StringValues values[i])
                    parser rawData
            res
        | ComplexData(ComplexArray indexedDicts) ->
            use indexedDicts = indexedDicts
            let res = ResizeArray()
            for i in indexedDicts.Keys do
                while i > res.Count - 1 do
                    res.Add(Unchecked.defaultof<_>)
                let struct (offset, dict) = indexedDicts[i]
                use dict = dict
                res[i] <-
                    let rawData = ComplexData { Offset = offset; Data = dict }
                    parser rawData
            res

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
