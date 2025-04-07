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

module internal StringValues =
    let toDict(v: StringValues) = dict ["", v]

module internal String =
    let toDict(v: string | null) = StringValues v |> StringValues.toDict

/// <summary>
/// Module for parsing models from a generic data set.
/// </summary>
module internal ModelParser =

    open System.ComponentModel
    open System.Text.RegularExpressions
    open TypeShape.Core
    open TypeShape.Core.Utils

    let private firstValue (rawValues: StringValues) = if rawValues.Count = 0 then null else rawValues[0]

    let private error (values: StringValues) : 'T =
        let value =
            match values |> firstValue with
            | null -> "null"
            | _ -> values.ToString()

        failwith $"Could not parse value '{value}' to type '{typeof<'T>}'."

    let private unionCaseExists caseName (case: ShapeFSharpUnionCase<'T>) =
        String.Equals( case.CaseInfo.Name, caseName, StringComparison.OrdinalIgnoreCase)

    let private (|UnionCase|_|) (shape: ShapeFSharpUnion<'T>) (caseName: string) =
        shape.UnionCases |> Array.tryFind (unionCaseExists caseName)

    let private (|RawValue|_|) (dict: IDictionary<string, StringValues>) =
        match dict |> Seq.tryExactlyOne with
        | Some (KeyValue ("", value))-> Some value
        | _ -> None

    let private (|RawValueQuick|) (dict: IDictionary<string, StringValues>) =
        match dict with
        | RawValue v -> v
        | _ -> failwith $"Dictionary %A{dict} should contain only one value but it has %i{dict.Count}."

    let private (|SimpleArray|_|) (data: IDictionary<string, StringValues>) =
        match data with
        | RawValue values when values.Count > 0 -> values |> Seq.map String.toDict |> Some
        | _ -> None

    let private (|ComplexArray|_|)  (data: IDictionary<string, StringValues>) =
        let regex = "\[(\d+)\]\.(.+)" |> Regex

        let values =
            data
            |> Seq.choose (fun item ->
                match regex.Match(item.Key) with
                | m when m.Success ->
                    let index = int m.Groups.[1].Value
                    let key = m.Groups.[2].Value
                    Some (index, key, item.Value)
                | _ -> None)

        let data' =
            values
            |> Seq.groupBy(fun (index, _, _) -> index)
            |> Seq.map (fun (i, items) ->
                i, items |> Seq.map (fun (_, k, v) -> k, v) |> dict)
            |> dict

        if data'.Count = 0 then None else

        Some data'

    let private (|ExactMatch|_|) (key: string) (data: IDictionary<string, StringValues>) =
        match data.TryGetValue(key) with
        | true, values-> Some (StringValues.toDict values)
        | false, _ -> None

    let private (|PrefixMatch|_|) (prefix: string) (data: IDictionary<string, StringValues>) =
        let matchedData =
            data
            |> Seq.choose (fun (KeyValue (key, value)) ->
                if not <| key.StartsWith(prefix) then None else

                let matchedKey = key.[prefix.Length..]

                if matchedKey.StartsWith '.' then
                    Some (matchedKey.[1..], value)
                elif matchedKey.StartsWith '[' then
                    Some (matchedKey, value)
                else
                    None)

            |> dict

        if matchedData.Count > 0 then Some matchedData else None

    type private FieldSetter<'T> = delegate of CultureInfo * IDictionary<string, StringValues> * 'T -> unit

    type private Parser<'T> = CultureInfo -> IDictionary<string, StringValues> -> 'T

    let rec private mkParser<'T> () : Parser<'T> =
        match cache.TryFind() with
        | Some x -> x
        | None ->
            use ctx = cache.CreateGenerationContext()
            mkParserCached<'T> ctx

    and private mkParserCached<'T> (ctx: TypeGenerationContext) : Parser<'T> =
        match ctx.InitOrGetCachedValue<CultureInfo -> IDictionary<string, StringValues> -> 'T>(fun c vs -> c.Value vs) with
        | Cached(value = v) -> v
        | NotCached t ->
            let v = mkParserAux<'T> ctx
            ctx.Commit t v

    and private mkParserAux<'T> (ctx: TypeGenerationContext) : Parser<'T> =
        let wrap (v: Parser<'t>) = unbox<Parser<'T>> v 
        let typeConverter = TypeDescriptor.GetConverter(typeof<'T>);

        let mkFieldSetter (shape : IShapeMember<'DeclaringType>) =
            shape.Accept { new IMemberVisitor<_, _> with
                member _.Visit<'Field>(fieldShape) =
                    let parse = mkParserCached<'Field> ctx

                    FieldSetter (fun culture data instance -> 
                        match data with
                        | ExactMatch fieldShape.Label data'
                        | PrefixMatch fieldShape.Label data' ->
                            parse culture data' |> fieldShape.Set instance |> ignore
                        | _ -> ())
            }

        match shapeof<'T> with
        | Shape.String ->
            fun _ (RawValueQuick values) ->
                values |> firstValue
            |> wrap

        | Shape.Nullable shape ->
            shape.Accept { new INullableVisitor<_> with
                member _.Visit<'t when 't : (new : unit -> 't) and 't : struct and 't :> ValueType>() = // 'T = Nullable<'t>
                    let parse = mkParserCached<'t> ctx

                    fun culture (RawValueQuick values) ->
                        if values |> firstValue |> isNull then Nullable() else

                        parse culture (StringValues.toDict values) |> Nullable
                    |> wrap
            }

        | Shape.FSharpOption shape ->
            shape.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() = // 'T = option<'t>
                    let parse = mkParserCached<'t> ctx

                    fun culture data ->
                        match data with
                        | RawValue values when values |> firstValue |> isNull ->
                            None
                        | _ ->  parse culture data |> Some
                    |> wrap
            }

        | Shape.FSharpList shape ->
            shape.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() = // 'T = 't list
                    let parse = mkParserCached<'t> ctx

                    fun culture (RawValueQuick values) ->
                        if values |> firstValue |> isNull then [] else

                        [ for value in values -> parse culture (String.toDict value) ]
                    |> wrap
            }

        | Shape.FSharpUnion (:? ShapeFSharpUnion<'T> as shape) ->
            fun _ (RawValueQuick values) ->
                match values |> firstValue with
                | NonNull (UnionCase shape case) ->
                    case.CreateUninitialized()

                | Null when not shape.IsStructUnion ->
                    Unchecked.defaultof<_>

                | _ ->
                    error values
            |> wrap

        | Shape.Array shape when shape.Rank = 1 ->
            shape.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() = // 'T = 't array
                    let parse = mkParserCached<'t> ctx

                    fun culture data ->
                        match data with
                        | SimpleArray dicts ->
                            [| for dict in dicts -> parse culture dict |]

                        | ComplexArray indexedDicts ->
                            let maxIndex = Seq.max indexedDicts.Keys

                            [|
                                for i in 0..maxIndex ->
                                    match indexedDicts.TryGetValue i with
                                    | true, dict ->
                                        parse culture dict
                                    | _ ->
                                        Unchecked.defaultof<_>
                            |]

                        | _ -> [||]
                    |> wrap
            }

        | Shape.FSharpRecord (:? ShapeFSharpRecord<'T> as shape) ->
                let fieldSetters = shape.Fields |> Array.map mkFieldSetter

                fun culture data ->
                    let instance = shape.CreateUninitialized()
                    
                    for fieldSetter in fieldSetters do
                        fieldSetter.Invoke(culture, data, instance)

                    instance

        | shape ->
            fun culture (RawValueQuick values) ->
                match values |> firstValue with
                | NonNull value ->
                    try
                        typeConverter.ConvertFromString(null, culture, value) |> unbox
                    with _ ->
                        error values

                | Null when not shape.Type.IsValueType  -> Unchecked.defaultof<_>

                | _ -> error values

    and private cache : TypeCache = TypeCache()

    let rec internal parseModel<'T> =
        let parse = mkParser<'T>()
        fun (culture: CultureInfo) (data: IDictionary<string, StringValues>) ->
            parse culture data

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
            ModelParser.parseModel<'T> options.CultureInfo (Dictionary data)
