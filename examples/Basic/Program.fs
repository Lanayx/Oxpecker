open System
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Oxpecker
open Oxpecker.Routing

let handler1 : HttpHandler =
    fun (_ : HttpFunc) (ctx : HttpContext) ->
        ctx.WriteTextAsync "Hello World"

let handler2 (firstName : string, age : int) : HttpHandler =
    fun (_ : HttpFunc) (ctx : HttpContext) ->
        ctx.WriteTextAsync "handler2"

let handler3 (a : string, b : string, c : string, d : int) : HttpHandler =
    fun (_ : HttpFunc) (ctx : HttpContext) ->
        sprintf "Hello %s %s %s %i" a b c d
        |> ctx.WriteTextAsync

let handler5 (firstName : string) (age : int): HttpHandler =
    fun (_ : HttpFunc) (ctx : HttpContext) ->
        ctx.WriteTextAsync "handler5"

let handler6 (firstName : string) (age : string) =
    fun (_ : HttpFunc) (ctx : HttpContext) ->
        sprintf "Hello %s, you are %s years old." firstName age
        |> ctx.WriteTextAsync


let xx = Oxpecker.Routing2.Routers.routef2 "55/{%s}/{%i}" handler5


let endpoints =
    [
        subRoute "/foo" [
            GET [
                route "/bar" (text "Aloha!")
            ]
        ]
        GET [
            route  "/" (text "Hello World")
            routef "22/%s:int/%i" handler2
            routef "/%s/%s/%s/%i" handler3
        ]
        GET_HEAD [
            route "/foo" (text "Bar")
            route "/x"   (text "y")
            route "/abc" (text "def")
        ]
        // Not specifying a http verb means it will listen to all verbs
        subRoute "/sub" [
            route "/test" handler1
        ]
    ]

let configureApp (appBuilder : IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseOxpecker(endpoints)
        .UseOxpecker2([xx])
    |> ignore

let configureServices (services : IServiceCollection) =
    services
        .AddRouting()
    |> ignore


[<EntryPoint>]
let main args =
    // WebHostBuilder()
    //     .UseKestrel()
    //     .Configure(configureApp)
    //     .ConfigureServices(configureServices)
    //     .Build()
    //     .Run()

    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services

    let app = builder.Build()

    if app.Environment.IsDevelopment() then
        app.UseDeveloperExceptionPage() |> ignore

    configureApp app
    app.Run()

    0
