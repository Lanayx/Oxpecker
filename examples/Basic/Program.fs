open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Net.Http.Headers
open Oxpecker

type RequiresAuditAttribute() = inherit Attribute()

let setHeaderMw key value: EndpointMiddleware =
    fun (next: EndpointHandler) (ctx: HttpContext) ->
        ctx.SetHttpHeader(key, value)
        task {
            do! next ctx
            Console.WriteLine($"Header {key} set")
        }
let handler0 (name: string) : EndpointHandler =
    _.WriteText(sprintf "Hello %s" name)

let handler1: EndpointHandler =
    fun ctx -> ctx.WriteText "Hello World"

let handler2 (name: string) (age: int): EndpointHandler =
    _.WriteText(sprintf "Hello %s, you are %i years old." name age)

let handler3 (a: string) (b: string) (c: string) (d: int): EndpointHandler =
    _.WriteText(sprintf "Hello %s %s %s %i" a b c d)

[<CLIMutable>]
type MyModel = {
    Name: string
    Age: int
}
let handler4 (a: MyModel): EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteJsonChunked { a with Name = a.Name + "!" }

let handler5 (test1: string) (test2: string) (a: MyModel): EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteJson {| a with Name = a.Name + "!"; Test = test1 + test2 |}

let handler6 (test: string) (a: MyModel): EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteJson {| a with Name = a.Name + "!"; Test = test |}

let handler7 (a: MyModel) (b: MyModel): EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteJson {| a with Name = b.Name + "!"; Test = b.Name |}

let handler8 (test: string) (a: MyModel) (b: MyModel): EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteJson {| a with Name = b.Name + "!"; Test = test |}

let handler9 (test1: string) (test2: string) (a: MyModel) (b: MyModel): EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteJson {| a with Name = b.Name + "!"; Test = test1 + test2 |}

let handler10: EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteText(string DateTime.Now)

let handler11 version: EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteText($"api version is %d{version}")

let handler12 version1 version2: EndpointHandler =
    fun (ctx: HttpContext) ->
        ctx.WriteText($"api version is %d{version1} %d{version2}")

let authHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        if ctx.Request.Path.Value.Contains("closed") then
            ctx.SetStatusCode 401
            json {| Status = "Unauthorized" |} ctx
        else
            Task.CompletedTask

let MY_AUTH = applyBefore authHandler
let MY_HEADER endpoint = applyBefore (setHeaderMw "my" "header") endpoint
let NO_RESPONSE_CACHE = applyBefore noResponseCaching
let RESPONSE_CACHE =
    let cacheDirective = CacheControlHeaderValue(MaxAge = TimeSpan.FromSeconds(10), Public = true) |> Some
    applyBefore <| responseCaching cacheDirective None None

let endpoints =
    [
        GET [
            route "/" (text "Hello World")
            routef "/{%s}" (setHeaderMw "foo" "moo" >>=> handler0)
            routef "/{%s}/{%i}" (setHeaderMw "foo" "var" >>=>+ handler2)
            routef "/{%s}/{%s}/{%s}/{%i:min(15)}" handler3
            route "/x" (bindQuery handler4)
            routef "/x/{%s}/{%s}" (bindQuery <<+ handler5)
            routef "/xx/{%s}" (setHeaderMw "foo" "xx" >>=> bindQuery << handler6)
            routef "/xx/{%s}/{%s}" (setHeaderMw "foo" "xx" >>=>+ (bindQuery <<+ handler5))
            route "/abc" (json {| X = "Y" |})
            route "/cbd/{**x}" (json {| X = "Z" |})
        ]
        POST [
            route "/x" (bindJson handler4)
            route "/y" (bindQuery (bindJson << handler7))
            routef "/y/{%s}" (bindQuery << (bindJson <<+ handler8))
            routef "/y/{%s}/{%s}" (setHeaderMw "foo" "yy" >>=>+ (bindQuery <<+ (bindJson <<++ handler9)))
        ]
        // Not specifying a http verb means it will listen to all verbs
        route "/foo" (setHeaderMw "foo" "bar" >=> text "Bar")

        subRoute "/sub1" [
            GET [
                subRoute "/sub2" [
                    MY_HEADER <| route "/test" handler1
                ]
            ]
            MY_AUTH <| POST [
                route "/sub2/test" handler1
            ]
        ]
        MY_AUTH <| route "/auth/x" (text "Not closed")
        MY_AUTH <| MY_HEADER (
            subRoute "/auth" [
                route "/open" handler1
                route "/closed" handler1 |> addMetadata (RequiresAuditAttribute())
            ]
        )

        route "/time" handler10 |> NO_RESPONSE_CACHE
        route "/time-cached" handler10 |> RESPONSE_CACHE
        route "/redirect" (redirectTo "/time" false)

        subRoutef "/v{%i}" [
            "/test", handler11
        ]
    ]

let notFoundHandler (ctx: HttpContext) =
    let logger = ctx.GetLogger()
    logger.LogWarning("Unhandled 404 error")
    ctx.SetStatusCode 404
    ctx.WriteText "Page not found!"

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
            return! ctx.WriteText <| string ex
        | ex ->
            let logger = ctx.GetLogger()
            logger.LogError(ex, "Unhandled 500 error")
            ctx.SetStatusCode StatusCodes.Status500InternalServerError
            return! ctx.WriteText <| string ex
    } :> Task

let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .Use(errorHandler)
        .UseOxpecker(endpoints)
        .Run(notFoundHandler)

let configureServices (services: IServiceCollection) =
    services
        .AddRouting()
        .AddOxpecker()
        .AddSingleton<ILogger>(fun sp ->
            sp.GetRequiredService<ILoggerFactory>().CreateLogger("Oxpecker.Examples.Basic"))
    |> ignore


[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services
    let app = builder.Build()
    configureApp app
    app.Run()
    0
