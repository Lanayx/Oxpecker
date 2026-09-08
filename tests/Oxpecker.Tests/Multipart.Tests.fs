module Oxpecker.Tests.Multipart

open System
open System.Collections.Generic
open System.IO
open System.Net
open System.Net.Http
open System.Text
open System.Text.Json
open System.Text.RegularExpressions
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.TestHost
open Microsoft.AspNetCore.WebUtilities
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Oxpecker
open Oxpecker.ViewEngine
open Xunit
open FsUnit.Light

#nowarn "3391"

// ---------------------------------
// Helpers
// ---------------------------------

let private getBoundary (contentType: string) =
    let m = Regex.Match(contentType, "boundary=([^;]+)")
    m.Success |> shouldEqual true
    m.Groups[1].Value

let private createContext () =
    let ctx = DefaultHttpContext()
    ctx.Response.Body <- new MemoryStream()
    ctx

let private responseContentType (ctx: HttpContext) =
    ctx.Response.Headers.ContentType.ToString()

let private readBody (ctx: HttpContext) =
    ctx.Response.Body.Seek(0, SeekOrigin.Begin) |> ignore
    use reader = new StreamReader(ctx.Response.Body, Encoding.UTF8)
    reader.ReadToEnd()

let private readBytes (ctx: HttpContext) =
    ctx.Response.Body.Seek(0, SeekOrigin.Begin) |> ignore
    use memoryStream = new MemoryStream()
    ctx.Response.Body.CopyTo memoryStream
    memoryStream.ToArray()

/// Parses a multipart body with ASP.NET Core's MultipartReader and returns (Content-Type, HX-Target, body) per section.
let private readSections (boundary: string) (stream: Stream) =
    task {
        let reader = MultipartReader(boundary, stream)
        let sections = ResizeArray<string * string option * string>()
        let mutable finished = false
        while not finished do
            match! reader.ReadNextSectionAsync() with
            | null -> finished <- true
            | section ->
                use sectionReader = new StreamReader(section.Body, Encoding.UTF8)
                let! body = sectionReader.ReadToEndAsync()
                let target =
                    match section.Headers with
                    | null -> None
                    | headers when headers.ContainsKey "HX-Target" -> Some(headers["HX-Target"].ToString())
                    | _ -> None
                sections.Add((string section.ContentType, target, body))
        return List.ofSeq sections
    }

/// An asynchronous producer of parts: every part is delivered after a hop to the thread pool, like a real
/// stream would, and `onMoveNext` is called each time the next part is requested.
type private AsyncParts(parts: MultipartPart list, onMoveNext: unit -> unit) =
    new(parts: MultipartPart list) = AsyncParts(parts, ignore)
    interface IAsyncEnumerable<MultipartPart> with
        member this.GetAsyncEnumerator(_) =
            let mutable remaining = parts
            let mutable current = Unchecked.defaultof<MultipartPart>
            { new IAsyncEnumerator<MultipartPart> with
                member this.Current = current
                member this.MoveNextAsync() =
                    ValueTask<bool>(
                        task {
                            do! Task.Yield()
                            onMoveNext()
                            match remaining with
                            | [] -> return false
                            | part :: rest ->
                                current <- part
                                remaining <- rest
                                return true
                        }
                    )
                member this.DisposeAsync() = ValueTask()
            }

let private statusView = div(id = "status") { "Online" }

let private expectedEnvelope (boundary: string) =
    $"--{boundary}\r\n"
    + "Content-Type: text/html; charset=utf-8\r\n"
    + "HX-Target: #status\r\n"
    + "\r\n"
    + """<div id="status">Online</div>"""
    + $"\r\n--{boundary}\r\n"
    + "Content-Type: text/plain; charset=utf-8\r\n"
    + "\r\n"
    + "done"
    + $"\r\n--{boundary}--\r\n"

let private twoParts () = [
    MultipartPart.Html(statusView, headers = [ "HX-Target", "#status" ])
    MultipartPart.Text "done"
]

// ---------------------------------
// WriteMultipart (buffered)
// ---------------------------------

