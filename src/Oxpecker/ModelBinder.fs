namespace Oxpecker

open System
open System.Collections.Generic
open System.Globalization
open Microsoft.Extensions.Primitives

/// <summary>
/// Interface defining Form and Query parsing methods.
/// Use this interface to customize Form and Query parsing in Oxpecker.
/// </summary>
type IModelBinder =
    abstract member Bind<'T> : Dictionary<string, StringValues> -> 'T

[<Struct>]
type internal RawData =
    | SimpleData of simpleData: StringValues
    | ComplexData of complexData: Dictionary<string, StringValues>

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
    let private (|IndexAccess|_|) (key: string) =
        let key = key.AsSpan()

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
                let subKey = key.Slice(currentIndex + 2)

                ValueSome(struct (index, subKey.ToString()))
            else
                ValueNone
        else
            ValueNone

    let private (|ComplexArray|_|) (data: Dictionary<string, StringValues>) =
        let matchedData = Dictionary()

        for KeyValue(key, value) in data do
            match key with
            | IndexAccess(index, subKey) ->
                if not <| matchedData.ContainsKey(index) then
                    matchedData[index] <- Dictionary()

                matchedData[index][subKey] <- value

            | _ -> ()

        if matchedData.Count = 0 then
            ValueNone
        else
            ValueSome matchedData

    let private (|ExactMatch|_|) (key: string) (data: Dictionary<string, StringValues>) =
        match data.TryGetValue(key) with
        | true, values -> ValueSome(SimpleData values)
        | _ -> ValueNone

    let private (|PrefixMatch|_|) (prefix: string) (data: Dictionary<string, StringValues>) =
        let matchedData = Dictionary()

        for KeyValue(key, value) in data do
            if key.StartsWith(prefix) then
                let matchedKey = key[prefix.Length ..]

                if matchedKey[0] = '.' then // property access
                    matchedData[matchedKey[1..]] <- value
                elif matchedKey[0] = '[' then // index access
                    matchedData[matchedKey] <- value

        if matchedData.Count > 0 then
            ValueSome(ComplexData matchedData)
        else
            ValueNone

    let (|UnionCase|_|) (shape: ShapeFSharpUnion<'T>) (caseName: string) =
        let unionCaseExists caseName (case: ShapeFSharpUnionCase<'T>) =
            String.Equals(case.CaseInfo.Name, caseName, StringComparison.OrdinalIgnoreCase)

        shape.UnionCases |> Array.tryFind(unionCaseExists caseName)

    let private unsupported ty = failwith $"Unsupported type '{ty}'."

    let private error (rawData: RawData) : 'T =
        let value =
            match rawData with
            | SimpleData(RawValue Null) -> "<null>"
            | SimpleData data -> $"{data}"
            | ComplexData data -> $"%A{data}"

        failwith $"Could not parse value '{value}' to type '{typeof<'T>}'."

    type private Struct<'T when 'T: (new: unit -> 'T) and 'T: struct and 'T :> ValueType> = 'T

    type private Enum<'T, 'U when Struct<'T> and 'T: enum<'U>> = 'T

    type private Nullable<'T when Struct<'T>> = 'T

    type private Parsable<'T when 'T: (static member TryParse: string | null * IFormatProvider * byref<'T> -> bool)> = 'T

    [<Struct>]
    type internal ParserContext = {
        Culture: CultureInfo
        RawData: RawData
    }

    type private Parser<'T> = ParserContext -> 'T

    type private FieldSetter<'T> = delegate of ParserContext * 'T byref -> unit

    type private FieldParser<'T> = IShapeMember<'T> -> FieldSetter<'T>

    let rec private getOrCreateParser<'T> () : Parser<'T> =
        match cache.TryFind() with
        | Some x -> x
        | None ->
            use ctx = cache.CreateGenerationContext()
            getOrCacheParser<'T> ctx

    and private getOrCacheParser<'T> (ctx: TypeGenerationContext) : Parser<'T> =
        match ctx.InitOrGetCachedValue<Parser<'T>>(fun cell parserContext -> cell.Value parserContext) with
        | Cached(value = v) -> v
        | NotCached t ->
            let v = createParser<'T> ctx
            ctx.Commit t v

    and inline private createSimpleParser<'T when Parsable<'T>> : Parser<'T> =
        let parser = getOrCreateParser<string | null>()

        fun ({ Culture = culture; RawData = rawData } as parserContext) ->
            try
                let rawValue = parser parserContext
                let mutable result = Unchecked.defaultof<'T>

                if 'T.TryParse(rawValue, culture, &result) then
                    result
                else
                    error rawData
            with _ ->
                error rawData

    and private createEnumerableParser<'Element> (ctx: TypeGenerationContext) : Parser<'Element seq> = // 'T = 'Element seq
        let parser = getOrCacheParser<'Element> ctx

        fun { Culture = culture; RawData = rawData } ->
            match rawData with
            | SimpleData values ->
                let res = Array.zeroCreate(values.Count)

                for i in 0 .. values.Count - 1 do
                    res[i] <-
                        parser {
                            Culture = culture
                            RawData = SimpleData(StringValues values[i])
                        }

                res

            | ComplexData(ComplexArray indexedDicts) ->
                let maxIndex = Seq.max indexedDicts.Keys
                let res = Array.zeroCreate(maxIndex + 1)

                for i in 0..maxIndex do
                    let mutable dict = Unchecked.defaultof<_>

                    if indexedDicts.TryGetValue(i, &dict) then
                        res[i] <-
                            parser {
                                Culture = culture
                                RawData = ComplexData dict
                            }

                res

            | _ -> Seq.empty

    and private createFieldParser (ctx: TypeGenerationContext) : FieldParser<'T> =
        fun shape ->
            shape.Accept
                { new IMemberVisitor<_, _> with
                    member _.Visit<'Field>(fieldShape) =
                        let parser = getOrCacheParser<'Field> ctx

                        FieldSetter(fun { Culture = culture; RawData = rawData } instance ->
                            match rawData with
                            | ComplexData(ExactMatch fieldShape.Label matchedData)
                            | ComplexData(PrefixMatch fieldShape.Label matchedData) ->
                                let field =
                                    parser {
                                        Culture = culture
                                        RawData = matchedData
                                    }
                                fieldShape.SetByRef(&instance, field)
                            | _ -> ())
                }

    and private createParser<'T> (ctx: TypeGenerationContext) : Parser<'T> =
        let wrap (v: Parser<'t>) = unbox<Parser<'T>> v

        match shapeof<'T> with
        | Shape.Guid -> wrap createSimpleParser<Guid>
        | Shape.Int32 -> wrap createSimpleParser<int>
        | Shape.Int64 -> wrap createSimpleParser<int64>
        | Shape.BigInt -> wrap createSimpleParser<bigint>
        | Shape.Double -> wrap createSimpleParser<double>
        | Shape.Decimal -> wrap createSimpleParser<decimal>
        | Shape.DateTime -> wrap createSimpleParser<DateTime>
        | Shape.TimeSpan -> wrap createSimpleParser<TimeSpan>
        | Shape.DateTimeOffset -> wrap createSimpleParser<DateTimeOffset>
        | Shape.Bool ->
            let parser = getOrCreateParser<string | null>()

            fun ({ RawData = rawData } as parserContext) ->
                try
                    let rawValue = parser parserContext

                    match Boolean.TryParse(rawValue) with
                    | true, value -> value
                    | false, _ -> error rawData
                with _ ->
                    error rawData
            |> wrap

        | Shape.String ->
            function
            | { RawData = SimpleData(RawValue value) } -> value
            | { RawData = rawData } -> error rawData
            |> wrap

        | Shape.Enum shape ->
            shape.Accept
                { new IEnumVisitor<_> with
                    member _.Visit<'t, 'u when Enum<'t, 'u>>() = // 'T = enum 't: 'u
                        let parser = getOrCreateParser<string | null>()

                        fun ({ RawData = rawData } as parserContext) ->
                            try
                                let rawValue = parser parserContext
                                match Enum.TryParse<'t>(rawValue, ignoreCase = true) with
                                | true, value -> value
                                | false, _ -> error rawData
                            with _ ->
                                error rawData
                        |> wrap
                }

        | Shape.Nullable shape ->
            shape.Accept
                { new INullableVisitor<_> with
                    member _.Visit<'t when Nullable<'t>>() = // 'T = Nullable<'t>
                        let parser = getOrCacheParser<'t> ctx

                        function
                        | { RawData = SimpleData(RawValue Null) } -> Nullable()
                        | parserContext -> parser parserContext |> Nullable
                        |> wrap
                }

        | Shape.FSharpOption shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't option
                        let parser = getOrCacheParser<'t> ctx

                        function
                        | { RawData = SimpleData(RawValue Null) } -> None
                        | parserContext -> parser parserContext |> Some
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
                        else
                            createEnumerableParser<'t> ctx |> wrap
                }

        | Shape.FSharpUnion(:? ShapeFSharpUnion<'T> as shape) ->
            let parser = getOrCacheParser<string | null> ctx

            fun ({ RawData = rawData } as parserContext) ->
                try
                    match parser parserContext with
                    | Null when not shape.IsStructUnion -> Unchecked.defaultof<_>
                    | NonNull(UnionCase shape case) -> case.CreateUninitialized()
                    | _ -> error rawData
                with _ ->
                    error rawData
            |> wrap

        | Shape.FSharpRecord(:? ShapeFSharpRecord<'T> as shape) ->
            let fieldSetters = shape.Fields |> Array.map(createFieldParser ctx)

            fun parserContext ->
                let mutable instance = shape.CreateUninitialized()

                for fieldSetter in fieldSetters do
                    fieldSetter.Invoke(parserContext, &instance)

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

/// Default implementation of the <see cref="Oxpecker.IModelBinder"/>
type ModelBinder(?options: ModelBinderOptions) =
    let options = defaultArg options <| ModelBinderOptions.Default

    interface IModelBinder with
        /// <summary>
        /// Tries to create an instance of type 'T from a given set of data.
        /// It will try to match each property of 'T with a key from the data dictionary and parse the associated value to the value of 'T's property.
        /// </summary>
        member this.Bind<'T>(data) =
            ModelParser.parseModel<'T> options.CultureInfo (ComplexData data)
