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

/// <summary>
/// Module for parsing models from a generic data set.
/// </summary>
module internal ModelParser =

    open System.ComponentModel
    open System.Text.RegularExpressions
    open TypeShape.Core
    open TypeShape.Core.Utils

    let firstValue (rawValues: StringValues) = if rawValues.Count = 0 then null else rawValues[0]

    let error (values: StringValues) : 'T =
        let value =
            match values |> firstValue with
            | null -> "null"
            | _ -> values.ToString()

        failwith $"Could not parse value '{value}' to type '{typeof<'T>}'."

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

    let (|SimpleArray|_|) (data: IDictionary<string, StringValues>) =
        match data with
        | RawValue values when values.Count > 0 -> values |> Seq.map String.toDict |> Some
        | _ -> None

    let (|ComplexArray|_|)  (data: IDictionary<string, StringValues>) =
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

        let data' =
            values
            |> Seq.groupBy(fun (index, _, _) -> index)
            |> Seq.map (fun (i, items) ->
                i, items |> Seq.map (fun (_, k, v) -> k, v) |> dict)
            |> dict

        if data'.Count = 0 then None else

        let maxIndex = Seq.max data'.Keys
        Some (maxIndex, data')

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

    type PropertySetter<'T> = delegate of CultureInfo * IDictionary<string, StringValues> * 'T -> unit

    let rec mkParser<'T> () : CultureInfo -> IDictionary<string, StringValues> -> 'T =
        match cache.TryFind() with
        | Some x -> x
        | None ->
            use ctx = cache.CreateGenerationContext()
            mkParserCached<'T> ctx

    and private mkParserCached<'T> (ctx: TypeGenerationContext) : CultureInfo -> IDictionary<string, StringValues> -> 'T =
        match ctx.InitOrGetCachedValue<CultureInfo -> IDictionary<string, StringValues> -> 'T>(fun c vs -> c.Value vs) with
        | Cached(value = v) -> v
        | NotCached t ->
            let v = mkParserAux<'T> ctx
            ctx.Commit t v

    and private mkParserAux<'T> (ctx: TypeGenerationContext) : CultureInfo -> IDictionary<string, StringValues> -> 'T =
        let wrap (v: CultureInfo -> IDictionary<string, StringValues> -> 't) = unbox<CultureInfo -> IDictionary<string, StringValues> -> 'T> v 
        let typeConverter = TypeDescriptor.GetConverter(typeof<'T>);

        let mkPropertySetter (shape : IShapeMember<'DeclaringType>) =
            shape.Accept { new IMemberVisitor<_, _> with
                member _.Visit<'TProperty>(propShape) =
                    let parse = mkParser<'TProperty>()

                    PropertySetter (fun culture data instance -> 
                        match data with
                        | ExactMatch propShape.Label data'
                        | PrefixMatch propShape.Label data' ->
                            parse culture data' |> propShape.Set instance |> ignore
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
                        if values |> firstValue = null then Nullable() else

                        parse culture (StringValues.toDict values) |> Nullable
                    |> wrap
            }

        | Shape.FSharpOption shape ->
            shape.Element.Accept { new ITypeVisitor<_> with
                member _.Visit<'t>() = // 'T = option<'t>
                    let parse = mkParserCached<'t> ctx

                    fun culture data ->
                        match data with
                        | Empty -> None
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
                        if values |> firstValue = null then [] else

                        [ for value in values -> parse culture (String.toDict value) ]
                    |> wrap
            }

        | Shape.FSharpUnion (:? ShapeFSharpUnion<'T> as shape) ->
            fun culture (RawValueQuick values) ->
                match values |> firstValue with
                | NonNull (UnionCase shape case) ->
                    case.CreateUninitialized()

                | null when not shape.IsStructUnion ->
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
                        | SimpleArray data' ->
                            [| for dict in data' do parse culture dict |]

                        | ComplexArray (maxIndex, data') ->
                            [|
                                for i in 0..maxIndex ->
                                    match data'.TryGetValue i with
                                    | true, dict ->
                                        parse culture dict
                                    | _ ->
                                        Unchecked.defaultof<_>
                            |]

                        | _ -> [||]
                    |> wrap
            }

        | Shape.CliMutable (:? ShapeCliMutable<'T> as shape) ->
                let propertySetters = shape.Properties |> Seq.map mkPropertySetter

                fun culture data ->
                    let instance = shape.CreateUninitialized()
                    
                    for propertySetter in propertySetters do
                        propertySetter.Invoke(culture, data, instance)

                    instance

        | _ ->
            fun culture (RawValueQuick values) ->
                match values |> firstValue with
                | Null -> Unchecked.defaultof<_>
                | NonNull value ->
                    try
                        typeConverter.ConvertFromString(null, culture, value) |> unbox
                    with _ ->
                        error values

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
