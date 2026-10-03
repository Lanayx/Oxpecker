namespace Oxpecker

open System
open System.Collections.Concurrent
open System.Reflection
open System.Runtime.CompilerServices
open System.Text.RegularExpressions
open System.Threading.Tasks
open Microsoft.AspNetCore.Antiforgery
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
        | QUERY
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
            | QUERY -> "QUERY"
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
        | MultiEndpoint of Endpoint seq * ConfigureEndpoint

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
            | 'i' -> int s |> boxv
            | 'b' -> bool.Parse s |> boxv
            | 'c' -> char s[0] |> boxv
            | 'd' -> int64 s |> boxv
            | 'f' -> float s |> boxv
            | 'u' -> uint64 s |> boxv
            | 'O' ->
                match modifier with
                | Some "guid" -> Guid.Parse s |> boxv
                | _ -> s
            | _ -> s
        with :? FormatException as ex ->
            raise
            <| RouteParseException($"Url segment value '%s{s}' has invalid format", ex)

    let placeholderPattern = Regex("\{(\*{0,2})%([sibcdfuO])(:[^}]+)?\}")
    // This function should convert to route template and mappings
    // "api/{%s}/{%i}" -> ("api/{x}/{y}", [("x", 's', None); ("y", 'i', None)])
    // "api/{%O:guid}/{%s}" -> ("api/{x:guid}/{y}", [("x", 'O', Some "guid"); ("y", 's', None)])
    let convertToRouteTemplate (pathValue: string) (parameters: ParameterInfo[]) =
        let mutable index = 0
        let mappings = ResizeArray()

        let placeholderEvaluator =
            MatchEvaluator(fun m ->
                let slug = m.Groups[1].Value
                let vtype = m.Groups[2].Value[0] // Second capture group is the variable type s, i, or O
                let formatSpecifier = if m.Groups[3].Success then m.Groups[3].Value else ""
                let paramName = parameters[index].Name |> string
                index <- index + 1 // Increment index for next use
                mappings.Add(
                    paramName,
                    vtype,
                    if formatSpecifier = "" then
                        None
                    else
                        Some <| formatSpecifier.TrimStart(':')
                )
                $"{{%s{slug}%s{paramName}%s{formatSpecifier}}}" // Construct the new placeholder
            )

        let newRoute = placeholderPattern.Replace(pathValue, placeholderEvaluator)
        (newRoute, mappings.ToArray())

