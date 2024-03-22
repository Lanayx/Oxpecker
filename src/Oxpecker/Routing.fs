namespace Oxpecker

open System
open System.Net
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
    type HttpVerbs =
        | Verbs of HttpVerb seq
        | Any

    and HttpVerb =
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

    type RouteTemplate = string
    type ConfigureEndpoint = IEndpointConventionBuilder -> IEndpointConventionBuilder
    type Endpoint =
        | SimpleEndpoint of HttpVerbs * RouteTemplate * EndpointHandler * ConfigureEndpoint
        | NestedEndpoint of RouteTemplate * Endpoint seq * ConfigureEndpoint
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

    let placeholderPattern = Regex(@"\{%([sibcdfuO])(:[^}]+)?\}")
    // This function should convert to route template and mappings
    // "api/{%s}/{%i}" -> ("api/{x}/{y}", [("x", 's', None); ("y", 'i', None)])
    // "api/{%O:guid}/{%s}" -> ("api/{x:guid}/{y}", [("x", 'O', Some "guid"); ("y", 's', None)])
    let convertToRouteTemplate (pathValue: string) (parameters: ParameterInfo[]) =
        let mutable index = 0
        let mappings = ResizeArray()

        let placeholderEvaluator = MatchEvaluator(fun m ->
            let vtype = m.Groups[1].Value[0] // First capture group is the variable type s, i, or O
            let formatSpecifier = if m.Groups[2].Success then m.Groups[2].Value else ""
            let paramName = parameters[index].Name
            index <- index + 1 // Increment index for next use
            mappings.Add(paramName, vtype, if formatSpecifier = "" then None else (Some <| formatSpecifier.TrimStart(':')))
            $"{{{paramName}{formatSpecifier}}}" // Construct the new placeholder
        )

        let newRoute = placeholderPattern.Replace(pathValue, placeholderEvaluator)
        (newRoute, mappings.ToArray()) // Convert ResizeArray to Array

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

    let tryGetParser (c: char) (modifier: string option) =
        match c with
        | 's' -> Some stringParse
        | 'i' -> Some intParse
        | 'b' -> Some boolParse
        | 'c' -> Some charParse
        | 'd' -> Some int64Parse
        | 'f' -> Some floatParse
        | 'u' -> Some uint64Parse
        | 'O' ->
            match modifier with
            | Some "guid" -> Some guidParse
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

    let rec private applyHttpVerbsToEndpoint (verbs: HttpVerbs) (endpoint: Endpoint) : Endpoint =
        match endpoint with
        | SimpleEndpoint(_, template, handler, metadata) -> SimpleEndpoint(verbs, template, handler, metadata)
        | NestedEndpoint(handler, endpoints, metadata) ->
            NestedEndpoint(handler, endpoints |> Seq.map(applyHttpVerbsToEndpoint verbs), metadata)
        | MultiEndpoint endpoints -> endpoints |> Seq.map(applyHttpVerbsToEndpoint verbs) |> MultiEndpoint



    let rec private applyHttpVerbsToEndpoints (verbs: HttpVerbs) (endpoints: Endpoint seq) : Endpoint =
        endpoints
        |> Seq.map (function
            | SimpleEndpoint(_, routeTemplate, requestDelegate, metadata) ->
                SimpleEndpoint(verbs, routeTemplate, requestDelegate, metadata)
            | NestedEndpoint(template, endpoints, metadata) ->
                NestedEndpoint(template, endpoints |> Seq.map(applyHttpVerbsToEndpoint verbs), metadata)
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


    let private invokeHandler<'T>
        (ctx: HttpContext)
        (methodInfo: MethodInfo)
        (handler: 'T)
        (mappings: (string * char * Option<_>) array)
        (parameters: ParameterInfo array)
        =
        let routeData = ctx.GetRouteData()
        let mappingArguments =
            seq {
                for mapping in mappings do
                    let placeholderName, formatChar, modifier = mapping
                    let routeValue = routeData.Values[placeholderName] |> string
                    match RequestDelegateBuilder.tryGetParser formatChar modifier with
                    | Some parseFn ->
                        try
                            parseFn routeValue
                        with :? FormatException as ex ->
                            raise
                            <| RouteParseException($"Url segment value '%s{routeValue}' has invalid format", ex)
                    | None -> routeValue
            }
        let paramCount = parameters.Length
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
        let parameters = handlerMethod.GetParameters()
        let template, mappings = RouteTemplateBuilder.convertToRouteTemplate path.Value parameters

        let requestDelegate =
            fun (ctx: HttpContext) -> invokeHandler<'T> ctx handlerMethod routeHandler mappings parameters

        SimpleEndpoint(HttpVerbs.Any, template, requestDelegate, id)

    let subRoute (path: string) (endpoints: Endpoint seq) : Endpoint =
        NestedEndpoint(path, endpoints, id)


    let inline applyBefore (beforeHandler: 'T) (endpoint: Endpoint) =
        compose_opImpl Unchecked.defaultof<ApplyBefore> beforeHandler endpoint

    let rec applyAfter (afterHandler: EndpointHandler) (endpoint: Endpoint) =
        match endpoint with
        | SimpleEndpoint(verb, template, handler, metadata) ->
            SimpleEndpoint(verb, template, handler >=> afterHandler, metadata)
        | NestedEndpoint(template, endpoints, metadata) ->
            NestedEndpoint(template, Seq.map (applyAfter afterHandler) endpoints, metadata)
        | MultiEndpoint endpoints -> MultiEndpoint(Seq.map (applyAfter afterHandler) endpoints)

    let rec configureEndpoint (f: ConfigureEndpoint) (endpoint: Endpoint) =
        match endpoint with
        | SimpleEndpoint(verb, template, handler, configureEndpoint) ->
            SimpleEndpoint(
                verb,
                template,
                handler,
                configureEndpoint >> f
            )
        | NestedEndpoint(template, endpoints, configureEndpoint) ->
            NestedEndpoint(
                template,
                endpoints,
                configureEndpoint >> f
            )
        | MultiEndpoint endpoints ->
            MultiEndpoint(Seq.map (configureEndpoint f) endpoints)

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
            configureEndpoint: ConfigureEndpoint
        ) =
        match verb with
        | Any ->
            builder
                .Map(routeTemplate, requestDelegate)
                |> configureEndpoint
        | Verbs verbs ->
            builder
                .MapMethods(routeTemplate, verbs |> Seq.map string, requestDelegate)
                |> configureEndpoint
        |> ignore

    [<Extension>]
    static member private MapNestedEndpoint
        (
            builder: IEndpointRouteBuilder,
            parentTemplate: RouteTemplate,
            endpoints: Endpoint seq,
            parentConfigureEndpoint: ConfigureEndpoint
        ) =
        let groupBuilder = builder.MapGroup(parentTemplate)
        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint(verb, template, handler, configureEndpoint) ->
                groupBuilder.MapSingleEndpoint(
                    verb,
                    template,
                    handler,
                    parentConfigureEndpoint >> configureEndpoint
                )
            | NestedEndpoint(template, endpoints, configureEndpoint) ->
                groupBuilder.MapNestedEndpoint(
                    template,
                    endpoints,
                    parentConfigureEndpoint >> configureEndpoint
                )
            | MultiEndpoint endpoints -> groupBuilder.MapMultiEndpoint endpoints

    [<Extension>]
    static member private MapMultiEndpoint(builder: IEndpointRouteBuilder, endpoints: Endpoint seq) =
        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint(verb, template, handler, configureEndpoint) ->
                builder.MapSingleEndpoint(verb, template, handler, configureEndpoint)
            | NestedEndpoint(template, endpoints, configureEndpoint) -> builder.MapNestedEndpoint(template, endpoints, configureEndpoint)
            | MultiEndpoint endpoints -> builder.MapMultiEndpoint endpoints

    [<Extension>]
    static member MapOxpeckerEndpoints(builder: IEndpointRouteBuilder, endpoints: Endpoint seq) =

        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint(verb, template, handler, configureEndpoint) ->
                builder.MapSingleEndpoint(verb, template, handler, configureEndpoint)
            | NestedEndpoint(template, endpoints, configureEndpoint) -> builder.MapNestedEndpoint(template, endpoints, configureEndpoint)
            | MultiEndpoint endpoints -> builder.MapMultiEndpoint endpoints
