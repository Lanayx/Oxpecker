namespace Oxpecker.Routing

open System
open System.Net
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Builder
open Microsoft.FSharp.Core
open Oxpecker

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

        pathValue
        |> List.ofSeq
        |> convert 0

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
    let stringParse (s: string) = s.Replace("%2F", "/", StringComparison.OrdinalIgnoreCase) |> box
    let intParse (s: string) = int s |> box
    let boolParse (s: string) = bool.Parse s |> box
    let charParse (s: string) = char s[0] |> box
    let int64Parse (s: string) = int64 s |> box
    let floatParse (s: string) = float s |> box

    let tryGetParser (c: char) =
        match c with
        | 's' -> Some stringParse
        | 'i' -> Some intParse
        | 'b' -> Some boolParse
        | 'c' -> Some charParse
        | 'd' -> Some int64Parse
        | 'f' -> Some floatParse
        | _   -> None

    let private handleResult (result: HttpContext option) (ctx: HttpContext) =
        match result with
        | None   -> ctx.SetStatusCode (int HttpStatusCode.UnprocessableEntity)
        | Some _ -> ()


[<AutoOpen>]
module Routers =

    type HttpVerb =
        | GET | POST | PUT | PATCH | DELETE | HEAD | OPTIONS | TRACE | CONNECT
        | Any

        override this.ToString() =
            match this with
            | GET        -> "GET"
            | POST       -> "POST"
            | PUT        -> "PUT"
            | PATCH      -> "PATCH"
            | DELETE     -> "DELETE"
            | HEAD       -> "HEAD"
            | OPTIONS    -> "OPTIONS"
            | TRACE      -> "TRACE"
            | CONNECT    -> "CONNECT"
            | _          -> ""

    type RouteTemplate = string
    type Metadata = obj seq

    type Endpoint =
        | SimpleEndpoint   of HttpVerb * RouteTemplate * EndpointHandler * Metadata
        | NestedEndpoint   of RouteTemplate * Endpoint seq  * Metadata
        | MultiEndpoint    of Endpoint seq

    let rec private applyHttpVerbToEndpoint
        (verb    : HttpVerb)
        (endpoint: Endpoint): Endpoint =
        match endpoint with
        | SimpleEndpoint (_, template, handler, metadata) ->
            SimpleEndpoint (verb, template, handler, metadata)
        | NestedEndpoint (handler, endpoints, metadata) ->
            NestedEndpoint (
                handler,
                endpoints
                |> Seq.map (applyHttpVerbToEndpoint verb),
                metadata)
        | MultiEndpoint endpoints ->
            endpoints
            |> Seq.map(applyHttpVerbToEndpoint verb)
            |> MultiEndpoint

    let rec private applyHttpVerbToEndpoints
        (verb: HttpVerb)
        (endpoints: Endpoint seq): Endpoint =
        endpoints
        |> Seq.map(
            fun endpoint ->
                match endpoint with
                | SimpleEndpoint (_, template, handler, metadata) ->
                    SimpleEndpoint (verb, template, handler, metadata)
                | NestedEndpoint (template, endpoints, metadata) ->
                    NestedEndpoint (
                        template,
                        endpoints
                        |> Seq.map (applyHttpVerbToEndpoint verb),
                        metadata)
                | MultiEndpoint endpoints ->
                    applyHttpVerbToEndpoints verb endpoints)
        |> MultiEndpoint

    let rec private applyHttpVerbsToEndpoints
        (verbs    : HttpVerb seq)
        (endpoints: Endpoint seq): Endpoint =
        endpoints
        |> Seq.map(
            fun endpoint ->
                match endpoint with
                | SimpleEndpoint (_, routeTemplate, requestDelegate, metadata) ->
                    verbs
                    |> Seq.map(fun verb -> SimpleEndpoint (verb, routeTemplate, requestDelegate, metadata))
                    |> MultiEndpoint
                | NestedEndpoint (routeTemplate, endpoints, metadata) ->
                    verbs
                    |> Seq.map(
                        fun verb ->
                            NestedEndpoint (
                                routeTemplate,
                                endpoints
                                |> Seq.map (applyHttpVerbToEndpoint verb),
                                metadata))
                    |> MultiEndpoint
                | MultiEndpoint endpoints ->
                    verbs
                    |> Seq.map(fun verb -> applyHttpVerbToEndpoints verb endpoints)
                    |> MultiEndpoint
        ) |> MultiEndpoint

    let GET_HEAD: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints [ GET; HEAD ]

    let GET: Endpoint seq -> Endpoint     = applyHttpVerbToEndpoints GET
    let POST: Endpoint seq -> Endpoint    = applyHttpVerbToEndpoints POST
    let PUT: Endpoint seq -> Endpoint     = applyHttpVerbToEndpoints PUT
    let PATCH: Endpoint seq -> Endpoint   = applyHttpVerbToEndpoints PATCH
    let DELETE: Endpoint seq -> Endpoint  = applyHttpVerbToEndpoints DELETE
    let HEAD: Endpoint seq -> Endpoint    = applyHttpVerbToEndpoints HEAD
    let OPTIONS: Endpoint seq -> Endpoint = applyHttpVerbToEndpoints OPTIONS
    let TRACE: Endpoint seq -> Endpoint   = applyHttpVerbToEndpoints TRACE
    let CONNECT: Endpoint seq -> Endpoint = applyHttpVerbToEndpoints CONNECT

    let route
        (path    : string)
        (handler : EndpointHandler): Endpoint =
        SimpleEndpoint (HttpVerb.Any, path, handler, Seq.empty)

    exception RouteParseException of string

    let routef
        (path        : PrintfFormat<'T,unit,unit, EndpointHandler>)
        (routeHandler: 'T): Endpoint =
        let handlerType = routeHandler.GetType()
        let handlerMethod = handlerType.GetMethods()[0]
        let template, mappings = RouteTemplateBuilder.convertToRouteTemplate path.Value
        let arrMappings = mappings |> List.toArray

        let requestDelegate =
            fun (ctx: HttpContext) ->
                let routeData = ctx.GetRouteData()
                try
                    handlerMethod.Invoke(routeHandler, [|
                        for mapping in arrMappings do
                            let placeholderName, formatChar = mapping
                            let routeValue = routeData.Values[placeholderName] |> string
                            match RequestDelegateBuilder.tryGetParser formatChar with
                            | Some parseFn ->
                                try
                                    parseFn routeValue
                                with
                                | :? FormatException ->
                                    raise <| RouteParseException routeValue
                            | None ->
                                routeValue
                        ctx
                    |]) :?> Task
                with
                | RouteParseException(value) ->
                    ctx.SetStatusCode 400
                    ctx.WriteTextAsync $"Url segment value '%s{value}' has invalid format"

        SimpleEndpoint (HttpVerb.Any, template, requestDelegate, Seq.empty)

    let subRoute
        (path     : string)
        (endpoints: Endpoint seq): Endpoint =
        NestedEndpoint (path, endpoints, Seq.empty)

    let rec applyBefore
        (httpHandler : EndpointHandler)
        (endpoint    : Endpoint) =
        match endpoint with
        | SimpleEndpoint(v, p, h, ml)      -> SimpleEndpoint(v, p, httpHandler >=> h, ml)
        | NestedEndpoint(t, lst, ml)       -> NestedEndpoint(t, Seq.map (applyBefore httpHandler) lst, ml)
        | MultiEndpoint(lst)               -> MultiEndpoint(Seq.map (applyBefore httpHandler) lst)

    let rec applyAfter
        (httpHandler : EndpointHandler)
        (endpoint    : Endpoint) =
        match endpoint with
        | SimpleEndpoint(v, p, h, ml)      -> SimpleEndpoint(v, p, h >=> httpHandler, ml)
        | NestedEndpoint(t, lst, ml)       -> NestedEndpoint(t, Seq.map (applyAfter httpHandler) lst, ml)
        | MultiEndpoint(lst)               -> MultiEndpoint(Seq.map (applyAfter httpHandler) lst)

    let rec addMetadata
        (newMetadata: obj)
        (endpoint: Endpoint) =
        match endpoint with
        | SimpleEndpoint(verb, template, handler, metadata) -> SimpleEndpoint(verb, template, handler, seq { yield! metadata; newMetadata })
        | NestedEndpoint(template, endpoints, metadata) -> NestedEndpoint(template, endpoints, seq { yield! metadata; newMetadata })
        | MultiEndpoint endpoints -> MultiEndpoint(Seq.map (addMetadata newMetadata) endpoints)

type EndpointRouteBuilderExtensions() =

    [<Extension>]
    static member private MapSingleEndpoint
        (builder       : IEndpointRouteBuilder,
        singleEndpoint : HttpVerb * RouteTemplate * RequestDelegate * Metadata) =

        let verb, routeTemplate, requestDelegate, metadata = singleEndpoint
        match verb with
        | Any  ->
            builder
                .Map(routeTemplate, requestDelegate)
                .WithMetadata(metadata) |> ignore
        | _  ->
            builder
                .MapMethods(routeTemplate, [| verb.ToString() |], requestDelegate)
                .WithMetadata(metadata) |> ignore

    [<Extension>]
    static member private MapSeveralEndpoints
        (builder     : IEndpointRouteBuilder,
        multiEndpoint: RouteTemplate * Endpoint seq * Metadata) =

        let parentTemplate, endpoints, parentMetadata = multiEndpoint
        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint (verb, template, handler, metadata) ->
                builder.MapSingleEndpoint(verb, $"{parentTemplate}{template}", handler, seq { yield! parentMetadata; yield! metadata; })
            | NestedEndpoint (template, endpoints, metadata) ->
                builder.MapSeveralEndpoints($"{parentTemplate}{template}", endpoints, seq { yield! parentMetadata; yield! metadata })
            | MultiEndpoint el ->
                builder.MapSeveralEndpoints(parentTemplate, el, parentMetadata)

    [<Extension>]
    static member MapOxpeckerEndpoints
        (builder : IEndpointRouteBuilder,
        endpoints: Endpoint seq) =

        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint (verb, template, handler, metadata) -> builder.MapSingleEndpoint (verb, template, handler, metadata)
            | NestedEndpoint (template, endpoints, metadata) -> builder.MapSeveralEndpoints (template, endpoints, metadata)
            | MultiEndpoint endpoints -> builder.MapSeveralEndpoints ("", endpoints, Seq.empty)
