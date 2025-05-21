open System
open System.ComponentModel
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open ModelContextProtocol.Server
open Oxpecker

[<McpServerToolType>]
type UtcTimeTool() =

    [<McpServerTool>]
    [<Description("Gets the current UTC time.")>]
    static member GetCurrentTime() : string =
        DateTime.UtcNow.ToString()

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
            .WithTools<UtcTimeTool>()
            .WithHttpTransport()
        |> ignore
    let app = builder.Build()
    app
        .UseRouting()
        .UseOxpecker(endpoints)
        |> ignore
    // Add the MCP endpoint
    app.MapMcp("mcp") |> ignore
    app.Run()
    0
