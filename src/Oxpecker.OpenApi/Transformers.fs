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
            let combined = LanguagePrimitives.EnumOfValue((int t.Value) ||| (int JsonSchemaType.Null))
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

    /// Generic type definition of the built-in System.Text.Json converter (.NET 11+) that
    /// serializes F# unions in the format this transformer describes. The converter is internal,
    /// so it is identified by its defining assembly and exact generic type definition.
    let private stjUnionConverterName = "System.Text.Json.Serialization.Converters.FSharpUnionConverter`1"

    /// True when the type is an F# union serialized by the built-in System.Text.Json
    /// FSharpUnionConverter. Matching the exact runtime converter keeps the transformer inert
    /// on custom converters (e.g. FSharp.SystemTextJson, or a user converter whose name merely
    /// starts with FSharpUnionConverter), on the specially-handled FSharp.Core types
    /// (option, voption and list have their own converters) and on older runtimes.
    let isUnionSerializedBySTJ (typeInfo: JsonTypeInfo) =
        let converterType = typeInfo.Converter.GetType()
        FSharpType.IsUnion typeInfo.Type
        && converterType.IsGenericType
        && converterType.Assembly = typeof<JsonConverter>.Assembly
        && converterType.GetGenericTypeDefinition().FullName = stjUnionConverterName

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

    /// The schema pipeline marks nullable properties with this metadata key and wraps them in
    /// oneOf [null, schema] when it resolves references.
    let private nullablePropertyKey = "x-is-nullable-property"

    /// Marks schemas already shaped by the union transformer, or wrapped around such a schema,
    /// so that a second visit through the property recursion of the schema pipeline (single-case
    /// unions expose their fields as properties of the union type) leaves them intact.
    let private transformedKey = "x-fsharp-union-schema"

    let private getMetadata (schema: OpenApiSchema) =
        match schema.Metadata with
        | null ->
            let dict = Dictionary<string, obj>() :> IDictionary<string, obj>
            schema.Metadata <- dict
            dict
        | m -> m

    let markTransformed (schema: OpenApiSchema) =
        (getMetadata schema)[transformedKey] <- (true :> obj)

    let isTransformed (schema: OpenApiSchema) =
        match schema.Metadata with
        | null -> false
        | m -> m.ContainsKey transformedKey

    let clearNullableProperty (schema: OpenApiSchema) =
        match schema.Metadata with
        | null -> ()
        | m -> m.Remove nullablePropertyKey |> ignore

    /// Unions marked UseNullAsTrueValue (exactly one nullary case, like option) represent that
    /// case as CLR null, which System.Text.Json writes and reads as JSON null instead of the case name.
    let usesNullAsTrueValue (unionType: Type) =
        match unionType.GetCustomAttribute<CompilationRepresentationAttribute>() with
        | null -> false
        | attr -> attr.Flags.HasFlag CompilationRepresentationFlags.UseNullAsTrueValue

    let private hasNullType (schema: IOpenApiSchema) =
        schema.Type.HasValue && schema.Type.Value.HasFlag JsonSchemaType.Null

    let private hasNullBranch (branches: IList<IOpenApiSchema> | null) =
        match branches with
        | null -> false
        | branches -> branches |> Seq.exists hasNullType

    /// True when the schema already accepts JSON null.
    let admitsNull (schema: OpenApiSchema) =
        hasNullType schema || hasNullBranch schema.OneOf || hasNullBranch schema.AnyOf

    /// Enum, const and composed schemas cannot be made nullable by adding null to their type.
    let isComposite (schema: OpenApiSchema) =
        not(isNull schema.Enum)
        || not(isNull schema.Const)
        || not(isNull schema.OneOf)
        || not(isNull schema.AnyOf)
        || not(isNull schema.AllOf)

    /// Makes the schema of innerType accept JSON null, the way System.Text.Json serializes None
    /// and null: simple inline schemas get null added to their type, references and composite
    /// schemas are wrapped in oneOf [null, schema]. A schema that already admits null (such as a
    /// UseNullAsTrueValue union) is returned as it is, since wrapping it would make both oneOf
    /// alternatives match null and reject it.
    let makeNullable (innerType: Type) (schema: OpenApiSchema) : IOpenApiSchema =
        if admitsNull schema || usesNullAsTrueValue innerType then
            schema
        elif (tryGetRefSchema schema).IsSome || isComposite schema then
            let items = ResizeArray<IOpenApiSchema>()
            items.Add nullSchema
            items.Add schema
            let wrapper = OpenApiSchema(OneOf = items)
            markTransformed wrapper
            wrapper
        else
            schema.Type <- unionWithNull schema.Type
            schema

    /// Nullable reference annotation (e.g. string | null) on a union case field, read the same
    /// way JsonSchemaExporter reads it for record properties.
    let isNullableReference (field: PropertyInfo) =
        not field.PropertyType.IsValueType
        && NullabilityInfoContext().Create(field).ReadState = NullabilityState.Nullable

    /// Schema for a union case field: option/voption fields and nullable reference fields
    /// become nullable schemas.
    let getFieldSchema (ctx: OpenApiSchemaTransformerContext) (field: PropertyInfo) (ct: CancellationToken) =
        task {
            let nullableInnerType =
                match field.PropertyType with
                | FSharpOptionKind innerType -> Some innerType
                | fieldType when isNullableReference field -> Some fieldType
                | _ -> None
            match nullableInnerType with
            | Some innerType ->
                let! innerSchema = ctx.GetOrCreateSchemaAsync(innerType, null, ct)
                return makeNullable innerType innerSchema
            | None ->
                let! schema = ctx.GetOrCreateSchemaAsync(field.PropertyType, null, ct)
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
    /// Field names follow the runtime converter's precedence: JsonPropertyNameAttribute, then
    /// PropertyNamingPolicy, then the raw name. F# cannot attach attributes to union case fields yet
    /// (fsharp/fslang-suggestions#684), so the attribute branch only matters once the language
    /// allows it. The same applies to the JsonConverter and JsonNumberHandling attributes the
    /// converter honours per field, which this transformer does not reflect in field schemas.
    let getFieldName ctx (field: PropertyInfo) =
        match field.GetCustomAttribute<JsonPropertyNameAttribute>() with
        | null -> convertName ctx field.Name
        | attr -> attr.Name

    /// Whether object branches may carry unknown properties: the runtime converter rejects them
    /// under JsonUnmappedMemberHandling.Disallow (type attribute, then serializer options),
    /// which JsonSchemaExporter documents for records with additionalProperties: false.
    let allowsAdditionalProperties (typeInfo: JsonTypeInfo) =
        let handling =
            if typeInfo.UnmappedMemberHandling.HasValue then
                typeInfo.UnmappedMemberHandling.Value
            else
                typeInfo.Options.UnmappedMemberHandling
        handling <> JsonUnmappedMemberHandling.Disallow

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
            let nullAsTrueValue = usesNullAsTrueValue unionType
            let additionalPropertiesAllowed = allowsAdditionalProperties ctx.JsonTypeInfo

            // Reset whatever the default exporter produced from the compiler-generated
            // union members (Tag, IsCase properties etc.) before applying the union shape.
            schema.Type <- Nullable()
            schema.Properties <- null
            schema.Required <- null
            schema.Enum <- null
            schema.AdditionalPropertiesAllowed <- true

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
                        if nullAsTrueValue then
                            // The only nullary case of a UseNullAsTrueValue union is serialized as JSON null.
                            branches.Add(OpenApiSchema(Type = Nullable JsonSchemaType.Null))
                        else
                            // Fieldless case is serialized as a plain string: a const branch per case.
                            branches.Add(
                                OpenApiSchema(Type = Nullable JsonSchemaType.String, Const = getCaseName ctx case)
                            )
                    else
                        // Case with fields is serialized as an object with a type discriminator property.
                        let properties = Dictionary<string, IOpenApiSchema>()
                        let required = HashSet<string>()
                        properties[discriminatorPropertyName] <-
                            OpenApiSchema(Type = Nullable JsonSchemaType.String, Const = getCaseName ctx case)
                        required.Add discriminatorPropertyName |> ignore
                        for field in fields do
                            let fieldName = getFieldName ctx field
                            let! fieldSchema = getFieldSchema ctx field ct
                            properties[fieldName] <- fieldSchema
                            required.Add fieldName |> ignore
                        branches.Add(
                            OpenApiSchema(
                                Type = Nullable JsonSchemaType.Object,
                                Properties = properties,
                                Required = required,
                                AdditionalPropertiesAllowed = additionalPropertiesAllowed
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
                        schema.AdditionalPropertiesAllowed <- branch.AdditionalPropertiesAllowed
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
                            match Helpers.tryGetRefSchema innerSchema with
                            | None -> propSchema |> Helpers.copyMetadata innerSchema "" true
                            | Some refSchema -> propSchema |> Helpers.copyMetadata innerSchema refSchema false
                            props[key] <- Helpers.makeNullable innerType innerSchema
                | _ -> ()
            }
            :> Task

/// Generates OpenAPI schemas for F# discriminated unions matching the built-in
/// System.Text.Json serialization format introduced in .NET 11:
/// fieldless cases are serialized as JSON strings, cases with fields as JSON objects
/// with a type discriminator property ("$type" by default, customizable via
/// JsonPolymorphicAttribute) and named field properties. The nullary case of a union
/// marked UseNullAsTrueValue is the exception: it is serialized as JSON null, not a string.
type FSharpUnionSchemaTransformer() =
    interface IOpenApiSchemaTransformer with
        member _.TransformAsync
            (schema: OpenApiSchema, ctx: OpenApiSchemaTransformerContext, ct: CancellationToken)
            : Task =
            task {
                let unionType = ctx.JsonTypeInfo.Type
                if
                    Helpers.isUnionSerializedBySTJ ctx.JsonTypeInfo
                    && not(Helpers.isTransformed schema)
                then
                    let inFlightUnions = Helpers.getInFlightUnions()
                    // For recursive unions, a nested occurrence of the type keeps its original
                    // schema, which the schema pipeline resolves into a component reference.
                    if inFlightUnions.Add unionType then
                        try
                            do! Helpers.transformUnionSchema schema ctx ct
                            Helpers.markTransformed schema
                            // The schema pipeline wraps nullable properties in oneOf [null, schema] when it
                            // resolves references. A union whose nullary case is null already admits null,
                            // so such a wrapper would make both alternatives match null and reject it.
                            if Helpers.usesNullAsTrueValue unionType then
                                Helpers.clearNullableProperty schema
                        finally
                            inFlightUnions.Remove unionType |> ignore
            }
            :> Task
