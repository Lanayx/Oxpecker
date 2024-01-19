namespace PerfTest

open System.IO
open BenchmarkDotNet.Attributes
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection

module OxpeckerRouting =
    open Oxpecker

    let endpoints =
        [
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

    let webApp() =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(fun app ->
                    app
                        .UseRouting()
                        .UseOxpecker(endpoints)|> ignore
                ).ConfigureServices(fun services ->
                    services
                        .AddRouting()
                        .AddOxpecker() |> ignore
                )
        new TestServer(builder)

module GiraffeRouting =
    open Giraffe

    let endpoints =
        choose [
            subRoute "/api1" (choose [
                GET >=> route "/users" >=> text "Users received"
                GET >=> routef "/user/%s" (fun id next ctx -> text $"User {id} received" next ctx)
                GET >=> route "/json" >=> json {| Name = "User" |}
            ])
            subRoute "/api2" (choose [
                GET >=> route "/users" >=> text "Users received"
                GET >=> routef "/user/%s" (fun id next ctx -> text $"User {id} received" next ctx)
                GET >=> route "/json" >=> json {| Name = "User" |}
            ])
            subRoute "/api3" (choose [
                GET >=> route "/users" >=> text "Users received"
                GET >=> routef "/user/%s" (fun id next ctx -> text $"User {id} received" next ctx)
                GET >=> route "/json" >=> json {| Name = "User" |}
            ])
            subRoute "/api" (choose [
                GET >=> route "/users" >=> text "Users received"
                GET >=> routef "/user/%s" (fun id next ctx -> text $"User {id} received" next ctx)
                GET >=> route "/json" >=> json {| Name = "User" |}
            ])
        ]

    let webApp() =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(fun app ->
                    app.UseGiraffe(endpoints)
                ).ConfigureServices(fun services ->
                    services
                        .AddGiraffe() |> ignore
                )
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
// | GetOxpeckerRoute  | 15.00 us | 0.649 us | 1.863 us | 14.37 us | 1.9531 |    8.1 KB |
// | GetOxpeckerRoutef | 21.52 us | 0.414 us | 0.808 us | 21.37 us | 2.1973 |   9.35 KB |
// | GetOxpeckerJson   | 17.00 us | 0.359 us | 1.014 us | 16.75 us | 2.0752 |   8.77 KB |
// | GetGiraffeRoute   | 16.98 us | 0.338 us | 0.883 us | 16.90 us | 2.4414 |  10.23 KB |
// | GetGiraffeRoutef  | 24.32 us | 0.481 us | 1.207 us | 24.21 us | 3.0518 |  12.81 KB |
// | GetGiraffeJson    | 18.13 us | 0.361 us | 0.918 us | 18.01 us | 2.0752 |   8.77 KB |

    let oxpeckerServer = OxpeckerRouting.webApp()
    let giraffeServer = GiraffeRouting.webApp()
    let oxpeckerClient = oxpeckerServer.CreateClient()
    let giraffeClient = giraffeServer.CreateClient()


    [<Benchmark>]
    member this.GetOxpeckerRoute () =
        oxpeckerClient.GetAsync("/api/users")

    [<Benchmark>]
    member this.GetOxpeckerRoutef () =
        oxpeckerClient.GetAsync("/api/user/1")

    [<Benchmark>]
    member this.GetOxpeckerJson () =
        oxpeckerClient.GetAsync("/api/json")

    [<Benchmark>]
    member this.GetGiraffeRoute () =
        giraffeClient.GetAsync("/api/users")

    [<Benchmark>]
    member this.GetGiraffeRoutef () =
        giraffeClient.GetAsync("/api/user/1")

    [<Benchmark>]
    member this.GetGiraffeJson () =
        oxpeckerClient.GetAsync("/api/json")