module RoutingInternal =
    type AddFilter =
        static member Compose(filter: EndpointHandler, endpoint: Endpoint) =
            match endpoint with
            | SimpleEndpoint(verb, template, handler, configure) ->
                SimpleEndpoint(verb, template, filter >=> handler, configure)
            | NestedEndpoint(template, endpoints, configure) ->
                NestedEndpoint(template, Seq.map (fun e -> AddFilter.Compose(filter, e)) endpoints, configure)
            | MultiEndpoint(endpoints, configure) ->
                MultiEndpoint(Seq.map (fun e -> AddFilter.Compose(filter, e)) endpoints, configure)

        static member Compose(filterMiddleware: EndpointMiddleware, endpoint: Endpoint) =
            match endpoint with
            | SimpleEndpoint(verb, template, handler, configure) ->
                SimpleEndpoint(verb, template, filterMiddleware >=> handler, configure)
            | NestedEndpoint(template, endpoints, configure) ->
                NestedEndpoint(template, Seq.map (fun e -> AddFilter.Compose(filterMiddleware, e)) endpoints, configure)
            | MultiEndpoint(endpoints, configure) ->
                MultiEndpoint(Seq.map (fun e -> AddFilter.Compose(filterMiddleware, e)) endpoints, configure)

    let private getArgByIndex (routeData: RouteData) (mappings: (string * char * Option<_>) array) (index: int) =
        let placeholderName, formatChar, modifier = mappings[index]
        let routeValue = routeData.Values[placeholderName] |> string
        RouteTemplateBuilder.parse formatChar modifier routeValue

    let private invokeHandler<'T>
        (ctx: HttpContext)
        (invoker: MethodInvoker)
        (handler: 'T)
        (mappings: (string * char * Option<_>) array)
        (ctxInParameterList: bool)
        =
        let routeData = ctx.GetRouteData()
        if ctxInParameterList then
            match mappings.Length with
            | 0 -> invoker.Invoke(handler, ctx)
            | 1 ->
                let arg = getArgByIndex routeData mappings 0
                invoker.Invoke(handler, arg, ctx)
            | 2 ->
                let arg1 = getArgByIndex routeData mappings 0
                let arg2 = getArgByIndex routeData mappings 1
                invoker.Invoke(handler, arg1, arg2, ctx)
            | 3 ->
                let arg1 = getArgByIndex routeData mappings 0
                let arg2 = getArgByIndex routeData mappings 1
                let arg3 = getArgByIndex routeData mappings 2
                invoker.Invoke(handler, arg1, arg2, arg3, ctx)
            | _ ->
                invoker.Invoke(
                    handler,
                    Span [|
                        for placeholderName, formatChar, modifier in mappings do
                            let routeValue = routeData.Values[placeholderName] |> string
                            RouteTemplateBuilder.parse formatChar modifier routeValue
                        ctx
                    |]
                )
            |> nonNull
            :?> Task
        else
            invoker.Invoke(
                handler,
                Span [|
                    for placeholderName, formatChar, modifier in mappings do
                        let routeValue = routeData.Values[placeholderName] |> string
                        RouteTemplateBuilder.parse formatChar modifier routeValue
                |]
            )
            |> nonNull
            :?> FSharpFunc<HttpContext, Task>
            <| ctx

    [<NoEquality; NoComparison>]
    type private RoutefInfo = {
        Template: RouteTemplate
        Mappings: (string * char * string option) array
        Parameters: ParameterInfo array
        Invoker: MethodInvoker
    }

    // Holds only data derived from the handler type and the format string, never handler instances,
    // so routef endpoints rebuilt on every request (inside subRoutef) don't repeat reflection
    let private routefInfoCache = ConcurrentDictionary<struct (Type * string), RoutefInfo>()

    let private createRoutefInfo =
        Func<struct (Type * string), RoutefInfo>(fun (struct (handlerType, path)) ->
            let handlerMethod = handlerType.GetMethods()[0]
            let parameters = handlerMethod.GetParameters()
            if parameters.Length < RouteTemplateBuilder.placeholderPattern.Count(path) then
                failwith $"Handler has fewer parameters than route placeholders: %s{path}"
            let template, mappings = RouteTemplateBuilder.convertToRouteTemplate path parameters
            {
                Template = template
                Mappings = mappings
                Parameters = parameters
                Invoker = MethodInvoker.Create(handlerMethod)
            })

    let private getRoutefInfo (path: string) (handlerType: Type) =
        routefInfoCache.GetOrAdd(struct (handlerType, path), createRoutefInfo)

    let routefInner (path: PrintfFormat<'T, unit, unit, EndpointHandler>) (handler: 'T) =
        let info = getRoutefInfo path.Value (handler.GetType())
        let ctxInParameterList =
            if info.Parameters.Length = info.Mappings.Length + 1 then
                true
            elif info.Parameters.Length = info.Mappings.Length then
                false
            else
                failwith <| "Unsupported routef handler: " + path.Value
        let requestDelegate =
            fun (ctx: HttpContext) -> invokeHandler<'T> ctx info.Invoker handler info.Mappings ctxInParameterList

        info.Template, info.Mappings, requestDelegate

    // Argument for the startup call of a subRoutef factory. Real path segments are never empty, hence non-empty string
    let private placeholderArg (parameter: ParameterInfo) =
        let parameterType = parameter.ParameterType
        if parameterType = typeof<string> || parameterType = typeof<obj> then
            box "placeholder"
        elif parameterType.IsValueType then
            Activator.CreateInstance parameterType
        else
            null

    let private invokeFactory (ctx: HttpContext) (info: RoutefInfo) (factory: obj) =
        let routeData = ctx.GetRouteData()
        let mappings = info.Mappings
        match mappings.Length with
        | 1 -> info.Invoker.Invoke(factory, getArgByIndex routeData mappings 0)
        | 2 ->
            let arg1 = getArgByIndex routeData mappings 0
            let arg2 = getArgByIndex routeData mappings 1
            info.Invoker.Invoke(factory, arg1, arg2)
        | 3 ->
            let arg1 = getArgByIndex routeData mappings 0
            let arg2 = getArgByIndex routeData mappings 1
            let arg3 = getArgByIndex routeData mappings 2
            info.Invoker.Invoke(factory, arg1, arg2, arg3)
        | _ -> info.Invoker.Invoke(factory, Span(Array.init mappings.Length (getArgByIndex routeData mappings)))
        |> nonNull
        :?> Endpoint seq

    // Finds the handler at indexPath, checking that every node on the way has the same kind and template as at startup
    // (templates[depth] is ValueNone for MultiEndpoint)
    let rec private tryFindHandler
        (indexPath: int array)
        (templates: RouteTemplate voption array)
        (depth: int)
        (endpoints: Endpoint seq)
        =
        let isLeaf = depth = indexPath.Length - 1
        match Seq.tryItem indexPath[depth] endpoints, templates[depth] with
        | Some(SimpleEndpoint(_, template, handler, _)), ValueSome expected when isLeaf && template = expected ->
            ValueSome handler
        | Some(NestedEndpoint(template, children, _)), ValueSome expected when not isLeaf && template = expected ->
            tryFindHandler indexPath templates (depth + 1) children
        | Some(MultiEndpoint(children, _)), ValueNone when not isLeaf ->
            tryFindHandler indexPath templates (depth + 1) children
        | _ -> ValueNone

    // Keeps the structure, templates, verbs and configuration of the endpoints, but replaces handlers with resolvers
    let rec private wrapEndpoints
        (resolve: int array -> RouteTemplate voption array -> EndpointHandler)
        (indexPath: int list)
        (templates: RouteTemplate voption list)
        (endpoints: Endpoint seq)
        : Endpoint seq =
        endpoints
        |> Seq.mapi(fun index endpoint ->
            let indexPath = index :: indexPath
            match endpoint with
            | SimpleEndpoint(verbs, template, _, configure) ->
                let templates = ValueSome template :: templates
                let handler = resolve (indexPath |> List.rev |> List.toArray) (templates |> List.rev |> List.toArray)
                SimpleEndpoint(verbs, template, handler, configure)
            | NestedEndpoint(template, children, configure) ->
                NestedEndpoint(
                    template,
                    wrapEndpoints resolve indexPath (ValueSome template :: templates) children,
                    configure
                )
            | MultiEndpoint(children, configure) ->
                MultiEndpoint(wrapEndpoints resolve indexPath (ValueNone :: templates) children, configure))
        |> Seq.toArray
        :> Endpoint seq

    let private subRoutefFromFactory (path: string) (factory: obj) =
        let info = getRoutefInfo path (factory.GetType())
        if info.Parameters.Length <> info.Mappings.Length then
            failwith <| "Unsupported subRoutef endpoints factory: " + path
        if info.Template.Contains("{*") then
            failwith <| "Catch-all parameters are not supported in subRoutef: " + path
        let startupEndpoints =
            try
                info.Invoker.Invoke(factory, Span(Array.map placeholderArg info.Parameters))
                |> nonNull
                :?> Endpoint seq
            with ex ->
                raise
                <| InvalidOperationException(
                    $"subRoutef '%s{path}': endpoints factory failed when called with placeholder arguments at startup. It should only construct endpoints.",
                    ex
                )
        let resolve (indexPath: int array) (templates: RouteTemplate voption array) : EndpointHandler =
            fun ctx ->
                match invokeFactory ctx info factory |> tryFindHandler indexPath templates 0 with
                | ValueSome handler -> handler ctx
                | ValueNone ->
                    raise
                    <| InvalidOperationException(
                        $"subRoutef '%s{path}': endpoints factory returned different endpoints than at startup. Number, order, nesting and templates of endpoints must not depend on route values."
                    )
        info.Template, info.Mappings, wrapEndpoints resolve [] [] startupEndpoints

    let subRoutefInner (path: PrintfFormat<'T, unit, unit, Endpoint list>) (endpointsFactory: 'T) =
        match box endpointsFactory with
        | :? (Endpoint list) as endpoints -> path.Value, [||], (endpoints :> Endpoint seq)
        | factory -> subRoutefFromFactory path.Value (nonNull factory)


[<AutoOpen>]
module Routers =
    open CoreInternal
    open RoutingInternal

    let rec applyHttpVerbsToEndpoint (verbs: HttpVerbs) (endpoint: Endpoint) : Endpoint =
        match endpoint with
        | SimpleEndpoint(oldVerbs, routeTemplate, handler, configure) ->
            if oldVerbs = HttpVerbs.Any || oldVerbs = verbs then
                SimpleEndpoint(verbs, routeTemplate, handler, configure)
            else
                failwithf $"Http verbs intersect at '%s{routeTemplate}'"
        | NestedEndpoint(handler, endpoints, configure) ->
            NestedEndpoint(handler, endpoints |> Seq.map(applyHttpVerbsToEndpoint verbs), configure)
        | MultiEndpoint(endpoints, configure) ->
            MultiEndpoint(endpoints |> Seq.map(applyHttpVerbsToEndpoint verbs), configure)

    let rec applyHttpVerbsToEndpoints (verbs: HttpVerbs) (endpoints: Endpoint seq) : Endpoint =
        endpoints
        |> Seq.map(function
            | SimpleEndpoint(oldVerbs, routeTemplate, handler, configure) ->
                if oldVerbs = HttpVerbs.Any || oldVerbs = verbs then
                    SimpleEndpoint(verbs, routeTemplate, handler, configure)
                else
                    failwithf $"Http verbs intersect at '%s{routeTemplate}'"
            | NestedEndpoint(template, endpoints, configure) ->
                NestedEndpoint(template, endpoints |> Seq.map(applyHttpVerbsToEndpoint verbs), configure)
            | MultiEndpoint(endpoints, configure) ->
                MultiEndpoint(endpoints |> Seq.map(applyHttpVerbsToEndpoint verbs), configure))
        |> (fun endpoints -> MultiEndpoint(endpoints, id))

    let GET_HEAD: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ GET; HEAD ])

    let GET: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ GET ])
    let POST: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ POST ])
    let PUT: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ PUT ])
    let PATCH: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ PATCH ])
    let QUERY: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ QUERY ])
    let DELETE: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ DELETE ])
    let HEAD: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ HEAD ])
    let OPTIONS: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ OPTIONS ])
    let TRACE: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ TRACE ])
    let CONNECT: Endpoint seq -> Endpoint = applyHttpVerbsToEndpoints(Verbs [ CONNECT ])

    let route (path: string) (handler: EndpointHandler) : Endpoint =
        SimpleEndpoint(HttpVerbs.Any, path, handler, id)

    let routef (path: PrintfFormat<'T, unit, unit, EndpointHandler>) (handler: 'T) : Endpoint =
        let template, _, requestDelegate = routefInner path handler

        SimpleEndpoint(HttpVerbs.Any, template, requestDelegate, id)

    let subRoute (path: string) (endpoints: Endpoint seq) : Endpoint = NestedEndpoint(path, endpoints, id)

    let subRoutef (path: PrintfFormat<'T, unit, unit, Endpoint list>) (endpointsFactory: 'T) : Endpoint =
        let template, _, endpoints = subRoutefInner path endpointsFactory

        NestedEndpoint(template, endpoints, id)

    let routeGroup (endpoints: Endpoint seq) : Endpoint = MultiEndpoint(endpoints, id)

    let rec configureEndpoint (f: ConfigureEndpoint) (endpoint: Endpoint) =
        match endpoint with
        | SimpleEndpoint(verb, template, handler, configure) -> SimpleEndpoint(verb, template, handler, configure >> f)
        | NestedEndpoint(template, endpoints, configure) -> NestedEndpoint(template, endpoints, configure >> f)
        | MultiEndpoint(endpoints, configure) -> MultiEndpoint(endpoints, configure >> f)

    let inline addFilter (filter: 'T) (endpoint: Endpoint) =
        compose_opImpl Unchecked.defaultof<AddFilter> filter endpoint

    let addMetadata (metadata: obj) =
        configureEndpoint _.WithMetadata(metadata)

type EndpointRouteBuilderExtensions() =

    static member private GetConfigureEndpoint(configure: ConfigureEndpoint, addAntiforgery: bool) =
        if addAntiforgery then
            _.WithMetadata(RequireAntiforgeryTokenAttribute()) >> configure
        else
            configure

    static member private GetConfigureEndpoint
        (verbs: HttpVerb seq, configure: ConfigureEndpoint, addAntiforgery: bool)
        =
        if addAntiforgery then
            let canHaveForm =
                verbs
                |> Seq.exists(fun verb -> verb = HttpVerb.POST || verb = HttpVerb.PUT || verb = HttpVerb.PATCH)
            if canHaveForm then
                _.WithMetadata(RequireAntiforgeryTokenAttribute()) >> configure
            else
                configure
        else
            configure

    [<Extension>]
    static member private IsAntiforgeryEnabled(builder: IEndpointRouteBuilder) =
        match builder.ServiceProvider.GetService(typeof<IAntiforgery>) with
        | null -> false
        | _ -> true

    [<Extension>]
    static member private MapSingleEndpoint
        (
            builder: IEndpointRouteBuilder,
            verb: HttpVerbs,
            routeTemplate: RouteTemplate,
            requestDelegate: RequestDelegate,
            configure: ConfigureEndpoint,
            addAntiforgery: bool
        ) =
        match verb with
        | Any ->
            builder.Map(routeTemplate, requestDelegate)
            |> EndpointRouteBuilderExtensions.GetConfigureEndpoint(configure, addAntiforgery)
        | Verbs verbs ->
            builder.MapMethods(routeTemplate, verbs |> Seq.map string, requestDelegate)
            |> EndpointRouteBuilderExtensions.GetConfigureEndpoint(verbs, configure, addAntiforgery)
        |> ignore

    [<Extension>]
    static member private MapNestedEndpoint
        (
            builder: IEndpointRouteBuilder,
            parentTemplate: RouteTemplate,
            endpoints: Endpoint seq,
            parentConfigure: ConfigureEndpoint,
            addAntiforgery: bool
        ) =
        let groupBuilder = builder.MapGroup(parentTemplate)
        let groupConfigure = EndpointRouteBuilderExtensions.GetConfigureEndpoint(parentConfigure, addAntiforgery)
        groupBuilder |> groupConfigure |> ignore
        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint(verb, template, handler, configure) ->
                groupBuilder.MapSingleEndpoint(verb, template, handler, configure, addAntiforgery)
            | NestedEndpoint(template, endpoints, configure) ->
                groupBuilder.MapNestedEndpoint(template, endpoints, configure, addAntiforgery)
            | MultiEndpoint(endpoints, configure) -> groupBuilder.MapMultiEndpoint(endpoints, configure, addAntiforgery)

    [<Extension>]
    static member private MapMultiEndpoint
        (
            builder: IEndpointRouteBuilder,
            endpoints: Endpoint seq,
            parentConfigure: ConfigureEndpoint,
            addAntiforgery: bool
        ) =
        builder.MapNestedEndpoint("", endpoints, parentConfigure, addAntiforgery)

    [<Extension>]
    static member MapOxpeckerEndpoint(builder: IEndpointRouteBuilder, endpoint: Endpoint) =
        let addAntiforgery = builder.IsAntiforgeryEnabled()
        match endpoint with
        | SimpleEndpoint(verb, template, handler, configure) ->
            builder.MapSingleEndpoint(verb, template, handler, configure, addAntiforgery)
        | NestedEndpoint(template, endpoints, configure) ->
            builder.MapNestedEndpoint(template, endpoints, configure, addAntiforgery)
        | MultiEndpoint(endpoints, configure) -> builder.MapMultiEndpoint(endpoints, configure, addAntiforgery)

    [<Extension>]
    static member MapOxpeckerEndpoints(builder: IEndpointRouteBuilder, endpoints: Endpoint seq) =
        let addAntiforgery = builder.IsAntiforgeryEnabled()
        for endpoint in endpoints do
            match endpoint with
            | SimpleEndpoint(verb, template, handler, configure) ->
                builder.MapSingleEndpoint(verb, template, handler, configure, addAntiforgery)
            | NestedEndpoint(template, endpoints, configure) ->
                builder.MapNestedEndpoint(template, endpoints, configure, addAntiforgery)
            | MultiEndpoint(endpoints, configure) -> builder.MapMultiEndpoint(endpoints, configure, addAntiforgery)
