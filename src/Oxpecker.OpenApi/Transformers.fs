namespace Oxpecker.OpenApi

open System
open System.Collections.Generic
open System.Reflection
open System.Text.Json.Nodes
open System.Text.Json.Serialization
open System.Text.Json.Serialization.Metadata
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.OpenApi
open Microsoft.FSharp.Reflection
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

    /// True when the type is an F# union serialized by the built-in System.Text.Json
    /// FSharpUnionConverter (.NET 11+): a union that is not one of the specially-handled
    /// FSharp.Core types (option, voption, list). The converter check keeps the transformer
    /// inert on custom converters (e.g. FSharp.SystemTextJson) and older runtimes.
    let isUnionSerializedBySTJ (typeInfo: JsonTypeInfo) =
        let t = typeInfo.Type
        FSharpType.IsUnion t
        && not(
            t.IsGenericType
            && (let gtd = t.GetGenericTypeDefinition()
                gtd = typedefof<option<_>>
                || gtd = typedefof<ValueOption<_>>
                || gtd = typedefof<list<_>>)
        )
        && typeInfo.Converter.GetType().Name.StartsWith("FSharpUnionConverter", StringComparison.Ordinal)

    /// Tracks union types currently being transformed on this async flow,
    /// so that recursive unions don't cause infinite recursion.
    let private inFlightUnions = AsyncLocal<HashSet<Type> | null>()

    let getInFlightUnions () =
        match inFlightUnions.Value with
        | null ->
            let set = HashSet<Type>()
            inFlightUnions.Value <- set
            set
        | set -> set

    /// Schema for a union case field, unwrapping option/voption fields into nullable schemas.
    let getFieldSchema (ctx: OpenApiSchemaTransformerContext) (fieldType: Type) (ct: CancellationToken) = task {
        match fieldType with
        | FSharpOptionKind innerType ->
            let! innerSchema = ctx.GetOrCreateSchemaAsync(innerType, null, ct)
            match tryGetRefSchema innerSchema with
            | None ->
                innerSchema.Type <- unionWithNull innerSchema.Type
                return innerSchema :> IOpenApiSchema
            | Some _ ->
                let items = ResizeArray<IOpenApiSchema>()
                items.Add nullSchema
                items.Add innerSchema
                return OpenApiSchema(OneOf = items) :> IOpenApiSchema
        | _ ->
            let! schema = ctx.GetOrCreateSchemaAsync(fieldType, null, ct)
            return schema :> IOpenApiSchema
    }

    let convertName (ctx: OpenApiSchemaTransformerContext) name =
        match ctx.JsonTypeInfo.Options.PropertyNamingPolicy with
        | null -> name
        | policy -> policy.ConvertName name
    let getCaseName ctx (case: UnionCaseInfo) =
        case.GetCustomAttributes typeof<JsonPropertyNameAttribute>
        |> Array.tryPick(function
            | :? JsonPropertyNameAttribute as attr -> Some attr.Name
            | _ -> None)
        |> Option.defaultWith(fun () -> convertName ctx case.Name)
    let getFieldName ctx (field: PropertyInfo) =
        match field.GetCustomAttribute<JsonPropertyNameAttribute>() with
        | null -> convertName ctx field.Name
        | attr -> attr.Name

    let transformUnionSchema
        (schema: OpenApiSchema)
        (ctx: OpenApiSchemaTransformerContext)
        (ct: CancellationToken)
        : Task<unit> =
        task {
            let unionType = ctx.JsonTypeInfo.Type
            let discriminatorPropertyName =
                match unionType.GetCustomAttribute<JsonPolymorphicAttribute>() with
                | null -> "$type"
                | attr ->
                    match attr.TypeDiscriminatorPropertyName with
                    | null -> "$type"
                    | name -> name
            let casesWithFields =
                FSharpType.GetUnionCases unionType
                |> Array.map(fun case -> case, case.GetFields())

            // Reset whatever the default exporter produced from the compiler-generated
            // union members (Tag, IsCase properties etc.) before applying the union shape.
            schema.Type <- Nullable()
            schema.Properties <- null
            schema.Required <- null
            schema.Enum <- null

            if casesWithFields |> Array.forall(fun (_, fields) -> fields.Length = 0) then
                // Enum-like union: every case is serialized as a plain string.
                let enumValues = ResizeArray<JsonNode>()
                for case, _ in casesWithFields do
                    enumValues.Add(JsonValue.Create(getCaseName ctx case) |> Unchecked.nonNull)
                schema.Type <- Nullable JsonSchemaType.String
                schema.Enum <- enumValues
            else
                let branches = ResizeArray<IOpenApiSchema>()
                for case, fields in casesWithFields do
                    if fields.Length = 0 then
                        // Fieldless case is serialized as a plain string: a const branch per case.
                        branches.Add(OpenApiSchema(Type = Nullable JsonSchemaType.String, Const = getCaseName ctx case))
                    else
                        // Case with fields is serialized as an object with a type discriminator property.
                        let properties = Dictionary<string, IOpenApiSchema>()
                        let required = HashSet<string>()
                        properties[discriminatorPropertyName] <-
                            OpenApiSchema(Type = Nullable JsonSchemaType.String, Const = getCaseName ctx case)
                        required.Add discriminatorPropertyName |> ignore
                        for field in fields do
                            let fieldName = getFieldName ctx field
                            let! fieldSchema = getFieldSchema ctx field.PropertyType ct
                            properties[fieldName] <- fieldSchema
                            required.Add fieldName |> ignore
                        branches.Add(
                            OpenApiSchema(
                                Type = Nullable JsonSchemaType.Object,
                                Properties = properties,
                                Required = required
                            )
                        )

                match branches with
                | single when single.Count = 1 ->
                    // Union with a single case with fields: inline the object shape.
                    match single[0] with
                    | :? OpenApiSchema as branch ->
                        schema.Type <- branch.Type
                        schema.Properties <- branch.Properties
                        schema.Required <- branch.Required
                    | _ -> ()
                | many -> schema.OneOf <- many
        }

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

/// Generates OpenAPI schemas for F# discriminated unions matching the built-in
/// System.Text.Json serialization format introduced in .NET 11:
/// fieldless cases are serialized as JSON strings, cases with fields as JSON objects
/// with a type discriminator property ("$type" by default, customizable via
/// JsonPolymorphicAttribute) and named field properties.
type FSharpUnionSchemaTransformer() =
    interface IOpenApiSchemaTransformer with
        member _.TransformAsync
            (schema: OpenApiSchema, ctx: OpenApiSchemaTransformerContext, ct: CancellationToken)
            : Task =
            task {
                let unionType = ctx.JsonTypeInfo.Type
                if Helpers.isUnionSerializedBySTJ ctx.JsonTypeInfo then
                    let inFlightUnions = Helpers.getInFlightUnions()
                    // For recursive unions, a nested occurrence of the type keeps its original
                    // schema, which the schema pipeline resolves into a component reference.
                    if inFlightUnions.Add unionType then
                        try
                            do! Helpers.transformUnionSchema schema ctx ct
                        finally
                            inFlightUnions.Remove unionType |> ignore
            }
            :> Task
