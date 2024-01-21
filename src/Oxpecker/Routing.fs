namespace Oxpecker

open System
open System.Net
open System.Reflection
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Builder
open Microsoft.FSharp.Core
open Oxpecker

[<AutoOpen>]
module RoutingTypes =
    type HttpVerb =
        | GET
        | POST
        | PUT
        | PATCH
        | DELETE
        | HEAD
        | OPTIONS
        | TRACE
        | CONNECT
        | Any

        override this.ToString() =
            match this with
            | GET -> "GET"
            | POST -> "POST"
            | PUT -> "PUT"
            | PATCH -> "PATCH"
            | DELETE -> "DELETE"
            | HEAD -> "HEAD"
            | OPTIONS -> "OPTIONS"
            | TRACE -> "TRACE"
            | CONNECT -> "CONNECT"
            | _ -> ""

    type RouteTemplate = string
    type Metadata = obj seq

    type Endpoint =
        | SimpleEndpoint of HttpVerb * RouteTemplate * EndpointHandler * Metadata
        | NestedEndpoint of RouteTemplate * Endpoint seq * Metadata
        | MultiEndpoint of Endpoint seq


module RoutingInternal =
    type ApplyBefore =
        static member Compose(beforeHandler: EndpointHandler, endpoint: Endpoint) =
            match endpoint with
            | SimpleEndpoint(verb, template, handler, metadata) ->
                SimpleEndpoint(verb, template, beforeHandler >=> handler, metadata)
            | NestedEndpoint(template, endpoints, metadata) ->
                NestedEndpoint(template, Seq.map (fun e -> ApplyBefore.Compose(beforeHandler, e)) endpoints, metadata)
            | MultiEndpoint endpoints ->
                MultiEndpoint(Seq.map (fun e -> ApplyBefore.Compose(beforeHandler, e)) endpoints)

        static member Compose(beforeMiddleware: EndpointMiddleware, endpoint: Endpoint) =
            match endpoint with
            | SimpleEndpoint(verb, template, handler, metadata) ->
                SimpleEndpoint(verb, template, beforeMiddleware >=> handler, metadata)
            | NestedEndpoint(template, endpoints, metadata) ->
                NestedEndpoint(
                    template,
                    Seq.map (fun e -> ApplyBefore.Compose(beforeMiddleware, e)) endpoints,
                    metadata
                )
            | MultiEndpoint endpoints ->
                MultiEndpoint(Seq.map (fun e -> ApplyBefore.Compose(beforeMiddleware, e)) endpoints)

module private RouteTemplateBuilder =

    let convertToRouteTemplate (pathValue: string) =
        let rec convert (i: int) (chars: char list) =
            match chars with
            | '%' :: '%' :: tail ->
                let template, mappings = convert i tail
                "%" + template, mappings
            | '%' :: c :: tail ->
                let template, mappings = convert (i + 1) tail
                let placeholderName = $"{c}{i}"
                placeholderName + template, (placeholderName, c) :: mappings
            | c :: tail ->
                let template, mappings = convert i tail
                c.ToString() + template, mappings
            | [] -> "", []

        pathValue |> List.ofSeq |> convert 0

module private RequestDelegateBuilder =
    // Kestrel has made the weird decision to
    // partially decode a route argument, which
    // means that a given route argument would get
    // entirely URL decoded except for '%2F' (/).
    // Hence decoding %2F must happen separately as
    // part of the string parsing function.
    //
    // For more information please check:
    // https://github.com/aspnet/Mvc/issues/4599
    let stringParse (s: string) =
        s.Replace("%2F", "/", StringComparison.OrdinalIgnoreCase) |> box
    let intParse (s: string) = int s |> box
    let boolParse (s: string) = bool.Parse s |> box
    let charParse (s: string) = char s[0] |> box
    let int64Parse (s: string) = int64 s |> box
    let floatParse (s: string) = float s |> box
    let uint64Parse (s: string) = uint64 s |> box
    let guidParse (s: string) = Guid.Parse s |> box

    let tryGetParser (c: char) (endpoint: RouteEndpoint) (placeholderName: string) =
        match c with
        | 's' -> Some stringParse
        | 'i' -> Some intParse
        | 'b' -> Some boolParse
        | 'c' -> Some charParse
        | 'd' -> Some int64Parse
        | 'f' -> Some floatParse
        | 'u' -> Some uint64Parse
        | 'O' ->
            match endpoint.RoutePattern.ParameterPolicies.TryGetValue(placeholderName) with
            | true, policyReference ->
                match policyReference[0].Content with
                | "guid" -> Some guidParse
                | _ -> None
            | _ -> None
        | _ -> None

    let private handleResult (result: HttpContext option) (ctx: HttpContext) =
        match result with
        | None -> ctx.SetStatusCode(int HttpStatusCode.UnprocessableEntity)
        | Some _ -> ()


