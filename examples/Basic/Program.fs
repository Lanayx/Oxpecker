open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
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
        ctx.WriteJson { a with Name = a.Name + "!" }

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

let authHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        if ctx.Request.Path.Value.Contains("closed") then
            ctx.SetStatusCode 401
            json {| Status = "Unauthorized" |} ctx
        else
            Task.CompletedTask

let MY_AUTH = applyBefore authHandler |> Seq.map

let endpoints =
    [
        GET [
            route "/" (text "Hello World")
            routef "/{%s}" (setHttpHeaderMw "foo" "moo" >>=> handler0)
            routef "/{%s}/{%i}" (setHttpHeaderMw "foo" "var" >>=>+ handler2)
            routef "/{%s}/{%s}/{%s}/{%i:min(15)}" handler3
            route "/x" (bindQuery handler4)
            routef "/x/{%s}/{%s}" (bindQuery <<+ handler5)
            routef "/xx/{%s}" (setHttpHeaderMw "foo" "xx" >>=> bindQuery << handler6)
            routef "/xx/{%s}/{%s}" (setHttpHeaderMw "foo" "xx" >>=>+ (bindQuery <<+ handler5))
        ]
        POST [
            route "/x" (bindJson handler4)
            route "/y" (bindQuery (bindJson << handler7))
            routef "/y/{%s}" (bindQuery << (bindJson <<+ handler8))
            routef "/y/{%s}/{%s}" (setHttpHeaderMw "foo" "yy" >>=>+ (bindQuery <<+ (bindJson <<++ handler9)))
            route "/abc" (json {| X = "Y" |})
        ]
        // Not specifying a http verb means it will listen to all verbs
        route "/foo" (setHttpHeaderMw "foo" "bar" >=> text "Bar")
        subRoute "/sub1" [
            subRoute "/sub2" [
                route "/test" handler1
            ]
        ]

        subRoute "/auth" (
            MY_AUTH [
                route "/open" handler1
                route "/closed" handler1 |> addMetadata (RequiresAuditAttribute())
            ]
        )
    ]

let notFoundHandler (ctx: HttpContext) =
    ctx.SetStatusCode 404
    ctx.WriteText "Page not found!" :> Task

let errorHandler (ctx: HttpContext) (next: RequestDelegate) =
    task {
        try
            return! next.Invoke(ctx)
        with
        | :? ModelBindException
        | :? RouteParseException as ex ->
            ctx.SetStatusCode StatusCodes.Status400BadRequest
            return! ctx.WriteText <| string ex
        | ex ->
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
    |> ignore

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services
    let app = builder.Build()
    configureApp app
    app.Run()
    0