[<Fact>]
let ``WriteMultipart writes multipart/mixed envelope and sets Content-Length`` () =
    task {
        let ctx = createContext()

        do! ctx.WriteMultipart(twoParts())

        let contentType = responseContentType ctx
        let boundary = getBoundary contentType
        contentType |> shouldEqual $"multipart/mixed; boundary={boundary}"
        boundary.StartsWith "multipart-" |> shouldEqual true
        boundary.Length |> shouldEqual 42
        let expected = expectedEnvelope boundary
        readBody ctx |> shouldEqual expected
        ctx.Response.Headers.ContentLength
        |> shouldEqual(int64(Encoding.UTF8.GetByteCount expected))
    }

[<Fact>]
let ``WriteMultipart with Parallel subtype writes multipart/parallel`` () =
    task {
        let ctx = createContext()

        do! ctx.WriteMultipart([ MultipartPart.Text "done" ], MultipartSubtype.Parallel)

        let contentType = responseContentType ctx
        let boundary = getBoundary contentType
        contentType |> shouldEqual $"multipart/parallel; boundary={boundary}"
        readBody ctx
        |> shouldEqual $"--{boundary}\r\nContent-Type: text/plain; charset=utf-8\r\n\r\ndone\r\n--{boundary}--\r\n"
    }

[<Fact>]
let ``WriteMultipart with HTTP HEAD sets headers but writes no body`` () =
    task {
        let ctx = createContext()
        ctx.Request.Method <- "HEAD"

        do! ctx.WriteMultipart(twoParts())

        let boundary = getBoundary(responseContentType ctx)
        ctx.Response.Headers.ContentLength
        |> shouldEqual(int64(Encoding.UTF8.GetByteCount(expectedEnvelope boundary)))
        readBody ctx |> shouldEqual ""
    }

[<Fact>]
let ``WriteMultipart generates a fresh boundary per response`` () =
    task {
        let ctx1 = createContext()
        let ctx2 = createContext()

        do! ctx1.WriteMultipart(twoParts())
        do! ctx2.WriteMultipart(twoParts())

        getBoundary(responseContentType ctx1)
        |> shouldNotEqual(getBoundary(responseContentType ctx2))
    }

[<Fact>]
let ``WriteMultipart serializes Json parts with web defaults or custom options`` () =
    task {
        let ctx = createContext()
        let parts = [
            MultipartPart.Json({| Hello = "World" |}, headers = [ "Content-ID", "result" ])
            MultipartPart.Json({| Hello = "World" |}, options = JsonSerializerOptions())
        ]

        do! ctx.WriteMultipart parts

        let boundary = getBoundary(responseContentType ctx)
        readBody ctx
        |> shouldEqual(
            $"--{boundary}\r\n"
            + "Content-Type: application/json; charset=utf-8\r\n"
            + "Content-ID: result\r\n"
            + "\r\n"
            + """{"hello":"World"}"""
            + $"\r\n--{boundary}\r\n"
            + "Content-Type: application/json; charset=utf-8\r\n"
            + "\r\n"
            + """{"Hello":"World"}"""
            + $"\r\n--{boundary}--\r\n"
        )
    }

[<Fact>]
let ``WriteMultipart writes Bytes parts without re-encoding`` () =
    task {
        let ctx = createContext()
        let data = [| 0uy; 255uy; 13uy; 10uy; 128uy |]

        do! ctx.WriteMultipart [ MultipartPart.Bytes("application/octet-stream", data) ]

        let boundary = getBoundary(responseContentType ctx)
        let expected =
            Array.concat [
                Encoding.ASCII.GetBytes $"--{boundary}\r\nContent-Type: application/octet-stream\r\n\r\n"
                data
                Encoding.ASCII.GetBytes $"\r\n--{boundary}--\r\n"
            ]
        readBytes ctx |> shouldEqual expected
        ctx.Response.Headers.ContentLength |> shouldEqual(int64 expected.Length)
    }

[<Fact>]
let ``WriteMultipart respects ContentType and Headers set on the part`` () =
    task {
        let ctx = createContext()
        let part = MultipartPart.Text "id,name"
        part.ContentType <- "text/csv; charset=utf-8"
        part.Headers["HX-Trigger"] <- "export-ready"
        part.Headers["HX-Part-ID"] <- "part-1"

        do! ctx.WriteMultipart [ part ]

        let boundary = getBoundary(responseContentType ctx)
        readBody ctx
        |> shouldEqual(
            $"--{boundary}\r\n"
            + "Content-Type: text/csv; charset=utf-8\r\n"
            + "HX-Trigger: export-ready\r\n"
            + "HX-Part-ID: part-1\r\n"
            + "\r\n"
            + "id,name"
            + $"\r\n--{boundary}--\r\n"
        )
    }

