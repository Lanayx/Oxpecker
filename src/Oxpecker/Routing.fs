namespace Oxpecker

open System
open System.Reflection
open System.Runtime.CompilerServices
open System.Text.RegularExpressions
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Builder
open Microsoft.FSharp.Core
open Oxpecker

[<AutoOpen>]
module RoutingTypes =
    [<Struct>]
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

    type HttpVerbs =
        | Verbs of HttpVerb seq
        | Any

    type RouteTemplate = string
    type ConfigureEndpoint = IEndpointConventionBuilder -> IEndpointConventionBuilder
    type Endpoint =
        | SimpleEndpoint of HttpVerbs * RouteTemplate * EndpointHandler * ConfigureEndpoint
        | NestedEndpoint of RouteTemplate * Endpoint seq * ConfigureEndpoint
        | MultiEndpoint of Endpoint seq

module RouteTemplateBuilder =

    // Kestrel has made the weird decision to
    // partially decode a route argument, which
    // means that a given route argument would get
    // entirely URL decoded except for '%2F' (/).
    // Hence decoding %2F must happen separately as
    // part of the string parsing function.
    //
    // For more information please check:
    // https://github.com/aspnet/Mvc/issues/4599

    let inline parse (c: char) (modifier: string option) (s: string) : obj =
        try
            match c with
            | 's' -> s.Replace("%2F", "/", StringComparison.OrdinalIgnoreCase)
            | 'i' -> int s |> box
            | 'b' -> bool.Parse s |> box
            | 'c' -> char s[0] |> box
            | 'd' -> int64 s |> box
            | 'f' -> float s |> box
            | 'u' -> uint64 s |> box
            | 'O' ->
                match modifier with
                | Some "guid" -> Guid.Parse s |> box
                | _ -> s
            | _ -> s
        with :? FormatException as ex ->
            raise
            <| RouteParseException($"Url segment value '%s{s}' has invalid format", ex)

    let placeholderPattern = Regex("\{%([sibcdfuO])(:[^}]+)?\}")
    // This function should convert to route template and mappings
    // "api/{%s}/{%i}" -> ("api/{x}/{y}", [("x", 's', None); ("y", 'i', None)])
    // "api/{%O:guid}/{%s}" -> ("api/{x:guid}/{y}", [("x", 'O', Some "guid"); ("y", 's', None)])
    let convertToRouteTemplate (pathValue: string) (parameters: ParameterInfo[]) =
        let mutable index = 0
        let mappings = ResizeArray()

        let placeholderEvaluator =
            MatchEvaluator(fun m ->
                let vtype = m.Groups[1].Value[0] // First capture group is the variable type s, i, or O
                let formatSpecifier = if m.Groups[2].Success then m.Groups[2].Value else ""
                let paramName = parameters[index].Name
                index <- index + 1 // Increment index for next use
                mappings.Add(
                    paramName,
                    vtype,
                    if formatSpecifier = "" then
                        None
                    else
                        Some <| formatSpecifier.TrimStart(':')
                )
                $"{{{paramName}{formatSpecifier}}}" // Construct the new placeholder
            )

        let newRoute = placeholderPattern.Replace(pathValue, placeholderEvaluator)
        (newRoute, mappings.ToArray())

