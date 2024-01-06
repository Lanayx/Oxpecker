open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Oxpecker
open Oxpecker.Routing

type RequiresAuditAttribute() = inherit Attribute()

let setHttpHeaderMw key value: EndpointMiddleware =
    fun (next: EndpointHandler) (ctx: HttpContext) ->
        ctx.SetHttpHeader(key, value)
        task {
            do! next ctx
            Console.WriteLine($"Header {key} set")
        }

let handler1: EndpointHandler =
    fun ctx -> ctx.WriteTextAsync "Hello World"

let handler2 (firstName: string) (age: int): EndpointHandler =
    _.WriteTextAsync(sprintf "Hello %s, you are %i years old." firstName age)

let handler3 (a: string) (b: string) (c: string) (d: int): EndpointHandler =
    _.WriteTextAsync(sprintf "Hello %s %s %s %i" a b c d)

let authHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        if ctx.Request.Path.Value.Contains("closed") then
            ctx.SetStatusCode 401
            ctx.WriteTextAsync "Unauthorized"
        else
            Task.CompletedTask
let endpoints =
    [
        GET [
            route  "/" (text "Hello World")
            routef "/{%s}/{%i}" (fun name age ctx -> (setHttpHeaderMw "foo" "var"  >=> handler2 name age) ctx)
            routef "/{%s}/{%s}/{%s}/{%i:min(15)}" handler3
        ]
        GET_HEAD [
            route "/x"   (text "y")
            route "/abc" (text "def")
        ]
        // Not specifying a http verb means it will listen to all verbs
        route "/foo" (text "Bar")
        subRoute "/sub1" [
            subRoute "/sub2" [
                route "/test" handler1
            ]
        ]

        subRoute "/auth" ([
            route "/open" handler1
            route "/closed" handler1 |> addMetadata (RequiresAuditAttribute())
        ] |> List.map (applyBefore authHandler))
    ]

let notFoundHandler (ctx: HttpContext) =
    ctx.SetStatusCode 404
    ctx.WriteTextAsync("Page not found!") :> Task

let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseOxpecker(endpoints)
        .Run(notFoundHandler)

[<EntryPoint>]
let main args =
    let app = WebApplication.Create(args)
    configureApp app
    app.Run()
    0