[<Fact>]
let ``WriteMultipart rejects header values containing line breaks`` () =
    task {
        let ctx = createContext()
        let parts = [
            MultipartPart.Text("done", headers = [ "HX-Trigger", "evil\r\nHX-Redirect: /" ])
        ]

        let! ex = Assert.ThrowsAsync<ArgumentException>(fun () -> ctx.WriteMultipart parts)

        ex.Message.Contains "HX-Trigger" |> shouldEqual true
    }

[<Fact>]
let ``WriteMultipart rejects header values containing control characters`` () =
    task {
        let ctx = createContext()
        let parts = [ MultipartPart.Text("done", headers = [ "HX-Trigger", "bad\000value" ]) ]

        let! ex = Assert.ThrowsAsync<ArgumentException>(fun () -> ctx.WriteMultipart parts)

        ex.Message.Contains "HX-Trigger" |> shouldEqual true
    }

[<Fact>]
let ``WriteMultipart accepts tabs and non-ASCII text in header values`` () =
    task {
        let ctx = createContext()
        let parts = [ MultipartPart.Text("done", headers = [ "HX-Trigger", "tab\tok Привет" ]) ]

        do! ctx.WriteMultipart parts

        (readBody ctx).Contains "HX-Trigger: tab\tok Привет\r\n" |> shouldEqual true
    }

[<Fact>]
let ``WriteMultipart rejects header names that are not HTTP tokens`` () =
    task {
        for name in [ ""; "HX-Trigger: x"; "Bad Header"; "Tab\tHeader"; "Quoted\"Name"; "Ünïcode" ] do
            let ctx = createContext()
            let parts = [ MultipartPart.Text("done", headers = [ name, "value" ]) ]
            let! ex = Assert.ThrowsAsync<ArgumentException>(fun () -> ctx.WriteMultipart parts)
            ex.Message.Contains name |> shouldEqual true
    }

[<Fact>]
let ``WriteMultipart accepts header names made of HTTP token characters`` () =
    task {
        let ctx = createContext()
        let parts = [
            MultipartPart.Text("done", headers = [ "X-Custom_Header.1!#$%&'*+^`|~", "value" ])
        ]

        do! ctx.WriteMultipart parts

        (readBody ctx).Contains "X-Custom_Header.1!#$%&'*+^`|~: value\r\n"
        |> shouldEqual true
    }

[<Fact>]
let ``WriteMultipart rejects header lines longer than 998 bytes`` () =
    task {
        // "HX-Trigger: " takes 12 bytes, so the line may hold a 986-byte value at most
        for value in [ String('x', 987); String('П', 494) ] do
            let ctx = createContext()
            let parts = [ MultipartPart.Text("done", headers = [ "HX-Trigger", value ]) ]
            let! ex = Assert.ThrowsAsync<ArgumentException>(fun () -> ctx.WriteMultipart parts)
            ex.Message.Contains "HX-Trigger" |> shouldEqual true
            ex.Message.Contains "998" |> shouldEqual true
    }

[<Fact>]
let ``WriteMultipart accepts header lines of exactly 998 bytes`` () =
    task {
        let ctx = createContext()
        let value = String('x', 986)
        let parts = [ MultipartPart.Text("done", headers = [ "HX-Trigger", value ]) ]

        do! ctx.WriteMultipart parts

        (readBody ctx).Contains $"HX-Trigger: {value}\r\n" |> shouldEqual true
    }

[<Fact>]
let ``WriteMultipart rejects an empty, line-broken or overlong ContentType`` () =
    task {
        // "Content-Type: " takes 14 bytes, so the line may hold a 984-byte value at most
        for contentType in [ ""; "text/plain\r\nHX-Redirect: /"; "text/plain; " + String('x', 973) ] do
            let ctx = createContext()
            let part = MultipartPart.Text "done"
            part.ContentType <- contentType
            let! ex = Assert.ThrowsAsync<ArgumentException>(fun () -> ctx.WriteMultipart [ part ])
            ex.Message.Contains "Content-Type" |> shouldEqual true
    }

