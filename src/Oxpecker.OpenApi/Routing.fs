namespace Oxpecker.OpenApi

open System.Reflection
open System.Threading.Tasks
open Microsoft.OpenApi

[<AutoOpen>]
module Routing =

    open Microsoft.AspNetCore.Builder
    open Oxpecker

    let private getSchema (c: char) (modifier: string option) =
        match c with
        | 's' -> OpenApiSchema(Type = JsonSchemaType.String)
        | 'i' -> OpenApiSchema(Type = JsonSchemaType.Integer, Format = "int32")
        | 'b' -> OpenApiSchema(Type = JsonSchemaType.Boolean)
        | 'c' -> OpenApiSchema(Type = JsonSchemaType.String)
        | 'd' -> OpenApiSchema(Type = JsonSchemaType.Integer, Format = "int64")
        | 'f' -> OpenApiSchema(Type = JsonSchemaType.Number, Format = "double")
        | 'u' -> OpenApiSchema(Type = JsonSchemaType.Integer, Format = "int64")
        | 'O' ->
            match modifier with
            | Some "guid" -> OpenApiSchema(Type = JsonSchemaType.String, Format = "uuid")
            | _ -> OpenApiSchema(Type = JsonSchemaType.String)
        | _ -> OpenApiSchema(Type = JsonSchemaType.String)

    let routef (path: PrintfFormat<'T, unit, unit, EndpointHandler>) (routeHandler: 'T) : Endpoint =
        let template, mappings, requestDelegate =
            RoutingInternal.routefInner path routeHandler
        let configureEndpoint =
            fun (endpoint: IEndpointConventionBuilder) ->
                endpoint.AddOpenApiOperationTransformer(fun operation context ct ->
                    operation.Parameters <-
                        ResizeArray(
                            mappings
                            |> Array.map(fun (name, format, modifier) ->
                                OpenApiParameter(
                                    Name = name,
                                    In = ParameterLocation.Path,
                                    Required = true,
                                    Style = ParameterStyle.Simple,
                                    Schema = getSchema format modifier
                                )
                                :> IOpenApiParameter)
                        )
                    Task.CompletedTask)
        SimpleEndpoint(HttpVerbs.Any, template, requestDelegate, configureEndpoint)

    let addOpenApi (config: OpenApiConfig) = configureEndpoint config.Build

    let addOpenApiSimple<'Req, 'Res> =
        let methodName =
            match typeof<'Req>, typeof<'Res> with
            | reqType, respType when reqType = unitType && respType = unitType -> "InvokeUnit"
            | reqType, _ when reqType = unitType -> "InvokeUnitReq"
            | _, respType when respType = unitType -> "InvokeUnitResp"
            | _, _ -> "Invoke"
        configureEndpoint
            _.WithMetadata(
                typeof<FakeFunc<'Req, 'Res>>.GetMethod(methodName, BindingFlags.Instance ||| BindingFlags.NonPublic)
                |> nullArgCheck $"Method {methodName} not found"
            )
