namespace Oxpecker.OpenApi

open System.Reflection

[<AutoOpen>]
module Routing =

    open Microsoft.AspNetCore.Builder
    open Microsoft.OpenApi.Models
    open Oxpecker

    let private getSchema (c: char) (modifier: string option) =
        match c with
        | 's' -> OpenApiSchema(Type = "string")
        | 'i' -> OpenApiSchema(Type = "integer", Format = "int32")
        | 'b' -> OpenApiSchema(Type = "boolean")
        | 'c' -> OpenApiSchema(Type = "string")
        | 'd' -> OpenApiSchema(Type = "integer", Format = "int64")
        | 'f' -> OpenApiSchema(Type = "number", Format = "double")
        | 'u' -> OpenApiSchema(Type = "integer", Format = "int64")
        | 'O' ->
            match modifier with
            | Some "guid" -> OpenApiSchema(Type = "string", Format = "uuid")
            | _ -> OpenApiSchema(Type = "string")
        | _ -> OpenApiSchema(Type = "string")

    let routef (path: PrintfFormat<'T, unit, unit, EndpointHandler>) (routeHandler: 'T) : Endpoint =
        let template, mappings, requestDelegate =
            RoutingInternal.routefInner path routeHandler
        let configureEndpoint =
            fun (endpoint: IEndpointConventionBuilder) ->
                endpoint.WithOpenApi(fun (operation: OpenApiOperation) ->
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
                                ))
                        )
                    operation)
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
            )
                .WithOpenApi()
