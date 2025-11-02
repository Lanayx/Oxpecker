module Oxpecker.Tests.Antiforgery

open System.Collections.Generic
open System.Net.Http
open System.Net.Http.Json
open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Net.Http.Headers
open Oxpecker
open Xunit
open FsUnit.Light

module WebApp =

    let notFoundHandler = setStatusCode 404 >=> text "Not found"

    let webApp (endpoints: Endpoint seq) =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(
                    _.UseRouting()
                        .UseAntiforgery()
                        .UseOxpecker(endpoints)
                        .Run(notFoundHandler)
                )
                .ConfigureServices(fun services ->
                    services
                        .AddRouting()
                        .AddAntiforgery()
                        .AddOxpecker() |> ignore)
        new TestServer(builder)


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
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()

        do! client.PostAsync("/action", null)
            |> shouldFailTask<AntiforgeryValidationException>
    }

[<Fact>]
let ``Request succeeds when antiforgery token is present`` () =
    task {
        let endpoints = [
            GET [
                route "/action" (fun ctx ->
                    let tokens = ctx.GetAntiforgeryTokens()
                    ctx.WriteJson tokens
                )
            ]
            POST [
                route "/action" (fun ctx -> task {
                    let! msg = ctx.BindForm<{| Message: string |}>()
                    return! ctx.WriteText msg.Message
                })
            ]
        ]
        let server = WebApp.webApp endpoints
        let client = server.CreateClient()
        let! response = client.GetAsync("/action")
        let! tokens = response.Content.ReadFromJsonAsync<AntiforgeryTokenSet>()
        let cookies = response.Headers.GetValues(HeaderNames.SetCookie)
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, cookies)
        let tokens = nonNull tokens
        use formContent = new FormUrlEncodedContent([
            KeyValuePair("Message", "Hi")
            KeyValuePair(tokens.FormFieldName, tokens.RequestToken)
        ])
        let! result = client.PostAsync("/action", formContent)
        let! text = result.Content.ReadAsStringAsync()
        text |> shouldEqual "Hi"
    }
