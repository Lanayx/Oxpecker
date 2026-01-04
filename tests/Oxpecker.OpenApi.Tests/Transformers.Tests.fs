module Oxpecker.OpenApi.Tests.Transformers

open System
open System.Net
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Oxpecker.OpenApi
open Xunit
open FsUnit.Light
open Oxpecker

module WebApp =

    let webApp (endpoints: Endpoint seq) =
        task {
            let host =
                HostBuilder()
                    .ConfigureWebHost(fun webHostBuilder ->
                        webHostBuilder
                            .UseTestServer()
                            .Configure(fun app ->
                                app.UseRouting().UseEndpoints(fun builder ->
                                    builder.MapOxpeckerEndpoints(endpoints)
                                    builder.MapOpenApi() |> ignore
                                ) |> ignore)
                            .ConfigureServices(fun services ->
                                services.AddRouting().AddOpenApi(fun o ->
                                    o.AddSchemaTransformer<FSharpOptionSchemaTransformer>() |> ignore
                                ) |> ignore)
                        |> ignore)
                    .Build()
            do! host.StartAsync()
            return host
        }
    let webAppOneRoute (endpoint: Endpoint) =
        task {
            let host =
                HostBuilder()
                    .ConfigureWebHost(fun webHostBuilder ->
                        webHostBuilder
                            .UseTestServer()
                            .Configure(fun app -> app.UseRouting().UseOxpecker(endpoint).Run(Default.notFoundHandler))
                            .ConfigureServices(fun services -> services.AddRouting() |> ignore)
                        |> ignore)
                    .Build()
            do! host.StartAsync()
            return host
        }

[<Fact>]
let ``route: GET "/foo" returns "bar"`` () =
    task {
        let endpoints = [ GET [
            route "/" <| text "Hello World"
            route "/foo" <| text "bar"
        ] ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        let! result = client.GetAsync("/foo")
        let! resultString = result.Content.ReadAsStringAsync()

        result.StatusCode |> shouldEqual HttpStatusCode.OK
        resultString |> shouldEqual "bar"
    }
