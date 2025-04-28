namespace Oxpecker

open System
open System.Collections.Generic
open System.Globalization
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives

/// <summary>
/// Interface defining Form and Query parsing methods.
/// Use this interface to customize Form and Query parsing in Oxpecker.
/// </summary>
type IModelBinder =
    abstract member Bind<'T> : seq<KeyValuePair<string, StringValues>> -> 'T

[<Struct>]
type internal RawData =
    | SimpleData of rawValue: StringValues
    | ComplexData of offset: int * rawData: Dictionary<string, StringValues>

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
                            member this.Dispose() = that.Return(this) |> ignore
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
module TypeShapeImpl =
    type IParsableVisitor<'R> =
        abstract Visit<'T when IParsable<'T>> : unit -> 'R

    type IShapeParsable =
        abstract Accept: IParsableVisitor<'R> -> 'R

    type ShapeParsable<'T when IParsable<'T>>() =
        interface IShapeParsable with
            member _.Accept v = v.Visit<'T>()

#nowarn 3536
module Shape =

    open TypeShape.Core

    let (|Parsable|_|) (shape: TypeShape) =
        let parsable =
            shape.Type.GetInterfaces()
            |> Seq.tryFind(fun x -> x.IsGenericType && x.GetGenericTypeDefinition() = typedefof<IParsable<int>>)
        match parsable with
        | Some _ ->
            Activator.CreateInstanceGeneric<ShapeParsable<int>>([| shape.Type |]) :?> IShapeParsable
            |> Some
        | None -> None

type UnsupportedTypeException(ty: Type) =
    inherit exn($"Unsupported type '{ty}'.")

type NotParsedException(value: string, ty: Type) =
    inherit exn($"Could not parse value '{value}' to type '{ty}'.")

