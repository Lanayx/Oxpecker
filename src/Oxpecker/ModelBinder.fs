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

module StringValues =
    let toDict(v: StringValues) = dict ["", v]

module String =
    let toDict(v: string | null) = StringValues v |> StringValues.toDict

module Result =
    let traverse input =
        Ok []
        |> List.foldBack
            (fun v  acc ->
                match acc, v with
                | Ok acc, Ok v -> Ok (v :: acc)
                | Error e, _ | _, Error e -> Error e)
            input

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


    let (|ExactMatch|_|) (propName: string) (data: IDictionary<string, StringValues>) =
        match data.TryGetValue(propName) with
        | true, values-> Some (StringValues.toDict values)
        | false, _ -> None

    let (|PrefixMatch|_|) (propName: string) (data: IDictionary<string, StringValues>) =
        let data' =
            data
            |> Seq.choose (fun (KeyValue (key, value)) ->
                if not <| key.StartsWith(propName) then None else

                // For example, when the property is 'Foo':
                // - 'Foo.Bar' becomes 'Bar' (trim the starting '.').
                // - 'Foo[0].Bar' becomes '[0].Bar' (no trimming needed).
                let key' = key.[propName.Length..].TrimStart('.')
                Some (key', value))
            |> dict

        if data'.Count > 0 then Some data' else None
        

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
        | Shape.String ->
            fun (RawValueQuick values) _ ->
                values |> firstValue |> Ok
            |> wrap

        | Shape.Nullable shape ->
            shape.Accept { new INullableVisitor<_> with
                member _.Visit<'t when 't : (new : unit -> 't) and 't : struct and 't :> ValueType>() = // 'T = Nullable<'t>
                    let parse = mkParserCached<'t> ctx

                    fun (RawValueQuick values) culture ->
                        if values |> firstValue = null
                        then Ok (Nullable())
                        else parse (StringValues.toDict values) culture |> Result.map Nullable
                    |> wrap
            }

        | Shape.FSharpOption shape ->
            shape.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() = // 'T = option<'t>
                    let parse = mkParserCached<'t> ctx

                    fun data culture ->
                        match data with
                        | Empty -> Ok None
                        | RawValue values when values |> firstValue |> isNull ->
                            Ok None
                        | _ ->  parse data culture |> Result.map Some
                    |> wrap
            }

        | Shape.FSharpList shape ->
            shape.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() = // 'T = 't list
                    let parse = mkParserCached<'t> ctx
                    fun (RawValueQuick values) culture ->
                        if values |> firstValue = null then Ok List.empty
                        else
                            [ for value in values -> parse (String.toDict value) culture ]
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

        | Shape.Array shape when shape.Rank = 1 ->
            shape.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() = // 'T = 't array
                    let parse = mkParserCached<'t> ctx

                    fun data culture ->
                        match data with
                        | RawValue values ->
                            if values.Count = 0 then Ok [||]
                            else
                                [ for value in values -> parse (String.toDict value) culture ]
                                |> Result.traverse
                                |> Result.map List.toArray

                        | _ ->
                            let regex = "\[(\d+)\]\.(\w+)" |> Regex

                            let values =
                                data
                                |> Seq.choose (fun item ->
                                    match regex.Match(item.Key) with
                                        | m when m.Success ->
                                            let index = int m.Groups.[1].Value
                                            let key = m.Groups.[2].Value
                                            Some (index, key, item.Value)
                                        | _ -> None)

                            let dicts =
                                values
                                |> Seq.groupBy(fun (index, _, _) -> index)
                                |> Seq.map (fun (i, items) ->
                                    i, items |> Seq.map (fun (_, k, v) -> k, v) |> dict)
                                |> dict

                            let maxIndex = Seq.max dicts.Keys

                            [
                                for i in 0..maxIndex do
                                    match dicts.TryGetValue i with
                                    | true, dict ->
                                        parse dict culture
                                    | _ ->
                                        Ok Unchecked.defaultof<_>
                            ]
                            |> Result.traverse
                            |> Result.map List.toArray
                    |> wrap
            }

        | Shape.CliMutable (:? ShapeCliMutable<'T> as shape) ->
            fun data culture ->
                let instance = shape.CreateUninitialized()

                [ for prop in shape.Properties ->
                    prop.Accept {
                        new IMemberVisitor<_, _> with
                            member _.Visit<'TProperty>(propShape) =
                                let parse = mkParser<'TProperty>()

                                match data with
                                | ExactMatch propShape.Label data'
                                | PrefixMatch propShape.Label data' ->
                                    parse data' culture |> Result.map (propShape.Set instance)

                                | _ -> Ok instance
                    }] |> List.tryFind _.IsError |> Option.defaultValue (Ok instance)

        | _ ->
            fun (RawValueQuick values) culture ->
                match values |> firstValue with
                | Null -> Ok Unchecked.defaultof<_>
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
