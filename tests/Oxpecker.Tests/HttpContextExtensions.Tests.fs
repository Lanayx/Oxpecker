module Oxpecker.Tests.HttpContextExtensions

open System
open System.IO
open System.Text
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.WebUtilities
open Microsoft.Extensions.DependencyInjection
open Oxpecker.ViewEngine
open Xunit
open FsUnit.Light
open Oxpecker


#nowarn "3391"

type StringCollectionModel = { Tags: string list }

type ScalarCollectionModel = {
    Counts: int array
    Ratios: float list
    Flags: bool seq
}

let private createFormContext (body: string) =
    let ctx = DefaultHttpContext()
    ctx.Request.Body <- new MemoryStream(Encoding.UTF8.GetBytes body)
    ctx.Request.ContentType <- "application/x-www-form-urlencoded"
    let services = ServiceCollection()
    services.AddSingleton<IModelBinder>(ModelBinder()) |> ignore
    ctx.RequestServices <- services.BuildServiceProvider()
    ctx

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
let ``ReadFormAsync groups repeated string collection values`` () =
    task {
        let ctx = createFormContext "tags=dotnet&tags=mvc&tags=api"

        let! form = ctx.Request.ReadFormAsync()

        form.Keys |> Seq.toList |> shouldEqual [ "tags" ]
        form["tags"] |> Seq.toList |> shouldEqual [ "dotnet"; "mvc"; "api" ]
    }

[<Fact>]
let ``ReadFormAsync preserves indexed string collection keys`` () =
    task {
        let ctx = createFormContext "tags[0]=dotnet&tags[1]=mvc&tags[2]=api"

        let! form = ctx.Request.ReadFormAsync()

        form.Keys |> Set.ofSeq |> shouldEqual(set [ "tags[0]"; "tags[1]"; "tags[2]" ])
        form["tags[0]"] |> string |> shouldEqual "dotnet"
        form["tags[1]"] |> string |> shouldEqual "mvc"
        form["tags[2]"] |> string |> shouldEqual "api"
    }

[<Fact>]
let ``BindForm binds repeated and indexed string collections`` () =
    task {
        let repeatedContext = createFormContext "Tags=dotnet&Tags=mvc&Tags=api"
        let indexedContext = createFormContext "Tags[0]=dotnet&Tags[1]=mvc&Tags[2]=api"

        let! repeated = repeatedContext.BindForm<StringCollectionModel>()
        let! indexed = indexedContext.BindForm<StringCollectionModel>()

        repeated.Tags |> shouldEqual [ "dotnet"; "mvc"; "api" ]
        indexed.Tags |> shouldEqual repeated.Tags
    }

[<Fact>]
let ``BindForm binds indexed scalar collections`` () =
    task {
        let ctx =
            createFormContext "Counts[0]=1&Counts[1]=2&Ratios[0]=1.5&Ratios[1]=2.25&Flags[0]=true&Flags[1]=false"

        let! result = ctx.BindForm<ScalarCollectionModel>()

        result.Counts |> shouldEqual [| 1; 2 |]
        result.Ratios |> shouldEqual [ 1.5; 2.25 ]
        result.Flags |> shouldEqual(seq [ true; false ])
    }

[<Fact>]
let ``BindForm binds first value for duplicated collection index`` () =
    task {
        // The checkbox + hidden-input fallback idiom: both inputs share the same indexed name.
        let ctx = createFormContext "Flags[0]=true&Flags[0]=false&Flags[1]=false"

        let! result = ctx.BindForm<ScalarCollectionModel>()

        result.Flags |> shouldEqual(seq [ true; false ])
    }

[<Fact>]
let ``BindQuery binds indexed string collections`` () =
    let ctx = DefaultHttpContext()
    let services = ServiceCollection()
    services.AddSingleton<IModelBinder>(ModelBinder()) |> ignore
    ctx.RequestServices <- services.BuildServiceProvider()
    ctx.Request.Query <- QueryCollection(QueryHelpers.ParseQuery "?Tags[0]=dotnet&Tags[1]=mvc&Tags[2]=api")

    let result = ctx.BindQuery<StringCollectionModel>()

    result.Tags |> shouldEqual [ "dotnet"; "mvc"; "api" ]

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
