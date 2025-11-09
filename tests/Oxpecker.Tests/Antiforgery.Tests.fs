module Oxpecker.Tests.Antiforgery

open System.Collections.Generic
open System.Net
open System.Net.Http
open System.Net.Http.Json
open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Net.Http.Headers
open Oxpecker
open Xunit
open FsUnit.Light

module WebApp =

    let webApp (endpoints: Endpoint seq) =
        task {
            let host =
                HostBuilder()
                    .ConfigureWebHost(fun webHostBuilder ->
                        webHostBuilder
                            .UseTestServer()
                            .Configure(
                                _.UseRouting()
                                    .UseAntiforgery()
                                    .UseOxpecker(endpoints)
                                    .Run(DefaultHandlers.notFoundHandler)
                            )
                            .ConfigureServices(fun services ->
                                services.AddRouting().AddAntiforgery().AddOxpecker() |> ignore)
                        |> ignore)
                    .Build()
            do! host.StartAsync()
            return host
        }

    let webAppWithDefaultErrorHandler (endpoints: Endpoint seq) =
        task {
            let host =
                HostBuilder()
                    .ConfigureWebHost(fun webHostBuilder ->
                        webHostBuilder
                            .UseTestServer()
                            .Configure(
                                _.UseRouting()
                                    .UseAntiforgery()
                                    .Use(DefaultHandlers.errorHandler)
                                    .UseOxpecker(endpoints)
                                    .Run(DefaultHandlers.notFoundHandler)
                            )
                            .ConfigureServices(fun services ->
                                services.AddRouting().AddAntiforgery().AddOxpecker() |> ignore)
                        |> ignore)
                    .Build()
            do! host.StartAsync()
            return host
        }


[<Fact>]
let ``Request fails when antiforgery token is missing`` () =
    task {
        let endpoints = [
            POST [
                route "/action" (fun ctx ->
                    let _ = ctx.BindForm<unit>()
                    ctx.WriteText "Hello World")
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()

        do!
            client.PostAsync("/action", null)
            |> shouldFailTask<AntiforgeryValidationException>
    }

[<Fact>]
let ``Server returns 403 with default error handler and antiforgery token is missing`` () =
    task {
        let endpoints = [
            POST [
                route "/action" (fun ctx ->
                    let _ = ctx.BindForm<unit>()
                    ctx.WriteText "Hello World")
            ]
        ]
        use! server = WebApp.webAppWithDefaultErrorHandler endpoints
        let client = server.GetTestClient()

        let! result = client.PostAsync("/action", null)
        result.StatusCode |> shouldEqual HttpStatusCode.Forbidden
    }

[<Fact>]
let ``Request succeeds when antiforgery token is present`` () =
    task {
        let endpoints = [
            GET [
                route "/action" (fun ctx ->
                    let tokens = ctx.GetAntiforgeryTokens()
                    ctx.WriteJson tokens)
            ]
            POST [
                route "/action" (fun ctx ->
                    task {
                        let! msg = ctx.BindForm<{| Message: string |}>()
                        return! ctx.WriteText msg.Message
                    })
            ]
        ]
        use! server = WebApp.webApp endpoints
        let client = server.GetTestClient()
        let! response = client.GetAsync("/action")
        let! tokens = response.Content.ReadFromJsonAsync<AntiforgeryTokenSet>()
        let cookies = response.Headers.GetValues(HeaderNames.SetCookie)
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, cookies)
        let tokens = nonNull tokens
        use formContent =
            new FormUrlEncodedContent(
                [
                    KeyValuePair("Message", "Hi")
                    KeyValuePair(tokens.FormFieldName, tokens.RequestToken)
                ]
            )
        let! result = client.PostAsync("/action", formContent)
        let! text = result.Content.ReadAsStringAsync()
        text |> shouldEqual "Hi"
    }
