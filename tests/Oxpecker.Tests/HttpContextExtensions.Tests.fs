module Oxpecker.Tests.HttpContextExtensions

open System
open System.Collections.Generic
open System.IO
open System.Text
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
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

// ---------------------------------------------------------------------------
// TryGet* extensions: StringValues conversion behaviour.
//
// Inputs are produced the way a real request produces them - a parsed query
// string, a parsed urlencoded form body and the request header dictionary -
// so the cases only cover values these getters can actually receive.
// ---------------------------------------------------------------------------

let private queryContext (wire: string) =
    let ctx = DefaultHttpContext()
    ctx.Request.QueryString <- QueryString("?" + wire)
    ctx

let private headerContext (values: string array) =
    let ctx = DefaultHttpContext()
    for value in values do
        ctx.Request.Headers.Append("X-Test", value)
    ctx

/// The implementations these extensions used to have, kept so the current ones
/// can be checked against the behaviour they replaced.
let private legacySingle (value: StringValues) = value |> string
let private legacyValues (value: StringValues) = value |> Seq.map string

let private isNotNull (value: string) = obj.ReferenceEquals(value, null) |> not

/// Key/value pairs exactly as they arrive on the wire. The urlencoded syntax is
/// the same for a query string and a form body, so both are driven from these.
let wireCases: seq<array<obj>>  =
    seq {
        [| "q=a"; "a"; [| "a" |] |]
        [| "q=a&q=bb"; "a,bb"; [| "a"; "bb" |] |]
        [| "q=a&q=bb&q=ccc"; "a,bb,ccc"; [| "a"; "bb"; "ccc" |] |]
        [| "q="; ""; [| "" |] |]
        // joining skips blank entries, so the single-value getter drops the empty one
        [| "q=&q=a"; "a"; [| ""; "a" |] |]
        [| "q=a%20b"; "a b"; [| "a b" |] |]
        [| "q=%C3%A9"; "é"; [| "é" |] |]
    }

let headerCases: seq<array<obj>> =
    seq {
        [| [| "a" |]; "a"; [| "a" |] |]
        [| [| "a"; "bb" |]; "a,bb"; [| "a"; "bb" |] |]
        [|
            [| "gzip"; "deflate"; "br" |]
            "gzip,deflate,br"
            [| "gzip"; "deflate"; "br" |]
        |]
    }

[<Theory; MemberData(nameof wireCases)>]
let ``TryGetQueryValue and TryGetQueryValues convert a parsed query string``
    (wire: string, singleExpected: string, valuesExpected: string array)
    =
    let ctx = queryContext wire

    ctx.TryGetQueryValue "q" |> shouldEqual(Some singleExpected)
    ctx.TryGetQueryValues "q"
    |> Option.map Array.ofSeq
    |> shouldEqual(Some valuesExpected)

[<Theory; MemberData(nameof wireCases)>]
let ``TryGetFormValue and TryGetFormValues convert a parsed form body``
    (wire: string, singleExpected: string, valuesExpected: string array)
    =
    task {
        let ctx = createFormContext wire
        let! _ = ctx.Request.ReadFormAsync()

        ctx.TryGetFormValue "q" |> shouldEqual(Some singleExpected)
        ctx.TryGetFormValues "q"
        |> Option.map Array.ofSeq
        |> shouldEqual(Some valuesExpected)
    }

[<Theory; MemberData(nameof headerCases)>]
let ``TryGetHeaderValue and TryGetHeaderValues convert request headers``
    (values: string array, singleExpected: string, valuesExpected: string array)
    =
    let ctx = headerContext values

    ctx.TryGetHeaderValue "X-Test" |> shouldEqual(Some singleExpected)
    ctx.TryGetHeaderValues "X-Test"
    |> Option.map Array.ofSeq
    |> shouldEqual(Some valuesExpected)

[<Fact>]
let ``TryGet value extensions return None for a missing key`` () =
    task {
        let ctx = queryContext "other=1"
        ctx.TryGetQueryValue "q" |> shouldEqual None
        ctx.TryGetQueryValues "q" |> shouldEqual None
        ctx.TryGetHeaderValue "X-Test" |> shouldEqual None
        ctx.TryGetHeaderValues "X-Test" |> shouldEqual None

        let formCtx = createFormContext "other=1"
        let! _ = formCtx.Request.ReadFormAsync()
        formCtx.TryGetFormValue "q" |> shouldEqual None
        formCtx.TryGetFormValues "q" |> shouldEqual None
    }