/// <summary>
/// Module for parsing models from a generic data set.
/// </summary>
module internal ModelParser =

    open TypeShape.Core
    open TypeShape.Core.Utils

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

    let private (|ComplexArray|) (offset, data) =
        let matchedData = DictionaryPool.getIndexed()

        for KeyValue(key, value) in data do
            match key with
            | IndexAccess offset (index, newOffset) ->
                if not <| matchedData.ContainsKey(index) then
                    matchedData[index] <- (newOffset, DictionaryPool.get())

                let struct (_, d) = matchedData[index]
                d[key] <- value

            | _ -> ()

        matchedData

    let private (|ExactMatch|_|) (fieldName: string) (offset, data: Dictionary<string, StringValues>) =
        if offset = 0 then
            match data.TryGetValue(fieldName) with
            | true, values -> ValueSome values
            | _ -> ValueNone
        else
            let mutable result = ValueNone
            let mutable enumerator = data.GetEnumerator()

            while result.IsValueNone && enumerator.MoveNext() do
                let (KeyValue(key, value)) = enumerator.Current
                let s1 = key.AsSpan(offset)
                let s2 = fieldName.AsSpan()
                if s1.SequenceEqual(s2) then
                    result <- ValueSome value
            result

    let private (|PrefixMatch|) (prefix: string) (offset: int, data: Dictionary<string, StringValues>) =
        let mutable newOffset = 0
        let matchedData = DictionaryPool.get()
        for KeyValue(key, value) in data do
            if key.AsSpan(offset).StartsWith(prefix) then
                let tempOffset = offset + prefix.Length
                if key[tempOffset] = '.' then // property access
                    newOffset <- tempOffset + 1
                else // index access
                    newOffset <- tempOffset
                matchedData[key] <- value

        newOffset, matchedData

    let (|UnionCase|_|) (shape: ShapeFSharpUnion<'T>) (caseName: string) =
        let unionCaseExists caseName (case: ShapeFSharpUnionCase<'T>) =
            String.Equals(case.CaseInfo.Name, caseName, StringComparison.OrdinalIgnoreCase)

        shape.UnionCases |> Array.tryFind(unionCaseExists caseName)

    type private Struct<'T when 'T: (new: unit -> 'T) and 'T: struct and 'T :> ValueType> = 'T

    type private Enum<'T, 'U when Struct<'T> and 'T: enum<'U>> = 'T

    type private Nullable<'T when Struct<'T>> = 'T

    [<Struct>]
    type internal ParsingState = {
        Culture: CultureInfo
        RawData: RawData
    }

    type private Parser<'T> = ParsingState -> 'T

    type private MemberSetter<'T> = delegate of ParsingState * 'T byref -> unit

    type private MemberParser<'T> = IShapeMember<'T> -> MemberSetter<'T>

    let private unsupported ty = raise <| UnsupportedTypeException ty

    let private notParsed { RawData = rawData } : 'T =
        let value =
            match rawData with
            | SimpleData(RawValue Null) -> "<null>"
            | SimpleData rawValue -> $"{rawValue}"
            | ComplexData(rawData = rawData) -> $"%A{rawData}"

        raise <| NotParsedException(value, typeof<'T>)

    let rec private getOrCreateParser<'T> () : Parser<'T> =
        match cache.TryFind() with
        | Some x -> x
        | None ->
            use ctx = cache.CreateGenerationContext()
            getOrCacheParser<'T> ctx

    and private getOrCacheParser<'T> (ctx: TypeGenerationContext) : Parser<'T> =
        match ctx.InitOrGetCachedValue<Parser<'T>>(fun cell state -> cell.Value state) with
        | Cached(value = v) -> v
        | NotCached t ->
            let v = createParser<'T> ctx
            ctx.Commit t v

    and private createEnumerableParser<'Element> (ctx: TypeGenerationContext) : Parser<'Element seq> = // 'T = 'Element seq
        let parser = getOrCacheParser<'Element> ctx

        fun state ->
            match state.RawData with
            | SimpleData values ->
                let res = Array.zeroCreate(values.Count)

                for i in 0 .. values.Count - 1 do
                    res[i] <-
                        let rawData = SimpleData(StringValues values[i])
                        parser { state with RawData = rawData }

                res

            | ComplexData(offset, data) ->
                match (offset, data) with
                | ComplexArray indexedDicts ->
                    use indexedDicts = indexedDicts

                    if indexedDicts.Count > 0 then
                        let maxIndex = Seq.max indexedDicts.Keys
                        let res = Array.zeroCreate(maxIndex + 1)

                        for i in 0..maxIndex do
                            match indexedDicts.TryGetValue i with
                            | true, (offset, dict) ->
                                use dict = dict

                                res[i] <-
                                    let rawData = ComplexData(offset, dict)
                                    parser { state with RawData = rawData }

                            | false, _ -> ()

                        res
                    else
                        Unchecked.defaultof<_>

    and private createMemberParser (ctx: TypeGenerationContext) : MemberParser<'T> =
        fun shape ->
            shape.Accept
                { new IMemberVisitor<_, _> with
                    member _.Visit<'Member>(memberShape) =
                        let parser = getOrCacheParser<'Member> ctx

                        MemberSetter(fun state instance ->
                            match state.RawData with
                            | ComplexData(offset, data) ->
                                match offset, data with
                                | ExactMatch memberShape.Label rawValues ->
                                    let rawData = SimpleData rawValues
                                    let memberValue = parser { state with RawData = rawData }

                                    memberShape.SetByRef(&instance, memberValue)

                                | PrefixMatch memberShape.Label (offset, matchedData) ->
                                    use matchedData = matchedData

                                    if matchedData.Count > 0 then
                                        let rawData = ComplexData(offset, matchedData)
                                        let memberValue = parser { state with RawData = rawData }

                                        memberShape.SetByRef(&instance, memberValue)

                            | _ -> ())
                }

    and private createParser<'T> (ctx: TypeGenerationContext) : Parser<'T> =
        let wrap (v: Parser<'t>) = unbox<Parser<'T>> v

        match shapeof<'T> with
        | Shape.String ->
            function
            | { RawData = SimpleData(RawValue value) } -> value
            | state -> notParsed state
            |> wrap

        | Shape.Parsable shape ->
            shape.Accept
                { new IParsableVisitor<_> with
                    member _.Visit<'t when 't :> IParsable<'t>>() =
                        let parser = getOrCacheParser<string | null> ctx

                        fun state ->
                            try
                                let rawValue = parser state
                                let mutable result = Unchecked.defaultof<'t>
                                if 't.TryParse(rawValue, state.Culture, &result) then
                                    result
                                else
                                    notParsed state
                            with _ ->
                                notParsed state
                        |> wrap
                }

        | Shape.Enum shape ->
            shape.Accept
                { new IEnumVisitor<_> with
                    member _.Visit<'t, 'u when Enum<'t, 'u>>() = // 'T = enum 't: 'u
                        let parser = getOrCacheParser<string | null> ctx

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
                        let parser = getOrCacheParser<'t> ctx

                        function
                        | { RawData = SimpleData(RawValue Null) } -> Nullable()
                        | state -> parser state |> Nullable
                        |> wrap
                }

        | Shape.FSharpOption shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't option
                        let parser = getOrCacheParser<'t> ctx

                        function
                        | { RawData = SimpleData(RawValue Null) } -> None
                        | state -> parser state |> Some
                        |> wrap
                }

        | Shape.FSharpList shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't list
                        getOrCacheParser<'t seq> ctx >> Seq.toList |> wrap
                }

        | Shape.Array shape when shape.Rank = 1 ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't array
                        getOrCacheParser<'t seq> ctx >> Seq.toArray |> wrap
                }

        | Shape.ResizeArray shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = ResizeArray<'t>
                        getOrCacheParser<'t seq> ctx >> ResizeArray |> wrap
                }

        | Shape.Enumerable shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't seq
                        if Type.(<>)(typeof<'T>, typeof<'t seq>) then
                            unsupported typeof<'T>

                        createEnumerableParser<'t> ctx |> wrap
                }

        | Shape.FSharpUnion(:? ShapeFSharpUnion<'T> as shape) ->
            let parser = getOrCacheParser<string | null> ctx

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
            let fieldSetters = shape.Fields |> Array.map(createMemberParser ctx)

            fun state ->
                let mutable instance = shape.CreateUninitialized()

                for fieldSetter in fieldSetters do
                    fieldSetter.Invoke(state, &instance)

                instance

        | Shape.CliMutable(:? ShapeCliMutable<'T> as shape) ->
            let propertySetters = shape.Properties |> Array.map(createMemberParser ctx)

            fun state ->
                let mutable instance = shape.CreateUninitialized()

                for propertySetter in propertySetters do
                    propertySetter.Invoke(state, &instance)

                instance

        | _ -> unsupported typeof<'T>

    and private cache: TypeCache = TypeCache()

    let rec internal parseModel<'T> (culture: CultureInfo) (rawData: RawData) =
        let parser = getOrCreateParser<'T>()

        parser { Culture = culture; RawData = rawData }

/// <summary>
/// Configuration options for the default <see cref="Oxpecker.ModelBinder"/>
/// </summary>
type ModelBinderOptions = {
    CultureInfo: CultureInfo
} with
    static member Default = {
        CultureInfo = CultureInfo.InvariantCulture
    }

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
            ModelParser.parseModel<'T> options.CultureInfo (ComplexData(0, dictionary))
