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

/// <summary>
/// Module for parsing models from a generic data set.
/// </summary>
module internal ModelParser =

    open System.ComponentModel
    open System.Text.RegularExpressions
    open TypeShape.Core
    open TypeShape.Core.Utils

    let firstValue (rawValues: StringValues) = if rawValues.Count = 0 then null else rawValues[0]

    let error (values: StringValues) : Result<'T, string> =
        let value =
            match values |> firstValue with
            | null -> "null"
            | _ -> values.ToString()
        Error $"Could not parse value '{value}' to type '{typeof<'T>}'."

    let unionCaseExists caseName (case: ShapeFSharpUnionCase<'T>) =
        String.Equals( case.CaseInfo.Name, caseName, StringComparison.OrdinalIgnoreCase)

    let (|UnionCase|_|) (shape: ShapeFSharpUnion<'T>) (caseName: string) =
        shape.UnionCases |> Array.tryFind (unionCaseExists caseName)

    let (|Empty|_|) (dict: IDictionary<string, StringValues>) =
        dict.Count = 0

    let (|RawValue|_|) (dict: IDictionary<string, StringValues>) =
        if dict.Count = 1 then
            dict |> Seq.head |> _.Value |> Some
        else
            None

    let (|RawValueQuick|) (dict: IDictionary<string, StringValues>) =
        match dict with
        | RawValue v -> v
        | _ -> failwith ""


    module Result =
        let traverse input =
            Ok []
            |> List.foldBack
                (fun v  acc ->
                    match acc, v with
                    | Ok acc, Ok v -> Ok (v :: acc)
                    | Error e, _ | _, Error e -> Error e)
                input

    let dictionary values = dict ["", values]

    let rec mkParser<'T> () : IDictionary<string, StringValues> -> CultureInfo -> Result<'T, string> =
        match cache.TryFind() with
        | Some x -> x
        | None ->
            use ctx = cache.CreateGenerationContext()
            mkParserCached<'T> ctx

    and private mkParserCached<'T> (ctx: TypeGenerationContext) : IDictionary<string, StringValues> -> CultureInfo -> Result<'T, string> =
        match ctx.InitOrGetCachedValue<IDictionary<string, StringValues> -> CultureInfo -> Result<'T, string>>(fun c vs -> c.Value vs) with
        | Cached(value = v) -> v
        | NotCached t ->
            let v = mkParserAux<'T> ctx
            ctx.Commit t v

    and private mkParserAux<'T> (ctx: TypeGenerationContext) : IDictionary<string, StringValues> -> CultureInfo -> Result<'T, string> =
        let wrap (v: IDictionary<string, StringValues> -> CultureInfo -> Result<'t, string>) = unbox<IDictionary<string, StringValues> -> CultureInfo -> Result<'T, string>> v 
        let typeConverter = TypeDescriptor.GetConverter(typeof<'T>);

        match shapeof<'T> with
        | Shape.String as s ->
            fun (RawValueQuick values) _ ->
                values |> firstValue |> Ok
            |> wrap

        | Shape.Nullable s ->
            s.Accept { new INullableVisitor<_> with
                member _.Visit<'t when 't : (new : unit -> 't) and 't : struct and 't :> ValueType>() =
                    let parse = mkParserCached<'t> ctx
                    fun (RawValueQuick values) culture ->
                        if values |> firstValue = null
                        then Ok (Nullable())
                        else parse (dictionary values) culture |> Result.map Nullable
                    |> wrap
            }

        | Shape.FSharpOption s ->
            s.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() =
                    let parse = mkParserCached<'t> ctx
                    fun dict culture ->
                        match dict with
                        | Empty -> Ok None
                        | RawValue values when values |> firstValue |> isNull ->
                            Ok None
                        | _ ->  parse dict culture |> Result.map Some
                    |> wrap
            }

        | Shape.FSharpList s ->
            s.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() =
                    let parse = mkParserCached<'t> ctx
                    fun (RawValueQuick values) culture ->
                        if values |> firstValue = null then Ok List.empty
                        else
                            [ for value in values -> parse (dictionary (StringValues value)) culture ]
                            |> Result.traverse
                    |> wrap
            }

        | Shape.FSharpUnion (:? ShapeFSharpUnion<'T> as shape) ->
            fun (RawValueQuick values) culture ->
                match values |> firstValue with
                | NonNull (UnionCase shape case) ->
                    Ok (case.CreateUninitialized())

                | null when not shape.IsStructUnion ->
                    Ok Unchecked.defaultof<_>

                | _ ->
                    error values
            |> wrap

        | Shape.Array s when s.Rank = 1 ->
            s.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() =
                    let parse = mkParserCached<'t> ctx
                    fun (RawValueQuick values) culture ->
                        if values.Count = 0 then Ok [||]
                        else
                            [ for value in values -> parse (dictionary (StringValues value)) culture ]
                            |> Result.traverse
                            |> Result.map Array.ofList
                    |> wrap
            }

        | Shape.CliMutable (:? ShapeCliMutable<'T> as s) ->
            fun data culture ->
                let instance = s.CreateUninitialized()
                try
                    let xs =
                        [ for prop in s.Properties ->
                            prop.Accept {
                                new IMemberVisitor<_, _> with
                                    member _.Visit(propShape: ShapeMember<'T, 'TProperty>) =
                                        let parseProp = mkParser<'TProperty>()
                                        match data.TryGetValue(propShape.Label) with
                                            | true, values ->
                                                parseProp (dictionary values) culture
                                                |> Result.map (propShape.Set instance)

                                            | false, _ ->
                                                match shapeof<'TProperty> with
                                                | Shape.Array s when s.Rank = 1 ->
                                                    let regex = propShape.Label |> Regex.Escape |> sprintf @"%s\[(\d+)\]\.(\w+)" |> Regex
                                                    let values =
                                                        seq {
                                                            for item in data do
                                                                match regex.Match(item.Key) with
                                                                | m when m.Success ->
                                                                    let index = int m.Groups.[1].Value
                                                                    let key = m.Groups.[2].Value
                                                                    Some (index, key, item.Value)
                                                                | _ -> None
                                                        }
                                                        |> Seq.choose id

                                                    let dicts =
                                                        values
                                                        |> Seq.groupBy(fun (index, _, _) -> index)
                                                        |> Seq.map (fun (i, items) ->
                                                            i, items |> Seq.map (fun (_, k, v) -> k, v) |> dict)
                                                        |> dict

                                                    let maxIndex = Seq.max dicts.Keys 
                                                    s.Element.Accept { new ITypeVisitor<_> with
                                                        member _.Visit<'TElement>() =
                                                            let parseElem = mkParser<'TElement>()
                                                            [
                                                                for i in 0..maxIndex do
                                                                    match dicts.TryGetValue i with
                                                                    | true, dict ->
                                                                        parseElem dict culture
                                                                    | _ ->
                                                                        Ok Unchecked.defaultof<_>
                                                            ]
                                                            |> Result.traverse
                                                            |> Result.map List.toArray
                                                            |> unbox<Result<'TProperty, string>>
                                                    }
                                                    |> Result.map (propShape.Set instance)

                                                | Shape.CliMutable _ ->
                                                    let regex = propShape.Label |> Regex.Escape |> sprintf @"%s\.(\w+)" |> Regex
                                                    let dict =
                                                        seq {
                                                            for item in data do
                                                                match regex.Match(item.Key) with
                                                                | m when m.Success ->
                                                                    let key = m.Groups.[1].Value
                                                                    Some (key, item.Value)
                                                                | _ -> None
                                                        }
                                                        |> Seq.choose id
                                                        |> dict

                                                    parseProp dict culture
                                                    |> Result.map (propShape.Set instance)

                                                | _ -> Ok instance
                            }]

                    Ok instance |> List.foldBack (fun v acc -> v |> Result.bind (fun _ -> acc)) xs
                with exn -> Error exn.Message

        | _ ->
            fun (RawValueQuick values) culture ->
                match values |> firstValue with
                | Null -> Ok (Unchecked.defaultof<_>)
                | NonNull value ->
                    try
                        typeConverter.ConvertFromString(null, culture, value)
                        |> unbox<'T>
                        |> Ok
                    with _ -> error values

    and private cache : TypeCache = TypeCache()

    let rec internal parseModel<'T> =
        let parse = mkParser<'T>()
        fun (culture: CultureInfo) (data: IDictionary<string, StringValues>) ->
            parse data culture

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
            match ModelParser.parseModel<'T> options.CultureInfo (Dictionary data) with
            | Ok value -> value
            | Error msg -> failwith msg
