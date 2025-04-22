namespace PerfTest

open System
open System.Net.Http
open System.Net.Http.Headers
open System.Threading.Tasks
open BenchmarkDotNet.Attributes
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection

[<CLIMutable>]
type BindingModelChild = { Name: string | null; Age: int }

[<CLIMutable>]
type BindingModel = {
    Id: Guid
    FirstName: string | null
    MiddleName: string | null
    LastName: string | null
    BirthDate: DateTime
    StatusCode: int
    Children: BindingModelChild[]
}


module OxpeckerBinder =
    open Oxpecker

    let bindModel (ctx: HttpContext) =
        task {
            let! model = ctx.BindForm<BindingModel>()
            return! setStatusCode model.StatusCode ctx
        }
        :> Task

    let endpoints = [ POST [ route "/bindModel" bindModel ] ]

    let webApp () =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(fun app -> app.UseRouting().UseOxpecker(endpoints) |> ignore)
                .ConfigureServices(fun services -> services.AddRouting().AddOxpecker() |> ignore)
        new TestServer(builder)

module GiraffeBinder =
    open Giraffe

    let bindModel: HttpHandler =
        fun next (ctx: HttpContext) ->
            task {
                let! model = ctx.BindFormAsync<BindingModel>()
                return! setStatusCode model.StatusCode next ctx
            }

    let endpoints = choose [ POST >=> route "/bindModel" >=> bindModel ]

    let webApp () =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(fun app -> app.UseGiraffe(endpoints))
                .ConfigureServices(fun services -> services.AddGiraffe() |> ignore)
        new TestServer(builder)

module MinimalApiBinder =

    open PerfTest.Csharp

    let webApp () =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(fun app ->
                    app.UseRouting().UseEndpoints(fun x -> ModelBindingTest.MapEndpoints(x))
                    |> ignore)
                .ConfigureServices(fun services -> services.AddRouting() |> ignore)
        new TestServer(builder)

[<MemoryDiagnoser>]
type ModelBinding() =

    // BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
    // AMD Ryzen 5 5600H with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
    // .NET SDK 9.0.201
    //   [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2 DEBUG
    //   DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
    //
    //
    // | Method         | Mean      | Error    | StdDev   | Median    | Gen0   | Allocated |
    // |--------------- |----------:|---------:|---------:|----------:|-------:|----------:|
    // | OxpeckerPost   |  31.00 us | 0.617 us | 1.406 us |  30.46 us | 1.9531 |  16.15 KB |
    // | GiraffePost    | 137.88 us | 1.007 us | 0.942 us | 137.71 us | 7.8125 |  65.42 KB |
    // | MinimalApiPost |  28.71 us | 0.526 us | 0.492 us |  28.58 us | 1.7090 |  14.55 KB |


    let oxpeckerServer = OxpeckerBinder.webApp()
    let giraffeServer = GiraffeBinder.webApp()
    let minimalApiServer = MinimalApiBinder.webApp()
    let oxpeckerClient = oxpeckerServer.CreateClient()
    let giraffeClient = giraffeServer.CreateClient()
    let minimalApiClient = minimalApiServer.CreateClient()

    let requestBody =
        "Id=4e213598-5b45-4cd7-a87c-429d7b6b2f03&FirstName=Susan&MiddleName=Elisabeth&LastName=Doe&BirthDate=1986-12-29&StatusCode=200&Nicknames=Susi&Nicknames=Eli&Nicknames=Liz&Children[0].Name=Hamed&Children[0].Age=32&Children[1].Name=Ali&Children[1].Age=22&Children[2].Name=Gholi&Children[2].Age=44"


    [<Benchmark>]
    member this.OxpeckerPost() =
        task {
            use content =
                new StringContent(requestBody, MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded"))
            return! oxpeckerClient.PostAsync("/bindModel", content)
        }

    [<Benchmark>]
    member this.GiraffePost() =
        task {
            use content =
                new StringContent(requestBody, MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded"))
            return! giraffeClient.PostAsync("/bindModel", content)
        }

    [<Benchmark>]
    member this.MinimalApiPost() =
        task {
            use content =
                new StringContent(requestBody, MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded"))
            return! minimalApiClient.PostAsync("/bindModel", content)
        }
