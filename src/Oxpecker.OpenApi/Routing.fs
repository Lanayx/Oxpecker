namespace Oxpecker.OpenApi

open System
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

    // Replaces only path parameters with the same names, so that subRoutef (group) and routef (endpoint) parameters are combined
    let private addPathParameters
        (mappings: (string * char * string option) array)
        (builder: IEndpointConventionBuilder)
        =
        builder.AddOpenApiOperationTransformer(fun operation context ct ->
            let parameters = ResizeArray<IOpenApiParameter>()
            match operation.Parameters with
            | null -> ()
            | existing ->
                for parameter in existing do
                    let isReplaced =
                        parameter.In = Nullable ParameterLocation.Path
                        && mappings
                           |> Array.exists(fun (name, _, _) -> String.Equals(name, parameter.Name))
                    if not isReplaced then
                        parameters.Add parameter
            for name, format, modifier in mappings do
                parameters.Add(
                    OpenApiParameter(
                        Name = name,
                        In = ParameterLocation.Path,
                        Required = true,
                        Style = ParameterStyle.Simple,
                        Schema = getSchema format modifier
                    )
                )
            operation.Parameters <- parameters
            Task.CompletedTask)

    let routef (path: PrintfFormat<'T, unit, unit, EndpointHandler>) (routeHandler: 'T) : Endpoint =
        let template, mappings, requestDelegate = RoutingInternal.routefInner path routeHandler
        SimpleEndpoint(HttpVerbs.Any, template, requestDelegate, addPathParameters mappings)

    let subRoutef<'T, 'Endpoints when 'Endpoints :> Endpoint seq>
        (path: PrintfFormat<'T, unit, unit, 'Endpoints>)
        (endpointsFactory: 'T)
        : Endpoint =
        let template, mappings, endpoints = RoutingInternal.subRoutefInner path endpointsFactory
        let configureEndpoint =
            if mappings.Length = 0 then
                id
            else
                addPathParameters mappings
        NestedEndpoint(template, endpoints, configureEndpoint)

    let addOpenApi (config: OpenApiConfig) = configureEndpoint config.Build

    let addOpenApiSimple<'Req, 'Res> =
        let reqType = typeof<'Req>
        let resType = typeof<'Res>
        if reqType <> unitType && resType <> unitType then
            OpenApiConfig(requestBody = RequestBody(typeof<'Req>), responseBodies = [| ResponseBody(typeof<'Res>) |])
        elif reqType <> unitType then
            OpenApiConfig(requestBody = RequestBody(typeof<'Req>))
        elif resType <> unitType then
            OpenApiConfig(responseBodies = [| ResponseBody(typeof<'Res>) |])
        else
            OpenApiConfig()
        |> addOpenApi