[<Fact>]
let ``WriteMultipart stops rendering when the request is aborted`` () =
    task {
        let ctx = createContext()
        use cts = new CancellationTokenSource()
        ctx.RequestAborted <- cts.Token
        cts.Cancel()

        let! _ = Assert.ThrowsAnyAsync<OperationCanceledException>(fun () -> ctx.WriteMultipart(twoParts()))

        responseContentType ctx |> shouldEqual ""
        readBody ctx |> shouldEqual ""
    }

[<Fact>]
let ``WriteMultipart rejects an empty sequence of parts and writes nothing`` () =
    task {
        let ctx = createContext()

        let! ex = Assert.ThrowsAsync<ArgumentException>(fun () -> ctx.WriteMultipart [])

        ex.ParamName |> shouldEqual "parts"
        responseContentType ctx |> shouldEqual ""
        ctx.Response.Headers.ContentLength |> shouldEqual(Nullable())
        readBody ctx |> shouldEqual ""
    }

[<Fact>]
let ``WriteMultipart output is parsed back by MultipartReader`` () =
    task {
        let ctx = createContext()
        let parts = [
            MultipartPart.Html(statusView, headers = [ "HX-Target", "#status" ])
            MultipartPart.Json({| Id = 42 |})
            MultipartPart.Text "done"
        ]

        do! ctx.WriteMultipart parts

        let boundary = getBoundary(responseContentType ctx)
        ctx.Response.Body.Seek(0, SeekOrigin.Begin) |> ignore
        let! sections = readSections boundary ctx.Response.Body

        sections
        |> shouldEqual [
            "text/html; charset=utf-8", Some "#status", """<div id="status">Online</div>"""
            "application/json; charset=utf-8", None, """{"id":42}"""
            "text/plain; charset=utf-8", None, "done"
        ]
    }

// ---------------------------------
// WriteMultipartChunked (streamed)
// ---------------------------------

[<Fact>]
let ``WriteMultipartChunked writes the same envelope without Content-Length`` () =
    task {
        let ctx = createContext()

        do! ctx.WriteMultipartChunked(AsyncParts(twoParts()))

        let contentType = responseContentType ctx
        let boundary = getBoundary contentType
        contentType |> shouldEqual $"multipart/mixed; boundary={boundary}"
        ctx.Response.Headers.ContentLength |> shouldEqual(Nullable())
        readBody ctx |> shouldEqual(expectedEnvelope boundary)
    }

[<Fact>]
let ``WriteMultipartChunked with Parallel subtype writes multipart/parallel`` () =
    task {
        let ctx = createContext()

        do! ctx.WriteMultipartChunked(AsyncParts [ MultipartPart.Text "done" ], MultipartSubtype.Parallel)

        let boundary = getBoundary(responseContentType ctx)
        responseContentType ctx
        |> shouldEqual $"multipart/parallel; boundary={boundary}"
    }

[<Fact>]
let ``WriteMultipartChunked with HTTP HEAD sets Content-Type but writes no body`` () =
    task {
        let ctx = createContext()
        ctx.Request.Method <- "HEAD"
        let enumerated = ref false
        let parts =
            AsyncParts([ MultipartPart.Text "done" ], (fun () -> enumerated.Value <- true))

        do! ctx.WriteMultipartChunked parts

        (responseContentType ctx).StartsWith "multipart/mixed; boundary="
        |> shouldEqual true
        ctx.Response.Headers.ContentLength |> shouldEqual(Nullable())
        readBody ctx |> shouldEqual ""
        enumerated.Value |> shouldEqual false
    }

[<Fact>]
let ``WriteMultipartChunked rejects an empty stream of parts and writes nothing`` () =
    task {
        let ctx = createContext()

        let! ex = Assert.ThrowsAsync<ArgumentException>(fun () -> ctx.WriteMultipartChunked(AsyncParts []))

        ex.ParamName |> shouldEqual "parts"
        responseContentType ctx |> shouldEqual ""
        readBody ctx |> shouldEqual ""
    }

