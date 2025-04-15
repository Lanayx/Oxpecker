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
    | ComplexData of complexData: IDictionary<string, StringValues>

/// <summary>
/// Module for parsing models from a generic data set.
/// </summary>
module internal ModelParser =

    open System.ComponentModel
    open System.Text.RegularExpressions
    open TypeShape.Core
    open TypeShape.Core.Utils

    let private unsupported ty = failwith $"Unsupported type '{ty}'."

    let (|RawValue|_|) (rawValue: StringValues) =
        if rawValue.Count = 0 then ValueSome null
        elif rawValue.Count = 1 then ValueSome rawValue[0]
        else ValueNone

    let private error (rawData: RawData) : 'T =
        let value =
            match rawData with
            | SimpleData(RawValue Null) -> "<null>"
            | SimpleData data -> $"{data}"
            | ComplexData data -> $"%A{data}"

        failwith $"Could not parse value '{value}' to type '{typeof<'T>}'."

    let private (|UnionCase|_|) (shape: ShapeFSharpUnion<'T>) (caseName: string) =
        let unionCaseExists caseName (case: ShapeFSharpUnionCase<'T>) =
            String.Equals(case.CaseInfo.Name, caseName, StringComparison.OrdinalIgnoreCase)

        shape.UnionCases |> Array.tryFind(unionCaseExists caseName)

    let private (|ComplexArray|_|) (data: IDictionary<string, StringValues>) =
        let indexAccess = "\[(\d+)\]\.(.+)"
        let matchedData = Dictionary()

        for KeyValue(key, value) in data do
            let m = Regex.Match(key, indexAccess)
            if m.Success then
                let index = int m.Groups.[1].Value
                let key = m.Groups.[2].Value

                if not <| matchedData.ContainsKey(index) then
                    matchedData[index] <- Dictionary()

                matchedData[index][key] <- value

        if matchedData.Count = 0 then
            ValueNone
        else
            ValueSome matchedData

    let private (|ExactMatch|_|) (key: string) (data: IDictionary<string, StringValues>) =
        match data.TryGetValue(key) with
        | true, values -> ValueSome(SimpleData values)
        | _ -> ValueNone

    let private (|PrefixMatch|_|) (prefix: string) (data: IDictionary<string, StringValues>) =
        let matchedData = Dictionary()

        for KeyValue(key, value) in data do
            if key.StartsWith(prefix) then
                let matchedKey = key.[prefix.Length ..]
                if matchedKey.StartsWith '.' then
                    matchedData[matchedKey.[1..]] <- value
                elif matchedKey.StartsWith '[' then
                    matchedData[matchedKey] <- value

        if matchedData.Count > 0 then
            ValueSome(ComplexData matchedData)
        else
            ValueNone

    [<Struct>]
    type internal ParserContext = {
        Culture: CultureInfo
        RawData: RawData
    }

    type private Parser<'T> = ParserContext -> 'T

    type private FieldSetter<'T> = delegate of ParserContext * 'T -> unit

    let rec private mkParser<'T> () : Parser<'T> =
        match cache.TryFind() with
        | Some x -> x
        | None ->
            use ctx = cache.CreateGenerationContext()
            mkParserCached<'T> ctx

    and private mkParserCached<'T> (ctx: TypeGenerationContext) : Parser<'T> =
        match ctx.InitOrGetCachedValue<Parser<'T>>(fun cell parserContext -> cell.Value parserContext) with
        | Cached(value = v) -> v
        | NotCached t ->
            let v = mkParserAux<'T> ctx
            ctx.Commit t v

    and private mkParserAux<'T> (ctx: TypeGenerationContext) : Parser<'T> =
        let wrap (v: Parser<'t>) = unbox<Parser<'T>> v

        let mkFieldSetter (shape: IShapeMember<'DeclaringType>) =
            shape.Accept
                { new IMemberVisitor<_, _> with
                    member _.Visit<'Field>(fieldShape) =
                        let parse = mkParserCached<'Field> ctx

                        FieldSetter(fun { Culture = culture; RawData = rawData } instance ->
                            match rawData with
                            | ComplexData(ExactMatch fieldShape.Label matchedData)
                            | ComplexData(PrefixMatch fieldShape.Label matchedData) ->
                                {
                                    Culture = culture
                                    RawData = matchedData
                                }
                                |> parse
                                |> fieldShape.Set instance
                                |> ignore

                            | _ -> ())
                }

        let mkEnumerableParser (parse: Parser<'Element>) : Parser<'Element seq> =
            fun { Culture = culture; RawData = rawData } ->
                match rawData with
                | SimpleData values ->
                    seq {
                        for value in values ->
                            parse {
                                Culture = culture
                                RawData = SimpleData(StringValues value)
                            }
                    }

                | ComplexData(ComplexArray indexedDicts) ->
                    let maxIndex = Seq.max indexedDicts.Keys

                    seq {
                        for index in 0..maxIndex ->
                            match indexedDicts.TryGetValue index with
                            | true, dict ->
                                parse {
                                    Culture = culture
                                    RawData = ComplexData dict
                                }

                            | _ -> Unchecked.defaultof<_>
                    }

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
                        let parse = mkParserCached<'t> ctx

                        function
                        | { RawData = SimpleData(RawValue Null) } -> Nullable()
                        | parserContext -> parse parserContext |> Nullable
                        |> wrap
                }

        | Shape.FSharpOption shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't option
                        let parse = mkParserCached<'t> ctx

                        function
                        | { RawData = SimpleData(RawValue Null) } -> None
                        | parserContext -> parse parserContext |> Some
                        |> wrap
                }

        | Shape.FSharpList shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't list
                        mkParserCached<'t seq> ctx >> Seq.toList |> wrap
                }

        | Shape.Array shape when shape.Rank = 1 ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't array
                        mkParserCached<'t seq> ctx >> Seq.toArray |> wrap
                }

        | Shape.ResizeArray shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = ResizeArray<'t>
                        mkParserCached<'t seq> ctx >> ResizeArray |> wrap
                }

        | Shape.Enumerable shape ->
            shape.Element.Accept
                { new ITypeVisitor<_> with
                    member _.Visit<'t>() = // 'T = 't seq
                        if Type.(<>)(typeof<'T>, typeof<'t seq>) then
                            unsupported typeof<'T>
                        else
                            mkParserCached<'t> ctx |> mkEnumerableParser |> wrap
                }

        | Shape.FSharpUnion(:? ShapeFSharpUnion<'T> as shape) ->
            fun { RawData = rawData } ->
                match rawData with
                | SimpleData(RawValue Null) when not shape.IsStructUnion -> Unchecked.defaultof<_>
                | SimpleData(RawValue(NonNull(UnionCase shape case))) -> case.CreateUninitialized()
                | _ -> error rawData
            |> wrap

        | Shape.NotStruct _ & Shape.FSharpRecord(:? ShapeFSharpRecord<'T> as shape) ->
            let fieldSetters = shape.Fields |> Array.map mkFieldSetter

            fun parserContext ->
                let instance = shape.CreateUninitialized()

                for fieldSetter in fieldSetters do
                    fieldSetter.Invoke(parserContext, instance)

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
        let parse = mkParser<'T>()
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
