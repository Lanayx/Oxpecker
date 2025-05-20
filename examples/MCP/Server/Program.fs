open System
open System.ComponentModel
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open ModelContextProtocol.Server
open Oxpecker

[<Sealed>]
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
    builder.Logging.AddConsole(fun options ->
        options.LogToStandardErrorThreshold <- LogLevel.Trace
    ) |> ignore
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
    app.MapMcp() |> ignore
    app.Run()
    0 // Exit code
