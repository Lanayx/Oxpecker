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

    module Result =
        let traverse input =
            Ok []
            |> List.foldBack
                (fun v  acc ->
                    match acc, v with
                    | Ok acc, Ok v -> Ok (v :: acc)
                    | Error e, _ | _, Error e -> Error e)
                input

    let rec mkParser<'T> () : StringValues -> CultureInfo -> Result<'T, string> =
        match cache.TryFind() with
        | Some x -> x
        | None ->
            use ctx = cache.CreateGenerationContext()
            mkParserCached<'T> ctx

    and private mkParserCached<'T> (ctx: TypeGenerationContext) : StringValues -> CultureInfo -> Result<'T, string> =
        match ctx.InitOrGetCachedValue<StringValues -> CultureInfo -> Result<'T, string>>(fun c vs -> c.Value vs) with
        | Cached(value = v) -> v
        | NotCached t ->
            let v = mkParserAux<'T> ctx
            ctx.Commit t v

    and private mkParserAux<'T> (ctx: TypeGenerationContext) : StringValues -> CultureInfo -> Result<'T, string> =
        let wrap (v: StringValues -> CultureInfo -> Result<'t, string>) = unbox<StringValues -> CultureInfo -> Result<'T, string>> v 
        let typeConverter = TypeDescriptor.GetConverter(typeof<'T>);

        match shapeof<'T> with
        | Shape.String as s ->
            fun values culture ->
                values |> firstValue |> Ok
            |> wrap

        | Shape.Nullable s ->
            s.Accept { new INullableVisitor<_> with
                member _.Visit<'t when 't : (new : unit -> 't) and 't : struct and 't :> ValueType>() =
                    let parse = mkParserCached<'t> ctx
                    fun values culture ->
                        if values |> firstValue = null
                        then Ok (Nullable())
                        else parse values culture |> Result.map Nullable
                    |> wrap
            }

        | Shape.FSharpOption s ->
            s.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() =
                    let parse = mkParserCached<'t> ctx
                    fun values culture ->
                        if values |> firstValue |> isNull
                        then Ok None
                        else parse values culture |> Result.map Some
                    |> wrap
            }

        | Shape.FSharpList s ->
            s.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() =
                    let parse = mkParserCached<'t> ctx
                    fun values culture ->
                        if values |> firstValue = null then Ok List.empty
                        else
                            [ for value in values -> parse (StringValues value) culture ]
                            |> Result.traverse
                    |> wrap
            }

        | Shape.FSharpUnion (:? ShapeFSharpUnion<'T> as shape) ->
            fun values culture ->
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
                    fun (values: StringValues) culture ->
                        if values.Count = 0 then Ok [||]
                        else
                            [ for value in values -> parse (StringValues value) culture ]
                            |> Result.traverse
                            |> Result.map Array.ofList
                    |> wrap
            }

        | _ ->
            fun values culture ->
                match values |> firstValue with
                | Null -> Ok (Unchecked.defaultof<_>)
                | NonNull value ->
                    try
                        typeConverter.ConvertFromString(null, culture, value)
                        |> unbox<'T>
                        |> Ok
                    with _ -> error values

    and private cache : TypeCache = TypeCache()

    let rec internal parseModel<'T> (culture: CultureInfo) (data: IDictionary<string, StringValues>) : Result<'T, string> =
        match shapeof<'T> with
        | Shape.FSharpOption s ->
            s.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() =
                    if data.Count = 0 then
                        let value : 't option = None
                        value |> unbox<'T> |> Ok
                    else
                        parseModel<'t> culture data
                        |> Result.map Some
                        |> unbox<Result<'T, string>>
            }
        | Shape.CliMutable (:? ShapeCliMutable<'T> as s) ->
            let instance: 'T = s.CreateUninitialized()
            try
                let xs =
                    [ for prop in s.Properties ->
                        prop.Accept {
                            new IMemberVisitor<_, _> with
                                member _.Visit(propShape: ShapeMember<'T, 'TProperty>) =
                                    let parse = mkParser<'TProperty>()
                                    match data.TryGetValue(propShape.Label) with
                                    | true, values ->
                                        parse values culture
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
                                                    [
                                                        for i in 0..maxIndex do
                                                            match dicts.TryGetValue i with
                                                            | true, dict ->
                                                                parseModel<'TElement> culture dict
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

                                            parseModel<'TProperty> culture dict
                                            |> Result.map (propShape.Set instance)

                                        | _ -> Ok instance
                        }]

                Ok instance |> List.foldBack (fun v acc -> v |> Result.bind (fun _ -> acc)) xs
            with exn -> Error exn.Message
        | _ ->
            failwith "not implemented"

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