module RoutingInternal =
    type ApplyBefore =
        static member Compose(beforeHandler: EndpointHandler, endpoint: Endpoint) =
            match endpoint with
            | SimpleEndpoint(verb, template, handler, configure) ->
                SimpleEndpoint(verb, template, beforeHandler >=> handler, configure)
            | NestedEndpoint(template, endpoints, configure) ->
                NestedEndpoint(template, Seq.map (fun e -> ApplyBefore.Compose(beforeHandler, e)) endpoints, configure)
            | MultiEndpoint endpoints ->
                MultiEndpoint(Seq.map (fun e -> ApplyBefore.Compose(beforeHandler, e)) endpoints)

        static member Compose(beforeMiddleware: EndpointMiddleware, endpoint: Endpoint) =
            match endpoint with
            | SimpleEndpoint(verb, template, handler, configure) ->
                SimpleEndpoint(verb, template, beforeMiddleware >=> handler, configure)
            | NestedEndpoint(template, endpoints, configure) ->
                NestedEndpoint(
                    template,
                    Seq.map (fun e -> ApplyBefore.Compose(beforeMiddleware, e)) endpoints,
                    configure
                )
            | MultiEndpoint endpoints ->
                MultiEndpoint(Seq.map (fun e -> ApplyBefore.Compose(beforeMiddleware, e)) endpoints)

    let invokeHandler<'T>
        (ctx: HttpContext)
        (methodInfo: MethodInfo)
        (handler: 'T)
        (mappings: (string * char * Option<_>) array)
        (ctxInParameterList: bool)
        =
        let routeData = ctx.GetRouteData()
        if ctxInParameterList then
            methodInfo.Invoke(
                handler,
                [|
                    for placeholderName, formatChar, modifier in mappings do
                        let routeValue = routeData.Values[placeholderName] |> string
                        RouteTemplateBuilder.parse formatChar modifier routeValue
                    ctx
                |]
            )
            :?> Task
        else
            methodInfo.Invoke(
                handler,
                [|
                    for placeholderName, formatChar, modifier in mappings do
                        let routeValue = routeData.Values[placeholderName] |> string
                        RouteTemplateBuilder.parse formatChar modifier routeValue
                |]
            )
            :?> FSharpFunc<HttpContext, Task>
            <| ctx

    let routefInner (path: PrintfFormat<'T, unit, unit, EndpointHandler>) (routeHandler: 'T) =
        let handlerType = routeHandler.GetType()
        let handlerMethod = handlerType.GetMethods()[0]
        let parameters = handlerMethod.GetParameters()
        let template, mappings =
            RouteTemplateBuilder.convertToRouteTemplate path.Value parameters
        let ctxInParameterList =
            if parameters.Length = mappings.Length + 1 then true
            elif parameters.Length = mappings.Length then false
            else failwith <| "Unsupported routef handler: " + path.Value

        let requestDelegate =
            fun (ctx: HttpContext) -> invokeHandler<'T> ctx handlerMethod routeHandler mappings ctxInParameterList

        template, mappings, requestDelegate


[<AutoOpen>]
module Routers =
    open CoreInternal
    open RoutingInternal

    let rec applyHttpVerbsToEndpoint (verbs: HttpVerbs) (endpoint: Endpoint) : Endpoint =
        match endpoint with
        | SimpleEndpoint(_, template, handler, configure) -> SimpleEndpoint(verbs, template, handler, configure)
        | NestedEndpoint(handler, endpoints, configure) ->
            NestedEndpoint(handler, endpoints |> Seq.map(applyHttpVerbsToEndpoint verbs), configure)
        | MultiEndpoint endpoints -> endpoints |> Seq.map(applyHttpVerbsToEndpoint verbs) |> MultiEndpoint

    let rec applyHttpVerbsToEndpoints (verbs: HttpVerbs) (endpoints: Endpoint seq) : Endpoint =
        endpoints
        |> Seq.map (function
            | SimpleEndpoint(_, routeTemplate, requestDelegate, configure) ->
                SimpleEndpoint(verbs, routeTemplate, requestDelegate, configure)
            | NestedEndpoint(template, endpoints, configure) ->
                NestedEndpoint(template, endpoints |> Seq.map(applyHttpVerbsToEndpoint verbs), configure)
            | MultiEndpoint endpoints -> MultiEndpoint(endpoints |> Seq.map(applyHttpVerbsToEndpoint verbs)))
        |> MultiEndpoint

    let GET_HEAD: Endpoint seq -> Endpoint =
        applyHttpVerbsToEndpoints(Verbs [ GET; HEAD ])

    let GET: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ GET ])
    let POST: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ POST ])
    let PUT: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ PUT ])
    let PATCH: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ PATCH ])
    let DELETE: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ DELETE ])
    let HEAD: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ HEAD ])
    let OPTIONS: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ OPTIONS ])
    let TRACE: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ TRACE ])
    let CONNECT: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ CONNECT ])

    let route (path: string) (handler: EndpointHandler) : Endpoint =
        SimpleEndpoint(HttpVerbs.Any, path, handler, id)

    let routef (path: PrintfFormat<'T, unit, unit, EndpointHandler>) (routeHandler: 'T) : Endpoint =
        let template, _, requestDelegate = routefInner path routeHandler

        SimpleEndpoint(HttpVerbs.Any, template, requestDelegate, id)

    let subRoute (path: string) (endpoints: Endpoint seq) : Endpoint = NestedEndpoint(path, endpoints, id)

    let inline applyBefore (beforeHandler: 'T) (endpoint: Endpoint) =
        compose_opImpl Unchecked.defaultof<ApplyBefore> beforeHandler endpoint

    let rec applyAfter (afterHandler: EndpointHandler) (endpoint: Endpoint) =
        match endpoint with
        | SimpleEndpoint(verb, template, handler, configure) ->
            SimpleEndpoint(verb, template, handler >=> afterHandler, configure)
        | NestedEndpoint(template, endpoints, configure) ->
            NestedEndpoint(template, Seq.map (applyAfter afterHandler) endpoints, configure)
        | MultiEndpoint endpoints -> MultiEndpoint(Seq.map (applyAfter afterHandler) endpoints)

    let rec configureEndpoint (f: ConfigureEndpoint) (endpoint: Endpoint) =
        match endpoint with
        | SimpleEndpoint(verb, template, handler, configure) -> SimpleEndpoint(verb, template, handler, configure >> f)
        | NestedEndpoint(template, endpoints, configure) -> NestedEndpoint(template, endpoints, configure >> f)
        | MultiEndpoint endpoints -> MultiEndpoint(Seq.map (configureEndpoint f) endpoints)

    let addMetadata (metadata: obj) =
        configureEndpoint _.WithMetadata(metadata)

type EndpointRouteBuilderExtensions() =

    [<Extension>]
    static member private MapSingleEndpoint
        (
            builder: IEndpointRouteBuilder,
            verb: HttpVerbs,
            routeTemplate: RouteTemplate,
            requestDelegate: RequestDelegate,
            configure: ConfigureEndpoint
        ) =
        match verb with
        | Any -> builder.Map(routeTemplate, requestDelegate) |> configure
        | Verbs verbs ->
            builder.MapMethods(routeTemplate, verbs |> Seq.map string, requestDelegate)
            |> configure
        |> ignore

    [<Extension>]
    static member private MapNestedEndpoint
        (
            builder: IEndpointRouteBuilder,
            parentTemplate: RouteTemplate,
            endpoints: Endpoint seq,
            parentConfigure: ConfigureEndpoint
        ) =
        let groupBuilder = builder.MapGroup(parentTemplate)
        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint(verb, template, handler, configure) ->
                groupBuilder.MapSingleEndpoint(verb, template, handler, parentConfigure >> configure)
            | NestedEndpoint(template, endpoints, configure) ->
                groupBuilder.MapNestedEndpoint(template, endpoints, parentConfigure >> configure)
            | MultiEndpoint endpoints -> groupBuilder.MapOxpeckerEndpoints endpoints

    [<Extension>]
    static member MapOxpeckerEndpoint(builder: IEndpointRouteBuilder, endpoint: Endpoint) =
        match endpoint with
        | SimpleEndpoint(verb, template, handler, configure) ->
            builder.MapSingleEndpoint(verb, template, handler, configure)
        | NestedEndpoint(template, endpoints, configure) -> builder.MapNestedEndpoint(template, endpoints, configure)
        | MultiEndpoint endpoints -> builder.MapOxpeckerEndpoints endpoints

    [<Extension>]
    static member MapOxpeckerEndpoints(builder: IEndpointRouteBuilder, endpoints: Endpoint seq) =
        for endpoint in endpoints do
            builder.MapOxpeckerEndpoint(endpoint)