[<Fact>]
let ``WriteMultipartChunked does not write a part produced after the request was aborted`` () =
    task {
        let ctx = createContext()
        use cts = new CancellationTokenSource()
        ctx.RequestAborted <- cts.Token
        let data = Array.create 4096 1uy
        // the client disconnects while the producer is generating the part
        let parts =
            AsyncParts([ MultipartPart.Bytes("application/octet-stream", data) ], (fun () -> cts.Cancel()))

        let! _ = Assert.ThrowsAnyAsync<OperationCanceledException>(fun () -> ctx.WriteMultipartChunked parts)

        (readBytes ctx).Length < data.Length |> shouldEqual true
    }

[<Fact>]
let ``WriteMultipartChunked flushes each part including its delimiter before requesting the next one`` () =
    task {
        let ctx = createContext()
        let body = ctx.Response.Body :?> MemoryStream
        let snapshots = ResizeArray<string>()
        let parts =
            AsyncParts(
                [ MultipartPart.Text "first"; MultipartPart.Text "second" ],
                (fun () -> snapshots.Add(Encoding.UTF8.GetString(body.ToArray())))
            )

        do! ctx.WriteMultipartChunked parts

        let boundary = getBoundary(responseContentType ctx)
        let firstPart =
            $"--{boundary}\r\nContent-Type: text/plain; charset=utf-8\r\n\r\nfirst\r\n--{boundary}"
        let secondPart =
            $"\r\nContent-Type: text/plain; charset=utf-8\r\n\r\nsecond\r\n--{boundary}"
        List.ofSeq snapshots |> shouldEqual [ ""; firstPart; firstPart + secondPart ]
        readBody ctx |> shouldEqual(firstPart + secondPart + "--\r\n")
    }

// ---------------------------------
// End-to-end through TestServer
// ---------------------------------

module private WebApp =
    let streamedParts =
        AsyncParts [
            MultipartPart.Html(statusView, headers = [ "HX-Target", "#status" ])
            MultipartPart.Json({| Id = 42 |}, headers = [ "Content-ID", "result" ])
            MultipartPart.Text "done"
        ]

    let endpoints = [
        route "/buffered" (multipart(twoParts()))
        route "/chunked" (multipartChunked streamedParts)
    ]

    let webApp () =
        task {
            let host =
                HostBuilder()
                    .ConfigureWebHost(fun webHostBuilder ->
                        webHostBuilder
                            .UseTestServer()
                            .Configure(fun app -> app.UseRouting().UseOxpecker(endpoints) |> ignore)
                            .ConfigureServices(fun services -> services.AddRouting().AddOxpecker() |> ignore)
                        |> ignore)
                    .Build()
            do! host.StartAsync()
            return host
        }

[<Fact>]
let ``HTTP GET buffered multipart endpoint`` () =
    task {
        use! server = WebApp.webApp()
        let client = server.GetTestClient()

        let! response = client.GetAsync("/buffered")

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        let contentType = string response.Content.Headers.ContentType
        let boundary = getBoundary contentType
        contentType |> shouldEqual $"multipart/mixed; boundary={boundary}"
        let! content = response.Content.ReadAsStringAsync()
        let expected = expectedEnvelope boundary
        content |> shouldEqual expected
        response.Content.Headers.ContentLength
        |> shouldEqual(int64(Encoding.UTF8.GetByteCount expected))
    }

[<Fact>]
let ``HTTP GET chunked multipart endpoint`` () =
    task {
        use! server = WebApp.webApp()
        let client = server.GetTestClient()

        let! response = client.GetAsync("/chunked", HttpCompletionOption.ResponseHeadersRead)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual(Nullable())
        let contentType = string response.Content.Headers.ContentType
        let boundary = getBoundary contentType
        contentType |> shouldEqual $"multipart/mixed; boundary={boundary}"
        let! stream = response.Content.ReadAsStreamAsync()
        let! sections = readSections boundary stream
        sections
        |> shouldEqual [
            "text/html; charset=utf-8", Some "#status", """<div id="status">Online</div>"""
            "application/json; charset=utf-8", None, """{"id":42}"""
            "text/plain; charset=utf-8", None, "done"
        ]
    }