[<Fact>]
let ``Appending an empty header value leaves the header absent`` () =
    // HeaderDictionary drops empty StringValues, so there is no "present but empty" header
    let ctx = headerContext [| "" |]

    ctx.Request.Headers.ContainsKey "X-Test" |> shouldEqual false
    ctx.TryGetHeaderValue "X-Test" |> shouldEqual None
    ctx.TryGetHeaderValues "X-Test" |> shouldEqual None

[<Theory; MemberData(nameof wireCases)>]
let ``TryGetQuery and TryGetForm match the previous implementation`` (wire: string, _: string, _: string array) =
    task {
        let ctx = queryContext wire
        let queryValue = ctx.Request.Query["q"]
        ctx.TryGetQueryValue "q" |> shouldEqual(Some(legacySingle queryValue))
        ctx.TryGetQueryValues "q"
        |> Option.map List.ofSeq
        |> shouldEqual(Some(legacyValues queryValue |> List.ofSeq))

        let formCtx = createFormContext wire
        let! _ = formCtx.Request.ReadFormAsync()
        let formValue = formCtx.Request.Form["q"]
        formCtx.TryGetFormValue "q" |> shouldEqual(Some(legacySingle formValue))
        formCtx.TryGetFormValues "q"
        |> Option.map List.ofSeq
        |> shouldEqual(Some(legacyValues formValue |> List.ofSeq))
    }

[<Theory; MemberData(nameof headerCases)>]
let ``TryGetHeader matches the previous implementation`` (values: string array, _: string, _: string array) =
    let ctx = headerContext values
    let headerValue = ctx.Request.Headers["X-Test"]

    ctx.TryGetHeaderValue "X-Test" |> shouldEqual(Some(legacySingle headerValue))
    ctx.TryGetHeaderValues "X-Test"
    |> Option.map List.ofSeq
    |> shouldEqual(Some(legacyValues headerValue |> List.ofSeq))

[<Theory; MemberData(nameof wireCases)>]
let ``Values from real collections are never null`` (wire: string, _: string, _: string array) =
    // TryGet*Values hands out the StringValues itself, which is only sound while
    // real query/form/header collections cannot contain null entries.
    task {
        let ctx = queryContext wire
        (ctx.TryGetQueryValues "q").Value |> Seq.forall isNotNull |> shouldEqual true

        let formCtx = createFormContext wire
        let! _ = formCtx.Request.ReadFormAsync()
        (formCtx.TryGetFormValues "q").Value |> Seq.forall isNotNull |> shouldEqual true

        let headerCtx = headerContext [| "a"; "bb" |]
        (headerCtx.TryGetHeaderValues "X-Test").Value
        |> Seq.forall isNotNull
        |> shouldEqual true
    }

[<Fact>]
let ``TryGetHeaderValues result can be enumerated more than once`` () =
    let ctx = headerContext [| "a"; "bb" |]

    let result = (ctx.TryGetHeaderValues "X-Test").Value

    result |> List.ofSeq |> shouldEqual [ "a"; "bb" ]
    result |> List.ofSeq |> shouldEqual [ "a"; "bb" ]

[<Fact>]
let ``TryGetHeaderValues result cannot be mutated into the header collection`` () =
    // The result is the StringValues itself rather than a copy, so it must not
    // expose a writable view onto the request headers.
    let ctx = headerContext [| "a"; "bb" |]

    let result = (ctx.TryGetHeaderValues "X-Test").Value

    result :? (string array) |> shouldEqual false
    match result with
    | :? IList<string> as list -> Assert.Throws<NotSupportedException>(fun () -> list[0] <- "mutated") |> ignore
    | _ -> ()

    ctx.Request.Headers["X-Test"] |> Array.ofSeq |> shouldEqual [| "a"; "bb" |]
    ctx.TryGetHeaderValues "X-Test"
    |> Option.map List.ofSeq
    |> shouldEqual(Some [ "a"; "bb" ])
