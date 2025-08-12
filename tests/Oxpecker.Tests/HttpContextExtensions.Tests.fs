module Oxpecker.Tests.HttpContextExtensions

open System
open System.IO
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.WebUtilities
open Microsoft.Extensions.DependencyInjection
open Oxpecker.ViewEngine
open Xunit
open FsUnit.Light
open Oxpecker


#nowarn "3391"

[<Fact>]
let ``GetRequestUrl returns entire URL of the HTTP request`` () =
    let ctx = DefaultHttpContext()
    ctx.Request.Scheme <- "http"
    ctx.Request.Host <- HostString("example.org:81")
    ctx.Request.PathBase <- PathString("/something")
    ctx.Request.Path <- PathString("/hello")
    ctx.Request.QueryString <- QueryString("?a=1&b=2")
    ctx.Request.Method <- "GET"
    ctx.Response.Body <- new MemoryStream()

    let result = ctx.GetRequestUrl()

    result |> shouldEqual "http://example.org:81/something/hello?a=1&b=2"

[<Fact>]
let ``TryGetRequestHeader during HTTP GET request with returns correct result`` () =
    let ctx = DefaultHttpContext()
    ctx.TryGetHeaderValue "X-Test" |> shouldEqual None
    ctx.Request.Headers.Add("X-Test", "It works!")

    let result = ctx.TryGetHeaderValue "X-Test"

    result |> shouldEqual(Some "It works!")

[<Fact>]
let ``TryGetQueryStringValue during HTTP GET request with query string returns correct result`` () =
    let ctx = DefaultHttpContext()
    ctx.TryGetQueryValue "BirthDate" |> shouldEqual None
    let queryStr =
        "?Name=John%20Doe&IsVip=true&BirthDate=1990-04-20&Balance=150000.5&LoyaltyPoints=137"
    let query = QueryHelpers.ParseQuery queryStr
    ctx.Request.Query <- QueryCollection(query)

    let result = ctx.TryGetQueryValue "BirthDate"

    result |> shouldEqual(Some "1990-04-20")

[<Fact>]
let ``WriteText with HTTP GET should return text in body`` () =
    task {
        let ctx = DefaultHttpContext()
        ctx.Response.Body <- new MemoryStream()

        do! ctx.WriteText "Hello World"

        ctx.Response.Body.Seek(0, SeekOrigin.Begin) |> ignore
        use reader = new StreamReader(ctx.Response.Body)
        let result = reader.ReadToEnd()
        result |> shouldEqual "Hello World"
    }

[<Fact>]
let ``WriteText with HTTP HEAD should not return text in body`` () =
    task {
        let ctx = DefaultHttpContext()
        ctx.Response.Body <- new MemoryStream()
        ctx.Request.Method <- "HEAD"

        do! ctx.WriteText "Hello World"

        ctx.Response.Body.Seek(0, SeekOrigin.Begin) |> ignore
        use reader = new StreamReader(ctx.Response.Body)
        reader.ReadToEnd() |> shouldEqual ""
    }

[<Fact>]
let ``WriteJson should add json to the context`` () =
    task {
        let ctx = DefaultHttpContext()
        ctx.Response.Body <- new MemoryStream()
        let services = ServiceCollection()
        services.AddSingleton<IJsonSerializer>(fun sp -> SystemTextJsonSerializer() :> IJsonSerializer)
        |> ignore
        ctx.RequestServices <- DefaultServiceProviderFactory().CreateServiceProvider(services)

        do! ctx.WriteJson({| Hello = "World" |})

        ctx.Response.Body.Seek(0, SeekOrigin.Begin) |> ignore
        use reader = new StreamReader(ctx.Response.Body)
        let result = reader.ReadToEnd()
        ctx.Response.Headers.ContentType
        |> shouldEqual "application/json; charset=utf-8"
        ctx.Response.Headers.ContentLength |> shouldEqual 17L
        result |> shouldEqual """{"hello":"World"}"""
    }

[<Fact>]
let ``WriteJsonChunked should add json to the context`` () =
    task {
        let ctx = DefaultHttpContext()
        ctx.Response.Body <- new MemoryStream()
        let services = ServiceCollection()
        services.AddSingleton<IJsonSerializer>(fun sp -> SystemTextJsonSerializer() :> IJsonSerializer)
        |> ignore
        ctx.RequestServices <- DefaultServiceProviderFactory().CreateServiceProvider(services)

        do! ctx.WriteJsonChunked {| Hello = "World" |}

        ctx.Response.Body.Seek(0, SeekOrigin.Begin) |> ignore
        use reader = new StreamReader(ctx.Response.Body)
        let result = reader.ReadToEnd()
        ctx.Response.Headers.ContentType
        |> shouldEqual "application/json; charset=utf-8"
        ctx.Response.Headers.ContentLength |> shouldEqual(Nullable())
        result |> shouldEqual """{"hello":"World"}"""
    }

[<Fact>]
let ``WriteHtmlViewAsync should add html to the context`` () =
    task {
        let ctx = DefaultHttpContext()
        ctx.Response.Body <- new MemoryStream()
        let htmlDoc =
            html() {
                head()
                body() { h1(id = "header") { "Hello world" } }
            }
        do! ctx.WriteHtmlView(htmlDoc)

        ctx.Response.Body.Seek(0, SeekOrigin.Begin) |> ignore
        use reader = new StreamReader(ctx.Response.Body)
        let result = reader.ReadToEnd()

        result
        |> shouldEqual
            $"""<!DOCTYPE html>{Environment.NewLine}<html><head></head><body><h1 id="header">Hello world</h1></body></html>"""
    }
