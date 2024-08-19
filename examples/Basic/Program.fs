open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Net.Http.Headers
open Oxpecker
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria
open Oxpecker.OpenApi
open Oxpecker.Htmx
open FSharp.Control
open type Microsoft.AspNetCore.Http.TypedResults

type RequiresAuditAttribute() =
    inherit Attribute()

let setHeaderMw key value : EndpointMiddleware =
    fun (next: EndpointHandler) (ctx: HttpContext) ->
        ctx.SetHttpHeader(key, value)
        task {
            do! next ctx
            Console.WriteLine($"Header {key} set")
        }
let handler0 (name: string) : EndpointHandler = _.WriteText(sprintf "Hello %s" name)

let handler1: EndpointHandler = fun ctx -> ctx.WriteText "Hello World"

let handler2 (name: string) (age: int) : EndpointHandler =
    _.WriteText(sprintf "Hello %s, you are %i years old." name age)

let handler3 (a: string) (b: string) (c: string) (d: int) : EndpointHandler =
    _.WriteText(sprintf "Hello %s %s %s %i" a b c d)

[<CLIMutable>]
type MyModel = { Name: string; Age: int }
let handler4 (a: MyModel) : EndpointHandler =
    fun (ctx: HttpContext) -> task { return! ctx.WriteJsonChunked { a with Name = a.Name + "!" } }

let handler5 (test1: string) (test2: string) (a: MyModel) : EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteJson {|
            a with
                Name = a.Name + "!"
                Test = test1 + test2
        |}

let handler6 (test: string) (a: MyModel) : EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteJson {|
            a with
                Name = a.Name + "!"
                Test = test
        |}

let handler7 (a: MyModel) (b: MyModel) : EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteJson {|
            a with
                Name = b.Name + "!"
                Test = b.Name
        |}

let handler8 (test: string) (a: MyModel) (b: MyModel) : EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteJson {|
            a with
                Name = b.Name + "!"
                Test = test
        |}

let handler9 (test1: string) (test2: string) (a: MyModel) (b: MyModel) : EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteJson {|
            a with
                Name = b.Name + "!"
                Test = test1 + test2
        |}

let handler10: EndpointHandler =
    fun (ctx: HttpContext) -> ctx.WriteText(string DateTime.Now)

let handler11: EndpointHandler =
    fun (ctx: HttpContext) ->
        let lang = ctx.TryGetRouteValue("lang") |> Option.defaultValue ""
        ctx.WriteText($"lang={lang}")

let closedHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        if ctx.Request.Path.Value.Contains("closed") then
            ctx.SetStatusCode 401
            json {| Status = "Unauthorized" |} ctx
        else
            Task.CompletedTask


let streamingJson: EndpointHandler =
    fun (ctx: HttpContext) ->
        let values =
            taskSeq {
                for i in 1..10 do
                    do! Task.Delay(500)
                    yield {| Id = i; Name = $"Name {i}" |}
            }
        jsonChunked values ctx

let streamingHtml1: EndpointHandler =
    fun (ctx: HttpContext) ->
        let html =
            html() {
                head() {
                    script(src = "https://unpkg.com/htmx.org@1.9.12")
                    script(src = "https://unpkg.com/htmx.ext...chunked-transfer@1.0.4/dist/index.js") // workaround, see https://github.com/bigskysoftware/htmx/issues/1911
                }
                body(style = "width: 800px; margin: 0 auto", hxExt = "chunked-transfer") {
                    h1(style = "text-align: center; color: blue") { "HTML Streaming example" }
                    h2(hxGet = "/streamHtml2", hxTarget = "this", hxTrigger = "load")
                }
            }
        htmlView html ctx


let streamingHtml2: EndpointHandler =
    fun (ctx: HttpContext) ->
        let values =
            taskSeq {
                for ch in "Hello world using Oxpecker streaming!" do
                    do! Task.Delay(20)
                    __() { string ch }
            }
        htmlChunked values ctx

let CLOSED = applyBefore closedHandler
let MY_HEADER endpoint =
    applyBefore (setHeaderMw "my" "header") endpoint
let NO_RESPONSE_CACHE = applyBefore noResponseCaching
let RESPONSE_CACHE =
    let cacheDirective =
        CacheControlHeaderValue(MaxAge = TimeSpan.FromSeconds(10), Public = true)
        |> Some
    applyBefore <| responseCaching cacheDirective None None

let GET_HEAD_OPTIONS: Endpoint seq -> Endpoint =
    applyHttpVerbsToEndpoints(Verbs [ HttpVerb.GET; HttpVerb.HEAD; HttpVerb.OPTIONS ])

