namespace Oxpecker.Routing2

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

    let convertToRouteTemplate (pathValue : string) =
        let rec convert (i : int) (chars : char list) =
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
    let stringParse (s : string) = s.Replace("%2F", "/", StringComparison.OrdinalIgnoreCase) |> box
    let intParse (s : string) = int s |> box
    let boolParse (s : string) = bool.Parse s |> box
    let charParse (s : string) = char s[0] |> box
    let int64Parse (s : string) = int64 s |> box
    let floatParse (s : string) = float s |> box

    let tryGetParser (c : char) =
        match c with
        | 's' -> Some stringParse
        | 'i' -> Some intParse
        | 'b' -> Some boolParse
        | 'c' -> Some charParse
        | 'd' -> Some int64Parse
        | 'f' -> Some floatParse
        | _   -> None

    let private handleResult (result : HttpContext option) (ctx : HttpContext) =
        match result with
        | None   -> ctx.SetStatusCode (int HttpStatusCode.UnprocessableEntity)
        | Some _ -> ()

    let createRequestDelegate (handler : HttpHandler) =
        let func : HttpFunc = handler earlyReturn
        fun (ctx : HttpContext) ->
            task {
                let! result = func ctx
                return handleResult result ctx
            } :> Task


[<AutoOpen>]
module Routers =

    type HttpVerb =
        | GET | POST | PUT | PATCH | DELETE | HEAD | OPTIONS | TRACE | CONNECT
        | NotSpecified

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
    type Metadata = obj[]

    type Endpoint2 =
        | SimpleEndpoint   of HttpVerb * RouteTemplate * RequestDelegate * Metadata
        | NestedEndpoint   of RouteTemplate * Endpoint2 seq  * Metadata
        | MultiEndpoint    of Endpoint2 seq

    let rec private applyHttpVerbToEndpoint
        (verb     : HttpVerb)
        (endpoint : Endpoint2) : Endpoint2 =
        match endpoint with
        | SimpleEndpoint (_, routeTemplate, requestDelegate, metadata) ->
            SimpleEndpoint (verb, routeTemplate, requestDelegate, metadata)
        | NestedEndpoint (routeTemplate, endpoints, metadata) ->
            NestedEndpoint (
                routeTemplate,
                endpoints
                |> Seq.map (applyHttpVerbToEndpoint verb),
                metadata)
        | MultiEndpoint endpoints ->
            endpoints
            |> Seq.map(applyHttpVerbToEndpoint verb)
            |> MultiEndpoint

    let rec private applyHttpVerbToEndpoints
        (verb      : HttpVerb)
        (endpoints : Endpoint2 seq) : Endpoint2 =
        endpoints
        |> Seq.map(
            fun endpoint ->
                match endpoint with
                | SimpleEndpoint (_, routeTemplate, requestDelegate, metadata) ->
                    SimpleEndpoint (verb, routeTemplate, requestDelegate, metadata)
                | NestedEndpoint (routeTemplate, endpoints, metadata) ->
                    NestedEndpoint (
                        routeTemplate,
                        endpoints
                        |> Seq.map (applyHttpVerbToEndpoint verb),
                        metadata)
                | MultiEndpoint endpoints ->
                    applyHttpVerbToEndpoints verb endpoints)
        |> MultiEndpoint

    let rec private applyHttpVerbsToEndpoints
        (verbs     : HttpVerb seq)
        (endpoints : Endpoint2 seq) : Endpoint2 =
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

    let GET_HEAD: Endpoint2 seq -> Endpoint2 = applyHttpVerbsToEndpoints [ GET; HEAD ]

    let GET: Endpoint2 seq -> Endpoint2     = applyHttpVerbToEndpoints GET
    let POST: Endpoint2 seq -> Endpoint2    = applyHttpVerbToEndpoints POST
    let PUT: Endpoint2 seq -> Endpoint2     = applyHttpVerbToEndpoints PUT
    let PATCH: Endpoint2 seq -> Endpoint2   = applyHttpVerbToEndpoints PATCH
    let DELETE: Endpoint2 seq -> Endpoint2  = applyHttpVerbToEndpoints DELETE
    let HEAD: Endpoint2 seq -> Endpoint2    = applyHttpVerbToEndpoints HEAD
    let OPTIONS: Endpoint2 seq -> Endpoint2 = applyHttpVerbToEndpoints OPTIONS
    let TRACE: Endpoint2 seq -> Endpoint2   = applyHttpVerbToEndpoints TRACE
    let CONNECT: Endpoint2 seq -> Endpoint2 = applyHttpVerbToEndpoints CONNECT

    let route
        (path     : string)
        (handler  : HttpHandler) : Endpoint2 =
        let d = RequestDelegateBuilder.createRequestDelegate handler
        SimpleEndpoint (HttpVerb.NotSpecified, path, d, [||])

    let routef
        (path         : PrintfFormat<'T,unit,unit, HttpHandler>)
        (routeHandler : 'T) : Endpoint2 =
        let handlerType = routeHandler.GetType()
        let handlerMethod = handlerType.GetMethods()[0]
        let template, mappings = RouteTemplateBuilder.convertToRouteTemplate path.Value
        let arrMappings = mappings |> List.toArray

        let requestDelegate =
            fun (ctx : HttpContext) ->
                let routeData = ctx.GetRouteData()
                let z = handlerMethod.Invoke(routeHandler, [|
                    for mapping in arrMappings do
                        let placeholderName, formatChar = mapping
                        let routeValue = routeData.Values[placeholderName]
                        match RequestDelegateBuilder.tryGetParser formatChar with
                        | Some parseFn -> parseFn (string routeValue)
                        | None         -> routeValue
                    earlyReturn
                    ctx
                |])
                z :?> Task
        SimpleEndpoint (HttpVerb.NotSpecified, template, requestDelegate, [||])

    let subRoute
        (path      : string)
        (endpoints : Endpoint2 seq) : Endpoint2 =
        NestedEndpoint (path, endpoints, [||])

type EndpointRouteBuilderExtensions() =

    [<Extension>]
    static member private MapSingleEndpoint
        (builder        : IEndpointRouteBuilder,
        singleEndpoint  : HttpVerb * RouteTemplate * RequestDelegate * Metadata) =

        let verb, routeTemplate, requestDelegate, metadata = singleEndpoint
        match verb with
        | NotSpecified  ->
            builder
                .Map(routeTemplate, requestDelegate)
                .WithMetadata(metadata) |> ignore
        | _  ->
            builder
                .MapMethods(routeTemplate, [ verb.ToString() ], requestDelegate)
                .WithMetadata(metadata) |> ignore

    [<Extension>]
    static member private MapSeveralEndpoints
        (builder      : IEndpointRouteBuilder,
        multiEndpoint : RouteTemplate * Endpoint2 seq * Metadata) =

        let subRouteTemplate, endpoints, parentMetadata = multiEndpoint
        let routeTemplate = sprintf "%s%s" subRouteTemplate
        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint (v, t, d, ml) ->
                builder.MapSingleEndpoint(v, routeTemplate t, d, [| yield! ml; yield! parentMetadata|])
            | NestedEndpoint (t, e, ml) ->
                builder.MapSeveralEndpoints(routeTemplate t, e, [| yield! ml; yield! parentMetadata|])
            | MultiEndpoint el ->
                builder.MapSeveralEndpoints(subRouteTemplate, el, parentMetadata)

    [<Extension>]
    static member MapOxpeckerEndpoints2
        (builder  : IEndpointRouteBuilder,
        endpoints : Endpoint2 seq) =

        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint (v, t, d, ml) -> builder.MapSingleEndpoint (v, t, d, ml)
            | NestedEndpoint (t, e, ml) -> builder.MapSeveralEndpoints (t, e, ml)
            | MultiEndpoint e -> builder.MapSeveralEndpoints ("", e, [||])
