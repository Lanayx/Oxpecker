open System
open System.ComponentModel
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open ModelContextProtocol.Server
open Oxpecker

[<McpServerToolType>]
type MyTools() =

    [<McpServerTool>]
    [<Description("Gets the current date.")>]
    static member GetCurrentDate() : string =
        DateTime.Now.ToLongDateString()

let endpoints = [
    route "/" <| text "Hello World!"
]

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.Services
        .AddRouting()
        .AddOxpecker()
        .AddMcpServer()
            .WithTools<MyTools>()
            .WithHttpTransport()
        |> ignore
    let app = builder.Build()
    app
        .UseRouting()
        .UseOxpecker(endpoints)
        |> ignore
    // Add MCP endpoint
    app.MapMcp("mcp") |> ignore
    app.Run()
    0
