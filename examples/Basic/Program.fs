open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Oxpecker
open Oxpecker.Routing

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
    setHttpHeaderMw "foo" "var"
    >=> _.WriteTextAsync(sprintf "Hello %s, you are %i years old." firstName age)

let handler3 (a: string) (b: string) (c: string) (d: int): EndpointHandler =
    _.WriteTextAsync(sprintf "Hello %s %s %s %i" a b c d)

let endpoints =
    [
        subRoute "/foo" [
            GET [
                route "/bar" (setHttpHeaderMw "foo" "bar" >=> setHttpHeaderMw "moo" "gar" >=> text "FooBar!")
            ]
        ]
        GET [
            route  "/" (text "Hello World")
            routef "/{%s}/{%i}" handler2
            routef "/{%s}/{%s}/{%s}/{%i}" handler3
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

let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseOxpecker(endpoints)
    |> ignore

let configureServices (services: IServiceCollection) =
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