[<AutoOpen>]
module Routers =
    open CoreInternal
    open RoutingInternal

    let rec private applyHttpVerbToEndpoint (verb: HttpVerb) (endpoint: Endpoint) : Endpoint =
        match endpoint with
        | SimpleEndpoint(_, template, handler, metadata) -> SimpleEndpoint(verb, template, handler, metadata)
        | NestedEndpoint(handler, endpoints, metadata) ->
            NestedEndpoint(handler, endpoints |> Seq.map(applyHttpVerbToEndpoint verb), metadata)
        | MultiEndpoint endpoints -> endpoints |> Seq.map(applyHttpVerbToEndpoint verb) |> MultiEndpoint

    let rec private applyHttpVerbToEndpoints (verb: HttpVerb) (endpoints: Endpoint seq) : Endpoint =
        endpoints
        |> Seq.map(fun endpoint ->
            match endpoint with
            | SimpleEndpoint(_, template, handler, metadata) -> SimpleEndpoint(verb, template, handler, metadata)
            | NestedEndpoint(template, endpoints, metadata) ->
                NestedEndpoint(template, endpoints |> Seq.map(applyHttpVerbToEndpoint verb), metadata)
            | MultiEndpoint endpoints -> applyHttpVerbToEndpoints verb endpoints)
        |> MultiEndpoint

    let rec private applyHttpVerbsToEndpoints (verbs: HttpVerb seq) (endpoints: Endpoint seq) : Endpoint =
        endpoints
        |> Seq.map (function
            | SimpleEndpoint(_, routeTemplate, requestDelegate, metadata) ->
                verbs
                |> Seq.map(fun verb -> SimpleEndpoint(verb, routeTemplate, requestDelegate, metadata))
                |> MultiEndpoint
            | NestedEndpoint(template, endpoints, metadata) ->
                verbs
                |> Seq.map(fun verb ->
                    NestedEndpoint(template, endpoints |> Seq.map(applyHttpVerbToEndpoint verb), metadata))
                |> MultiEndpoint
            | MultiEndpoint endpoints ->
                verbs
                |> Seq.map(fun verb -> applyHttpVerbToEndpoints verb endpoints)
                |> MultiEndpoint)
        |> MultiEndpoint

    let GET_HEAD: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints [ GET; HEAD ]

    let GET: Endpoint seq -> Endpoint = applyHttpVerbToEndpoints GET
    let POST: Endpoint seq -> Endpoint = applyHttpVerbToEndpoints POST
    let PUT: Endpoint seq -> Endpoint = applyHttpVerbToEndpoints PUT
    let PATCH: Endpoint seq -> Endpoint = applyHttpVerbToEndpoints PATCH
    let DELETE: Endpoint seq -> Endpoint = applyHttpVerbToEndpoints DELETE
    let HEAD: Endpoint seq -> Endpoint = applyHttpVerbToEndpoints HEAD
    let OPTIONS: Endpoint seq -> Endpoint = applyHttpVerbToEndpoints OPTIONS
    let TRACE: Endpoint seq -> Endpoint = applyHttpVerbToEndpoints TRACE
    let CONNECT: Endpoint seq -> Endpoint = applyHttpVerbToEndpoints CONNECT

    let route (path: string) (handler: EndpointHandler) : Endpoint =
        SimpleEndpoint(HttpVerb.Any, path, handler, Seq.empty)


    let private invokeHandler<'T>
        (ctx: HttpContext)
        (methodInfo: MethodInfo)
        (handler: 'T)
        (mappings: (string * char) array)
        =
        let routeData = ctx.GetRouteData()
        let endpointData = ctx.GetEndpoint() :?> RouteEndpoint
        let mappingArguments =
            seq {
                for mapping in mappings do
                    let placeholderName, formatChar = mapping
                    let routeValue = routeData.Values[placeholderName] |> string
                    match RequestDelegateBuilder.tryGetParser formatChar endpointData placeholderName with
                    | Some parseFn ->
                        try
                            parseFn routeValue
                        with :? FormatException as ex ->
                            raise
                            <| RouteParseException($"Url segment value '%s{routeValue}' has invalid format", ex)
                    | None -> routeValue
            }
        let paramCount = methodInfo.GetParameters().Length
        if paramCount = mappings.Length + 1 then
            methodInfo.Invoke(handler, [| yield! mappingArguments; ctx |]) :?> Task
        elif paramCount = mappings.Length then
            let result =
                methodInfo.Invoke(handler, [| yield! mappingArguments |]) :?> FSharpFunc<HttpContext, Task>
            result ctx
        else
            failwith "Unsupported"

    let routef (path: PrintfFormat<'T, unit, unit, EndpointHandler>) (routeHandler: 'T) : Endpoint =
        let handlerType = routeHandler.GetType()
        let handlerMethod = handlerType.GetMethods()[0]
        let template, mappings = RouteTemplateBuilder.convertToRouteTemplate path.Value
        let arrMappings = mappings |> List.toArray

        let requestDelegate =
            fun (ctx: HttpContext) -> invokeHandler<'T> ctx handlerMethod routeHandler arrMappings

        SimpleEndpoint(HttpVerb.Any, template, requestDelegate, Seq.empty)

    let subRoute (path: string) (endpoints: Endpoint seq) : Endpoint =
        NestedEndpoint(path, endpoints, Seq.empty)


    let inline applyBefore (beforeHandler: 'T) (endpoint: Endpoint) =
        compose_opImpl Unchecked.defaultof<ApplyBefore> beforeHandler endpoint

    let rec applyAfter (afterHandler: EndpointHandler) (endpoint: Endpoint) =
        match endpoint with
        | SimpleEndpoint(verb, template, handler, metadata) ->
            SimpleEndpoint(verb, template, handler >=> afterHandler, metadata)
        | NestedEndpoint(template, endpoints, metadata) ->
            NestedEndpoint(template, Seq.map (applyAfter afterHandler) endpoints, metadata)
        | MultiEndpoint endpoints -> MultiEndpoint(Seq.map (applyAfter afterHandler) endpoints)

    let rec addMetadata (newMetadata: obj) (endpoint: Endpoint) =
        match endpoint with
        | SimpleEndpoint(verb, template, handler, metadata) ->
            SimpleEndpoint(
                verb,
                template,
                handler,
                seq {
                    yield! metadata
                    newMetadata
                }
            )
        | NestedEndpoint(template, endpoints, metadata) ->
            NestedEndpoint(
                template,
                endpoints,
                seq {
                    yield! metadata
                    newMetadata
                }
            )
        | MultiEndpoint endpoints -> MultiEndpoint(Seq.map (addMetadata newMetadata) endpoints)

type EndpointRouteBuilderExtensions() =

    [<Extension>]
    static member private MapSingleEndpoint
        (
            builder: IEndpointRouteBuilder,
            verb: HttpVerb,
            routeTemplate: RouteTemplate,
            requestDelegate: RequestDelegate,
            metadata: Metadata
        ) =
        match verb with
        | Any -> builder.Map(routeTemplate, requestDelegate).WithMetadata(metadata)
        | _ ->
            builder
                .MapMethods(routeTemplate, [| verb.ToString() |], requestDelegate)
                .WithMetadata(metadata)
        |> ignore

    [<Extension>]
    static member private MapNestedEndpoint
        (
            builder: IEndpointRouteBuilder,
            parentTemplate: RouteTemplate,
            endpoints: Endpoint seq,
            parentMetadata: Metadata
        ) =
        let groupBuilder = builder.MapGroup(parentTemplate)
        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint(verb, template, handler, metadata) ->
                groupBuilder.MapSingleEndpoint(
                    verb,
                    template,
                    handler,
                    seq {
                        yield! parentMetadata
                        yield! metadata
                    }
                )
            | NestedEndpoint(template, endpoints, metadata) ->
                groupBuilder.MapNestedEndpoint(
                    template,
                    endpoints,
                    seq {
                        yield! parentMetadata
                        yield! metadata
                    }
                )
            | MultiEndpoint endpoints -> groupBuilder.MapMultiEndpoint endpoints

    [<Extension>]
    static member private MapMultiEndpoint(builder: IEndpointRouteBuilder, endpoints: Endpoint seq) =
        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint(verb, template, handler, metadata) ->
                builder.MapSingleEndpoint(verb, template, handler, metadata)
            | NestedEndpoint(template, endpoints, metadata) -> builder.MapNestedEndpoint(template, endpoints, metadata)
            | MultiEndpoint endpoints -> builder.MapMultiEndpoint endpoints

    [<Extension>]
    static member MapOxpeckerEndpoints(builder: IEndpointRouteBuilder, endpoints: Endpoint seq) =

        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint(verb, template, handler, metadata) ->
                builder.MapSingleEndpoint(verb, template, handler, metadata)
            | NestedEndpoint(template, endpoints, metadata) -> builder.MapNestedEndpoint(template, endpoints, metadata)
            | MultiEndpoint endpoints -> builder.MapMultiEndpoint endpoints
