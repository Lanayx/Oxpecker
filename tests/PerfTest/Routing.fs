namespace PerfTest

open BenchmarkDotNet.Attributes
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection

module OxpeckerRouting =
    open Oxpecker

    let endpoints = [
        subRoute "/api1" [
            GET [ route "/users" <| text "Users received" ]
            GET [ routef "/user/{%s}" <| fun id ctx -> text $"User {id} received" ctx ]
            GET [ route "/json" <| json {| Name = "User" |} ]
        ]
        subRoute "/api2" [
            GET [ route "/users" <| text "Users received" ]
            GET [ routef "/user/{%s}" <| fun id ctx -> text $"User {id} received" ctx ]
            GET [ route "/json" <| json {| Name = "User" |} ]
        ]
        subRoute "/api3" [
            GET [ route "/users" <| text "Users received" ]
            GET [ routef "/user/{%s}" <| fun id ctx -> text $"User {id} received" ctx ]
            GET [ route "/json" <| json {| Name = "User" |} ]
        ]
        subRoute "/api" [
            GET [ route "/users" <| text "Users received" ]
            GET [ routef "/user/{%s}" <| fun id ctx -> text $"User {id} received" ctx ]
            GET [ route "/json" <| json {| Name = "User" |} ]
        ]
    ]

    let webApp () =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(fun app -> app.UseRouting().UseOxpecker(endpoints) |> ignore)
                .ConfigureServices(fun services -> services.AddRouting().AddOxpecker() |> ignore)
        new TestServer(builder)

module GiraffeRouting =
    open Giraffe

    let endpoints =
        choose [
            subRoute
                "/api1"
                (choose [
                    GET >=> route "/users" >=> text "Users received"
                    GET
                    >=> routef "/user/%s" (fun id next ctx -> text $"User {id} received" next ctx)
                    GET >=> route "/json" >=> json {| Name = "User" |}
                ])
            subRoute
                "/api2"
                (choose [
                    GET >=> route "/users" >=> text "Users received"
                    GET
                    >=> routef "/user/%s" (fun id next ctx -> text $"User {id} received" next ctx)
                    GET >=> route "/json" >=> json {| Name = "User" |}
                ])
            subRoute
                "/api3"
                (choose [
                    GET >=> route "/users" >=> text "Users received"
                    GET
                    >=> routef "/user/%s" (fun id next ctx -> text $"User {id} received" next ctx)
                    GET >=> route "/json" >=> json {| Name = "User" |}
                ])
            subRoute
                "/api"
                (choose [
                    GET >=> route "/users" >=> text "Users received"
                    GET
                    >=> routef "/user/%s" (fun id next ctx -> text $"User {id} received" next ctx)
                    GET >=> route "/json" >=> json {| Name = "User" |}
                ])
        ]

    let webApp () =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(fun app -> app.UseGiraffe(endpoints))
                .ConfigureServices(fun services -> services.AddGiraffe() |> ignore)
        new TestServer(builder)

[<MemoryDiagnoser>]
type Routing() =
    // BenchmarkDotNet v0.13.12, Windows 10
    // AMD Ryzen 7 2700X, 1 CPU, 16 logical and 8 physical cores
    // .NET SDK 8.0.101
    //   [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2 DEBUG
    //   DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
    //
    //
    // | Method            | Mean     | Error    | StdDev   | Median   | Gen0   | Allocated |
    // |------------------ |---------:|---------:|---------:|---------:|-------:|----------:|
    // | GetOxpeckerRoute  | 12.10 us | 0.311 us | 0.871 us | 11.98 us | 1.9531 |   8.09 KB |
    // | GetOxpeckerRoutef | 21.05 us | 0.418 us | 0.764 us | 20.85 us | 2.1973 |   9.34 KB |
    // | GetOxpeckerJson   | 18.11 us | 0.361 us | 0.971 us | 18.09 us | 2.0752 |   8.77 KB |
    // | GetGiraffeRoute   | 15.22 us | 0.379 us | 1.082 us | 15.06 us | 2.3193 |    9.5 KB |
    // | GetGiraffeRoutef  | 23.17 us | 0.548 us | 1.553 us | 22.71 us | 2.9297 |  12.02 KB |
    // | GetGiraffeJson    | 21.60 us | 0.975 us | 2.874 us | 20.52 us | 2.0752 |   8.78 KB |


    let oxpeckerServer = OxpeckerRouting.webApp()
    let giraffeServer = GiraffeRouting.webApp()
    let oxpeckerClient = oxpeckerServer.CreateClient()
    let giraffeClient = giraffeServer.CreateClient()


    [<Benchmark>]
    member this.GetOxpeckerRoute() = oxpeckerClient.GetAsync("/api/users")

    [<Benchmark>]
    member this.GetOxpeckerRoutef() = oxpeckerClient.GetAsync("/api/user/1")

    [<Benchmark>]
    member this.GetOxpeckerJson() = oxpeckerClient.GetAsync("/api/json")

    [<Benchmark>]
    member this.GetGiraffeRoute() = giraffeClient.GetAsync("/api/users")

    [<Benchmark>]
    member this.GetGiraffeRoutef() = giraffeClient.GetAsync("/api/user/1")

    [<Benchmark>]
    member this.GetGiraffeJson() = oxpeckerClient.GetAsync("/api/json")
