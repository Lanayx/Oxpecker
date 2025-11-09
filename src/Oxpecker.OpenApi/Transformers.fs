namespace Oxpecker.OpenApi

open System
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.OpenApi
open Microsoft.OpenApi
open FSharp.Control
open type Microsoft.AspNetCore.Http.TypedResults


// ---------- F# helpers ----------
module private FSharpTypeChecks =
    let (|FSharpOptionKind|_|) (t: Type) =
        if t.IsGenericType then
            let gtd = t.GetGenericTypeDefinition()
            if gtd = typedefof<option<_>> || gtd = typedefof<ValueOption<_>> then
                Some(t.GetGenericArguments()[0])
            else
                None
        else
            None

module private SchemaCopy =

    /// Shallowly "adopt" the public, settable surface from `src` into `dst`.
    /// We intentionally avoid touching internals; only copy what's publicly available.
    let copyTo (dst: OpenApiSchema) (src: IOpenApiSchema) =
        // Json-Schema identity & metadata
        dst.Title <- src.Title
        dst.Schema <- src.Schema
        dst.Id <- src.Id
        dst.Comment <- src.Comment
        dst.Vocabulary <-
            match src.Vocabulary with
            | null -> null
            | v -> Dictionary(v)
        dst.DynamicRef <- src.DynamicRef
        dst.DynamicAnchor <- src.DynamicAnchor
        dst.Definitions <-
            match src.Definitions with
            | null -> null
            | d -> Dictionary(d)
        // Numeric/string constraints
        dst.ExclusiveMaximum <- src.ExclusiveMaximum
        dst.ExclusiveMinimum <- src.ExclusiveMinimum
        dst.Maximum <- src.Maximum
        dst.Minimum <- src.Minimum
        dst.MultipleOf <- src.MultipleOf
        dst.MaxLength <- src.MaxLength
        dst.MinLength <- src.MinLength
        dst.Pattern <- src.Pattern

        // Type/format & const/default
        dst.Type <- src.Type
        dst.Format <- src.Format
        dst.Const <- src.Const
        dst.Default <- src.Default

        // Read/Write/Deprecated
        dst.ReadOnly <- src.ReadOnly
        dst.WriteOnly <- src.WriteOnly
        dst.Deprecated <- src.Deprecated

        // Compositions & negation
        dst.AllOf <-
            match src.AllOf with
            | null -> null
            | a -> ResizeArray(a)
        dst.AnyOf <-
            match src.AnyOf with
            | null -> null
            | a -> ResizeArray(a)
        dst.OneOf <-
            match src.OneOf with
            | null -> null
            | a -> ResizeArray(a)
        dst.Not <- src.Not

        // Array/object facets
        dst.Items <- src.Items
        dst.MaxItems <- src.MaxItems
        dst.MinItems <- src.MinItems
        dst.UniqueItems <- src.UniqueItems

        dst.Properties <-
            match src.Properties with
            | null -> null
            | p -> Dictionary(p)
        dst.PatternProperties <-
            match src.PatternProperties with
            | null -> null
            | p -> Dictionary(p)
        dst.MaxProperties <- src.MaxProperties
        dst.MinProperties <- src.MinProperties
        dst.Required <-
            match src.Required with
            | null -> null
            | r -> HashSet(r)
        dst.AdditionalPropertiesAllowed <- src.AdditionalPropertiesAllowed
        dst.AdditionalProperties <- src.AdditionalProperties

        // Misc
        dst.Discriminator <- src.Discriminator
        dst.Description <- src.Description
        dst.Example <- src.Example
        dst.Examples <-
            match src.Examples with
            | null -> null
            | e -> ResizeArray(e)
        dst.Enum <-
            match src.Enum with
            | null -> null
            | e -> ResizeArray(e)
        dst.UnevaluatedProperties <- src.UnevaluatedProperties
        dst.ExternalDocs <- src.ExternalDocs
        dst.Xml <- src.Xml
        dst.Extensions <-
            match src.Extensions with
            | null -> null
            | e -> Dictionary(e)
        dst.UnrecognizedKeywords <-
            match src.UnrecognizedKeywords with
            | null -> null
            | e -> Dictionary(e)
        dst.Metadata <-
            if src :? IMetadataContainer then
                (src :?> IMetadataContainer).Metadata
            else
                null
        dst.DependentRequired <-
            match src.DependentRequired with
            | null -> null
            | d -> Dictionary(d)

    /// Union an existing JsonSchemaType with `null` (OpenAPI 3.1), and also drives `nullable: true` for 3.0.
    let unionWithNull (t: Nullable<JsonSchemaType>) : Nullable<JsonSchemaType> =
        if t.HasValue then
            let combined =
                LanguagePrimitives.EnumOfValue((int t.Value) ||| (int JsonSchemaType.Null))
            Nullable<JsonSchemaType>(combined)
        else
            // Leave as null; writer will omit 'type'. (Optional: add x-nullable via Extensions if you need 3.0 on typeless schemas.)
            Nullable()

// ---------- Transformers ----------

/// 1) Map F# option/valueoption to the **inner T** and mark it nullable.
///    Works with the vNext `Microsoft.OpenApi.OpenApiSchema` model you pasted.
type FSharpOptionSchemaTransformer() =
    interface IOpenApiSchemaTransformer with
        member _.TransformAsync
            (schema: OpenApiSchema, ctx: OpenApiSchemaTransformerContext, ct: CancellationToken)
            : Task =
            task {
                match ctx.JsonTypeInfo.Type with
                | FSharpTypeChecks.FSharpOptionKind innerT ->
                    // Ask pipeline for T's schema …
                    let! inner = ctx.GetOrCreateSchemaAsync(innerT, null, ct)
                    // … copy its shape …
                    inner |> SchemaCopy.copyTo schema
                    // … and mark nullable (OAS 3.0 => "nullable: true"; OAS 3.1 => type union with "null")
                    schema.Type <- SchemaCopy.unionWithNull schema.Type
                | _ -> ()
            }
            :> Task

/// 2) On object schemas, ensure option-backed properties are **NOT in `required`**.
///    We use CLR metadata only — no dependency on child schema concrete types.
// type FSharpOptionRequiredPruner() =
//     interface IOpenApiSchemaTransformer with
//         member _.TransformAsync(schema: OpenApiSchema, ctx: OpenApiSchemaTransformerContext, _ct: CancellationToken) : Task =
//             task {
//                 match ctx.JsonTypeInfo.Kind, schema.Required with
//                 | JsonTypeInfoKind.Object, NonNull s when s.Count > 0 ->
//                     for p in ctx.JsonTypeInfo.Properties do
//                         match p.PropertyType with
//                         | FSharpTypeChecks.FSharpOptionKind _ ->
//                             s.Remove(p.Name) |> ignore
//                         | _ ->
//                             ()
//                 | _ ->
//                     ()
//             } :> Task
