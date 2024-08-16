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
    app.UseRouting().UseOxpecker(endpoints) |> ignore
    app.Run()
    0 // Exit code
