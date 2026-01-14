namespace Oxpecker.OpenApi

open System
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.OpenApi
open Microsoft.OpenApi
open FSharp.Control
open type Microsoft.AspNetCore.Http.TypedResults

module private Helpers =
    let nullSchema = OpenApiSchema(Type = Nullable(JsonSchemaType.Null))

    let (|FSharpOptionKind|_|) (t: Type) =
        if t.IsGenericType then
            let gtd = t.GetGenericTypeDefinition()
            if gtd = typedefof<option<_>> || gtd = typedefof<ValueOption<_>> then
                Some(t.GetGenericArguments()[0])
            else
                None
        else
            None

    let (|OptionalProperties|_|) (ctx: OpenApiSchemaTransformerContext) =
        let result = ResizeArray()
        for propertyInfo in ctx.JsonTypeInfo.Properties do
            match propertyInfo.PropertyType with
            | FSharpOptionKind innerType -> result.Add((propertyInfo, innerType))
            | _ -> ()
        if result.Count > 0 then Some(result) else None

    /// Union an existing JsonSchemaType with `null` (OpenAPI 3.1), and also drives `nullable: true` for 3.0.
    let unionWithNull (t: Nullable<JsonSchemaType>) : Nullable<JsonSchemaType> =
        if t.HasValue then
            let combined =
                LanguagePrimitives.EnumOfValue((int t.Value) ||| (int JsonSchemaType.Null))
            Nullable<JsonSchemaType>(combined)
        else
            // Leave as null; writer will omit 'type'.
            Nullable()

    let tryGetRefSchema (schema: OpenApiSchema) =
        match schema.Metadata with
        | null -> None
        | metadata ->
            match metadata.TryGetValue("x-schema-id") with
            | true, o ->
                match o with
                | :? string as s ->
                    match String.IsNullOrEmpty s with
                    | true -> None
                    | false -> Some s
                | _ -> None
            | _ -> None

    let copyMetadata (dst: OpenApiSchema) refSchemaId isSimple (src: IOpenApiSchema) =
        if src :? IMetadataContainer then
            match (src :?> IMetadataContainer).Metadata with
            | null -> ()
            | srcMeta ->
                let dstMeta =
                    match dst.Metadata with
                    | null ->
                        let dict = Dictionary<string, obj>() :> IDictionary<string, obj>
                        dst.Metadata <- dict
                        dict
                    | m -> m
                for KeyValue(k, v) in srcMeta do
                    match k with
                    | "x-schema-id" -> dstMeta[k] <- refSchemaId
                    | "x-ref-description" ->
                        if isSimple then
                            dst.Description <- v :?> string
                        else
                            dstMeta[k] <- v
                    | _ -> ()

type FSharpOptionSchemaTransformer() =
    interface IOpenApiSchemaTransformer with
        member _.TransformAsync
            (schema: OpenApiSchema, ctx: OpenApiSchemaTransformerContext, ct: CancellationToken)
            : Task =
            task {
                match ctx with
                | Helpers.OptionalProperties props ->
                    for propInfo, innerType in props do
                        let key = propInfo.Name
                        match schema.Properties with
                        | null -> ()
                        | props when not(props.ContainsKey key) -> ()
                        | props ->
                            let propSchema = props[key]
                            let! innerSchema = ctx.GetOrCreateSchemaAsync(innerType, null, ct)
                            // If it's a reference (complex type), use oneOf [null, $ref].
                            // If it's inline (simple type), just add null to the type.
                            let newSchema =
                                match Helpers.tryGetRefSchema innerSchema with
                                | None ->
                                    propSchema |> Helpers.copyMetadata innerSchema "" true
                                    innerSchema.Type <- Helpers.unionWithNull innerSchema.Type
                                    innerSchema
                                | Some refSchema ->
                                    propSchema |> Helpers.copyMetadata innerSchema refSchema false
                                    let items = ResizeArray<IOpenApiSchema>()
                                    items.Add(Helpers.nullSchema)
                                    items.Add(innerSchema)
                                    OpenApiSchema(OneOf = items)
                            props[key] <- newSchema
                | _ -> ()
            }
            :> Task
