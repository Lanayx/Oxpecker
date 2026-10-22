module OxpBench.Program

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Oxpecker
open Oxpecker.ViewEngine

open OxpBench.Endpoints

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.Logging.ClearProviders() |> ignore
    builder.Services.AddRouting().AddOxpecker() |> ignore
    let app = builder.Build()
    app.UseRouting() |> ignore
    // raw ASP.NET endpoint (not Oxpecker) used by the driver to read server-side counters
    app.MapGet(
        "/__stats",
        Func<HttpContext, Task>(fun ctx ->
            ctx.Response.WriteAsync(
                $"{GC.GetTotalAllocatedBytes(true)} {GC.CollectionCount(0)} {GC.CollectionCount(1)} {GC.CollectionCount(2)} {Environment.ProcessorCount} {Diagnostics.Process.GetCurrentProcess().TotalProcessorTime.Ticks}"
            ))
    )
    |> ignore
    app.UseOxpecker(endpoints) |> ignore
    let port =
        Environment.GetEnvironmentVariable("BENCH_PORT")
        |> Option.ofObj
        |> Option.defaultValue "5099"
    app.Run($"http://127.0.0.1:{port}")
    0
