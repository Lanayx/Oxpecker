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
    abstract member Bind<'T> : seq<KeyValuePair<string, StringValues>> -> 'T

[<Struct>]
type internal RawData =
    | SimpleData of simpleData: StringValues
    | ComplexData of complexData: Dictionary<string, StringValues>

/// <summary>
/// Module for parsing models from a generic data set.
/// </summary>
module internal ModelParser =

    open System.ComponentModel
    open TypeShape.Core
    open TypeShape.Core.Utils

    let private (|RawValue|_|) (rawValue: StringValues) =
        if rawValue.Count = 0 then ValueSome null
        elif rawValue.Count = 1 then ValueSome rawValue[0]
        else ValueNone

    let private (|UnionCase|_|) (shape: ShapeFSharpUnion<'T>) (caseName: string) =
        let unionCaseExists caseName (case: ShapeFSharpUnionCase<'T>) =
            String.Equals(case.CaseInfo.Name, caseName, StringComparison.OrdinalIgnoreCase)

        shape.UnionCases |> Array.tryFind(unionCaseExists caseName)

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

    let private unsupported ty = failwith $"Unsupported type '{ty}'."

    let private error (rawData: RawData) : 'T =
        let value =
            match rawData with
            | SimpleData(RawValue Null) -> "<null>"
            | SimpleData data -> $"{data}"
            | ComplexData data -> $"%A{data}"

        failwith $"Could not parse value '{value}' to type '{typeof<'T>}'."

    [<Struct>]
    type internal ParserContext = {
        Culture: CultureInfo
        RawData: RawData
    }

    type private Parser<'T> = ParserContext -> 'T

    type private FieldSetter<'T> = delegate of ParserContext * 'T byref -> unit

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

    and private createParser<'T> (ctx: TypeGenerationContext) : Parser<'T> =
        let wrap (v: Parser<'t>) = unbox<Parser<'T>> v

        let makeFieldSetter (shape: IShapeMember<'DeclaringType>) =
            shape.Accept
                { new IMemberVisitor<_, _> with
                    member _.Visit<'Field>(fieldShape) =
                        let parse = getOrCacheParser<'Field> ctx

                        FieldSetter(fun { Culture = culture; RawData = rawData } instance ->
                            match rawData with
                            | ComplexData(ExactMatch fieldShape.Label matchedData)
                            | ComplexData(PrefixMatch fieldShape.Label matchedData) ->
                                let field =
                                    parse {
                                        Culture = culture
                                        RawData = matchedData
                                    }

                                fieldShape.SetByRef(&instance, field)

                            | _ -> ())
                }

        let makeEnumerableParser (parse: Parser<'Element>) : Parser<'Element seq> =
            fun { Culture = culture; RawData = rawData } ->
                match rawData with
                | SimpleData values ->
                    let res = Array.zeroCreate(values.Count)

                    for i in 0 .. values.Count - 1 do
                        res[i] <-
                            parse {
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
                                parse {
                                    Culture = culture
                                    RawData = ComplexData dict
                                }

                    res

                | _ -> Seq.empty

        match shapeof<'T> with
        | Shape.String ->
            function
            | { RawData = SimpleData(RawValue value) } -> value
            | { RawData = rawData } -> error rawData
            |> wrap

        | Shape.Nullable shape ->
            shape.Accept
                { new INullableVisitor<_> with
                    member _.Visit<'t when 't: (new: unit -> 't) and 't: struct and 't :> ValueType>() = // 'T = Nullable<'t>
                        let parse = getOrCacheParser<'t> ctx

                        function
                        | { RawData = SimpleData(RawValue Null) } -> Nullable()
                        | parserContext -> parse parserContext |> Nullable
                        |> wrap
                }

        | Shape.FSharpOption shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't option
                        let parse = getOrCacheParser<'t> ctx

                        function
                        | { RawData = SimpleData(RawValue Null) } -> None
                        | parserContext -> parse parserContext |> Some
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
                            getOrCacheParser<'t> ctx |> makeEnumerableParser |> wrap
                }

        | Shape.FSharpUnion(:? ShapeFSharpUnion<'T> as shape) ->
            fun { RawData = rawData } ->
                match rawData with
                | SimpleData(RawValue Null) when not shape.IsStructUnion -> Unchecked.defaultof<_>
                | SimpleData(RawValue(NonNull(UnionCase shape case))) -> case.CreateUninitialized()
                | _ -> error rawData
            |> wrap

        | Shape.FSharpRecord(:? ShapeFSharpRecord<'T> as shape) ->
            let fieldSetters = shape.Fields |> Array.map makeFieldSetter

            fun parserContext ->
                let mutable instance = shape.CreateUninitialized()

                for fieldSetter in fieldSetters do
                    fieldSetter.Invoke(parserContext, &instance)

                instance

        | Shape.Struct _ ->
            let typeConverter = TypeDescriptor.GetConverter(typeof<'T>)

            if not <| typeConverter.CanConvertFrom(typeof<string>) then
                unsupported typeof<'T>
            else
                fun { Culture = culture; RawData = rawData } ->
                    match rawData with
                    | SimpleData(RawValue(NonNull value)) ->
                        try
                            typeConverter.ConvertFromString(null, culture, value) |> unbox
                        with _ ->
                            error rawData

                    | _ -> error rawData

        | _ -> unsupported typeof<'T>

    and private cache: TypeCache = TypeCache()

    let rec internal parseModel<'T> (culture: CultureInfo) (rawData: RawData) =
        let parse = getOrCreateParser<'T>()
        parse { Culture = culture; RawData = rawData }

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
            ModelParser.parseModel<'T> options.CultureInfo (ComplexData(Dictionary data))
