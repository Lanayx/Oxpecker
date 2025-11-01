open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Oxpecker

let endpoints = [
    route "/" <| text "Hello World!"
]

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.Services.AddRouting().AddOxpecker() |> ignore
    let app = builder.Build()
    app.UseRouting()
        .Use(DefaultHandlers.errorHandler)
        .UseOxpecker(endpoints)
        .Run(DefaultHandlers.notFoundHandler)
    app.Run()
    0 // Exit code
