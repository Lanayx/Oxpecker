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
            GET [ routef "/user/{%s}/{%s}" <| fun id name -> text "User received" ]
            GET [ route "/json" <| json {| Name = "User" |} ]
        ]
        subRoute "/api2" [
            GET [ route "/users" <| text "Users received" ]
            GET [ routef "/user/{%s}/{%s}" <| fun id name -> text "User received" ]
            GET [ route "/json" <| json {| Name = "User" |} ]
        ]
        subRoute "/api3" [
            GET [ route "/users" <| text "Users received" ]
            GET [ routef "/user/{%s}/{%s}" <| fun id name -> text "User received" ]
            GET [ route "/json" <| json {| Name = "User" |} ]
        ]
        subRoute "/api" [
            GET [ route "/users" <| text "Users received" ]
            GET [ routef "/user/{%s}/{%s}" <| fun id name -> text "User received" ]
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
                    GET >=> routef "/user/%s/%s" (fun (id, name) -> text "User received")
                    GET >=> route "/json" >=> json {| Name = "User" |}
                ])
            subRoute
                "/api2"
                (choose [
                    GET >=> route "/users" >=> text "Users received"
                    GET >=> routef "/user/%s/%s" (fun (id, name) -> text "User received")
                    GET >=> route "/json" >=> json {| Name = "User" |}
                ])
            subRoute
                "/api3"
                (choose [
                    GET >=> route "/users" >=> text "Users received"
                    GET >=> routef "/user/%s/%s" (fun (id, name) -> text "User received")
                    GET >=> route "/json" >=> json {| Name = "User" |}
                ])
            subRoute
                "/api"
                (choose [
                    GET >=> route "/users" >=> text "Users received"
                    GET >=> routef "/user/%s/%s" (fun (id, name) -> text "User received")
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

    // BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3296/23H2/2023Update/SunValley3)
    // AMD Ryzen 5 5600H with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
    // .NET SDK 8.0.200
    //   [Host]     : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2 DEBUG
    //   DefaultJob : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2
    //
    // | Method            | Mean      | Error     | StdDev    | Gen0   | Allocated |
    // |------------------ |----------:|----------:|----------:|-------:|----------:|
    // | GetOxpeckerRoute  |  8.706 us | 0.1729 us | 0.3289 us | 0.9766 |   8.08 KB |
    // | GetOxpeckerRoutef |  9.138 us | 0.1807 us | 0.2219 us | 0.9766 |    8.4 KB |
    // | GetGiraffeRoute   |  9.611 us | 0.1918 us | 0.4741 us | 1.0986 |   9.48 KB |
    // | GetGiraffeRoutef  | 26.128 us | 0.5105 us | 0.7948 us | 1.4648 |  13.11 KB |

    let oxpeckerServer = OxpeckerRouting.webApp()
    let giraffeServer = GiraffeRouting.webApp()
    let oxpeckerClient = oxpeckerServer.CreateClient()
    let giraffeClient = giraffeServer.CreateClient()


    [<Benchmark>]
    member this.GetOxpeckerRoute() = oxpeckerClient.GetAsync("/api/users")

    [<Benchmark>]
    member this.GetOxpeckerRoutef() =
        oxpeckerClient.GetAsync("/api/user/1/don")

    [<Benchmark>]
    member this.GetGiraffeRoute() = giraffeClient.GetAsync("/api/users")

    [<Benchmark>]
    member this.GetGiraffeRoutef() =
        giraffeClient.GetAsync("/api/user/1/don")
