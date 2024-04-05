module CRUD.Program

open System.Threading.Tasks
open CRUD.Env
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Oxpecker
open type Microsoft.AspNetCore.Http.TypedResults

let getEndpoints env = [
    subRoute "/order" [
        GET [
            route "/" <| Handlers.getOrders env
            routef "/{%O:guid}" <| Handlers.getOrderDetails env
        ]
        POST [ route "/" <| Handlers.createOrder env ]
        PUT [ routef "/{%O:guid}" <| Handlers.updateOrder env ]
        DELETE [ routef "/{%O:guid}" <| Handlers.deleteOrder env ]
    ]
]

let notFoundHandler (ctx: HttpContext) =
    let logger = ctx.GetLogger()
    logger.LogWarning("Unhandled 404 error")
    ctx.Write <| NotFound {| Error = "Resource was not found" |}

let errorHandler (ctx: HttpContext) (next: RequestDelegate) =
    task {
        try
            return! next.Invoke(ctx)
        with
        | :? ModelBindException
        | :? RouteParseException as ex ->
            let logger = ctx.GetLogger()
            logger.LogWarning(ex, "Unhandled 400 error")
            return! ctx.Write <| BadRequest {| Error = ex.Message |}
        | ex ->
            let logger = ctx.GetLogger()
            logger.LogError(ex, "Unhandled 500 error")
            ctx.SetStatusCode StatusCodes.Status500InternalServerError
            return! ctx.WriteText <| string ex
    }
    :> Task

let configureApp (appBuilder: IApplicationBuilder) =
    let env = {
        DbClient = Database.Fake.fakeClient
        Logger = appBuilder.ApplicationServices.GetService<ILogger>()
    }
    appBuilder
        .UseRouting()
        .Use(errorHandler)
        .UseOxpecker(getEndpoints env)
        .Run(notFoundHandler)

let configureServices (services: IServiceCollection) =
    services.AddRouting().AddOxpecker() |> ignore

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services
    let app = builder.Build()
    configureApp app
    app.Run()
    0