let endpoints = [
    GET [
        route "/" <| text "Hello World"
        route "/iresult" <| %Ok {| Text = "Hello World" |}
        route "/ibadResult" <| % BadRequest()
        routef "/text/{%s}" text
        |> configureEndpoint _.WithName("GetText")
        |> addOpenApiSimple<unit, string>
        routef "/{%s}" (setHeaderMw "foo" "moo" >>=> handler0)
        routef "/{%s}/{%i}" (setHeaderMw "foo" "var" >>=>+ handler2)
        routef "/{%s}/{%s}/{%s}/{%i:min(15)}" handler3
        route "/x" (bindQuery handler4)
        routef "/xx/{%s}" (setHeaderMw "foo" "xx" >>=> bindQuery << handler6)
        routef "/xx/{%s}/{%s}" (setHeaderMw "foo" "xx" >>=>+ (bindQuery <<+ handler5))
        route "/abc" (json {| X = "Y" |})
        route "/cbd/{**x}" (json {| X = "Z" |})
        route "streamJson" streamingJson
        route "streamHtml1" streamingHtml1
        route "streamHtml2" streamingHtml2
    ]
    POST [
        route "/x" (bindJson handler4)
        routef "/x/{%s}/{%s}" (bindForm <<+ handler5)
        |> configureEndpoint _.WithName("BindForm")
        |> addOpenApi(
            OpenApiConfig(
                requestBody =
                    RequestBody(typeof<MyModel>, [| "multipart/form-data"; "application/x-www-form-urlencoded" |]),
                responseBodies = [ ResponseBody(typeof<MyModel>) ]
            )
        )
        route "/y" (bindQuery(bindJson << handler7))
        routef "/y/{%s}" (bindQuery << (bindJson <<+ handler8))
        routef "/y/{%s}/{%s}" (setHeaderMw "foo" "yy" >>=>+ (bindQuery <<+ (bindJson <<++ handler9)))
    ]
    // Not specifying a http verb means it will listen to all verbs
    route "/foo" (setHeaderMw "foo" "bar" >=> %Ok {| Text = "Bar" |})

    subRoute "/sub1" [
        GET [ subRoute "/sub2" [ MY_HEADER <| route "/test" handler1 ] ]
        CLOSED <| POST [ route "/sub2/test" handler1 ]
    ]
    CLOSED <| route "/auth/x" (text "Not closed")
    CLOSED
    <| MY_HEADER(
        subRoute "/auth" [
            route "/open" handler1
            route "/closed" handler1 |> addMetadata(RequiresAuditAttribute())
        ]
    )
    GET_HEAD_OPTIONS [
        route "/time" handler10 |> NO_RESPONSE_CACHE
        route "/time-cached" handler10 |> RESPONSE_CACHE
    ]
    |> configureEndpoint _.RequireAuthorization(AuthorizeAttribute(AuthenticationSchemes = "MyScheme"))
    route "/redirect" (redirectTo "/time" false)
    subRoute "/auth/{lang}" [ route "/" handler11 ]
]

let errorView errorCode (errorText: string) =
    html() {
        body(style = "width: 800px; margin: 0 auto") {
            h1(style = "text-align: center; color: red") { raw $"Error <i>%d{errorCode}</i>" }
            p(ariaErrorMessage = "err1").on("click", "console.log('clicked on error')") { errorText }
        }
    }

let notFoundHandler (ctx: HttpContext) =
    let logger = ctx.GetLogger()
    logger.LogWarning("Unhandled 404 error")
    ctx.SetStatusCode 404
    ctx.WriteHtmlView(errorView 404 "Page not found!")

let errorHandler (ctx: HttpContext) (next: RequestDelegate) =
    task {
        try
            return! next.Invoke(ctx)
        with
        | :? ModelBindException
        | :? RouteParseException as ex ->
            let logger = ctx.GetLogger()
            logger.LogWarning(ex, "Unhandled 400 error")
            ctx.SetStatusCode StatusCodes.Status400BadRequest
            return! ctx.WriteHtmlView(errorView 400 (string ex))
        | ex ->
            let logger = ctx.GetLogger()
            logger.LogError(ex, "Unhandled 500 error")
            ctx.SetStatusCode StatusCodes.Status500InternalServerError
            return! ctx.WriteHtmlView(errorView 500 (string ex))
    }
    :> Task

let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .Use(errorHandler)
        .UseOxpecker(endpoints)
        .UseSwagger() // for generating OpenApi spec
        .UseSwaggerUI() // for viewing Swagger UI
        .Run(notFoundHandler)

let configureServices (services: IServiceCollection) =
    services
        .AddRouting()
        .AddOxpecker()
        .AddEndpointsApiExplorer() // use the API Explorer to discover and describe endpoints
        .AddSwaggerGen() // swagger dependencies
    |> ignore


[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services
    let app = builder.Build()
    configureApp app
    app.Run()
    0
