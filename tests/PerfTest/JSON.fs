namespace PerfTest

open System.Buffers
open System.Threading.Tasks
open BenchmarkDotNet.Attributes
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open System
open Microsoft.IO

module Common =
    type User = {
        Name: string
        Age: int
        Created: DateTime
        Height: float
    }
    let users = [|
        {
            Name = "User1"
            Age = 20
            Created = DateTime.UtcNow
            Height = 170.2
        }
        {
            Name = "User2"
            Age = 21
            Created = DateTime.UtcNow
            Height = 170.3
        }
        {
            Name = "User3"
            Age = 22
            Created = DateTime.UtcNow
            Height = 170.4
        }
        {
            Name = "User4"
            Age = 30
            Created = DateTime.UtcNow
            Height = 180.2
        }
        {
            Name = "User5"
            Age = 31
            Created = DateTime.UtcNow
            Height = 180.3
        }
        {
            Name = "User6"
            Age = 32
            Created = DateTime.UtcNow
            Height = 180.4
        }
    |]

module STJ =
    open Oxpecker

    let endpoints = [
        GET [ route "/json" <| json Common.users ]
        GET [ route "/jsonChunked" <| jsonChunked Common.users ]
    ]

    let webApp () =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(fun app -> app.UseRouting().UseOxpecker(endpoints) |> ignore)
                .ConfigureServices(fun services -> services.AddRouting().AddOxpecker() |> ignore)
        new TestServer(builder)

module SpanJson =
    open Oxpecker
    open SpanJson

    let endpoints = [
        GET [ route "/json" <| json Common.users ]
        GET [ route "/jsonChunked" <| jsonChunked Common.users ]
    ]

    type SpanJsonSerializer() =
        interface IJsonSerializer with
            member this.Serialize(value, ctx, chunked) =
                ctx.Response.ContentType <- "application/json; charset=utf-8"
                if chunked then
                    if ctx.Request.Method <> HttpMethods.Head then
                        JsonSerializer.Generic.Utf8.SerializeAsync<_>(value, ctx.Response.Body).AsTask()
                    else
                        Task.CompletedTask
                else
                    task {
                        let buffer = JsonSerializer.Generic.Utf8.SerializeToArrayPool<_>(value)
                        ctx.Response.Headers.ContentLength <- buffer.Count
                        if ctx.Request.Method <> HttpMethods.Head then
                            do! ctx.Response.Body.WriteAsync(buffer)
                        ArrayPool<byte>.Shared.Return(buffer.Array)
                    }

            member this.Deserialize(ctx) = failwith "Not implemented"

    let webApp () =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(fun app -> app.UseRouting().UseOxpecker(endpoints) |> ignore)
                .ConfigureServices(fun services ->
                    services
                        .AddRouting()
                        .AddOxpecker()
                        .AddSingleton<IJsonSerializer>(SpanJsonSerializer())
                    |> ignore)
        new TestServer(builder)

[<MemoryDiagnoser>]
type JSON() =

    // BenchmarkDotNet v0.13.12, Windows 11
    // AMD Ryzen 5 5600H with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
    // .NET SDK 8.0.200
    //   [Host]     : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2 DEBUG
    //   DefaultJob : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2
    //
    //
    // | Method          | Mean     | Error    | StdDev   | Gen0   | Allocated |
    // |---------------- |---------:|---------:|---------:|-------:|----------:|
    // | STJ             | 18.46 us | 0.365 us | 0.832 us | 1.0986 |   9.55 KB |
    // | SpanJson        | 11.34 us | 0.222 us | 0.364 us | 0.9766 |   8.78 KB |
    // | STJChunked      | 14.34 us | 0.285 us | 0.614 us | 1.0986 |   9.13 KB |
    // | SpanJsonChunked | 10.69 us | 0.214 us | 0.407 us | 0.9766 |   8.65 KB |

    let stjServer = STJ.webApp()
    let spanJsonServer = SpanJson.webApp()
    let stjClient = stjServer.CreateClient()
    let spanJsonClient = spanJsonServer.CreateClient()


    [<Benchmark>]
    member this.STJ() = stjClient.GetAsync("/json")
    [<Benchmark>]
    member this.SpanJson() = spanJsonClient.GetAsync("/json")
    [<Benchmark>]
    member this.STJChunked() = stjClient.GetAsync("/jsonChunked")
    [<Benchmark>]
    member this.SpanJsonChunked() = spanJsonClient.GetAsync("/jsonChunked")
