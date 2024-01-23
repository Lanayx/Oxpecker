module Oxpecker.Tests.Streaming

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Xunit
open Oxpecker
open FsUnitTyped

#nowarn "3391"

// ---------------------------------
// Text file used for feature testing
// ---------------------------------

// ### TEXT REPRESENTATION
// ---------------------------------

// 0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ


// ### BYTE REPRESENTATION
// ---------------------------------

// 48,49,50,51,52,53,54,55,56,57,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90


// ### TABULAR BYTE REPRESENTATION
// ---------------------------------

// 0  ,1  ,2  ,3  ,4  ,5  ,6  ,7  ,8  ,9
// ----------------------------------------
// 48 ,49 ,50 ,51 ,52 ,53 ,54 ,55 ,56 ,57
// 97 ,98 ,99 ,100,101,102,103,104,105,106
// 107,108,109,110,111,112,113,114,115,116
// 117,118,119,120,121,122,65 ,66 ,67 ,68
// 69 ,70 ,71 ,72 ,73 ,74 ,75 ,76 ,77 ,78
// 79 ,80 ,81 ,82 ,83 ,84 ,85 ,86 ,87 ,88
// 89 ,90

// ---------------------------------
// Streaming App
// ---------------------------------

module Urls =
    let rangeProcessingEnabled = "/range-processing-enabled"
    let rangeProcessingDisabled = "/range-processing-disabled"

module WebApp =
    let streamHandler (enableRangeProcessing: bool) : EndpointHandler =
        streamFile enableRangeProcessing "TestFiles/streaming.txt" None None

    let endpoints = [
        route Urls.rangeProcessingEnabled (streamHandler true)
        route Urls.rangeProcessingDisabled (streamHandler false)
    ]

    let webApp () =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(fun app -> app.UseRouting().UseOxpecker(endpoints) |> ignore)
                .ConfigureServices(fun services -> services.AddRouting() |> ignore)
        new TestServer(builder)

let server = WebApp.webApp()

// ---------------------------------
// Tests
// ---------------------------------

[<Fact>]
let ``HTTP GET entire file with range processing disabled`` () =
    task {
        let client = server.CreateClient()

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        response.Headers.AcceptRanges |> shouldBeEmpty
        response.Content.Headers.ContentRange |> shouldEqual null
        let! content = response.Content.ReadAsStringAsync()
        content
        |> shouldEqual "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
    }

[<Fact>]
let ``HTTP GET entire file with range processing enabled`` () =
    task {
        let client = server.CreateClient()

        let! response = client.GetAsync(Urls.rangeProcessingEnabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        response.Headers.AcceptRanges |> shouldContain "bytes"
        response.Content.Headers.ContentRange |> shouldEqual null
        let! content = response.Content.ReadAsStringAsync()
        content
        |> shouldEqual "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
    }

[<Fact>]
let ``HTTP HEAD entire file with range processing disabled`` () =
    task {
        let client = server.CreateClient()

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, Urls.rangeProcessingDisabled))

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        response.Headers.AcceptRanges |> shouldBeEmpty
        response.Content.Headers.ContentRange |> shouldEqual null
        let! content = response.Content.ReadAsStringAsync()
        content |> shouldEqual ""
    }

[<Fact>]
let ``HTTP HEAD entire file with range processing enabled`` () =
    task {
        let client = server.CreateClient()

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, Urls.rangeProcessingEnabled))

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        response.Headers.AcceptRanges |> shouldContain "bytes"
        response.Content.Headers.ContentRange |> shouldEqual null
        let! content = response.Content.ReadAsStringAsync()
        content |> shouldEqual ""
    }

[<Fact>]
let ``HTTP GET part of file with range processing enabled`` () =
    task {
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("Range", "bytes=0-9")

        let! response = client.GetAsync(Urls.rangeProcessingEnabled)

        response.StatusCode |> shouldEqual HttpStatusCode.PartialContent
        response.Content.Headers.ContentLength |> shouldEqual 10L
        response.Headers.AcceptRanges |> shouldContain "bytes"
        response.Content.Headers.ContentRange
        |> shouldEqual(ContentRangeHeaderValue(0, 9, 62))
        let! content = response.Content.ReadAsByteArrayAsync()
        content
        |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy |]
    }

[<Fact>]
let ``HTTP GET middle part of file with range processing enabled`` () =
    task {
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("Range", "bytes=12-26")

        let! response = client.GetAsync(Urls.rangeProcessingEnabled)

        response.StatusCode |> shouldEqual HttpStatusCode.PartialContent
        response.Content.Headers.ContentLength |> shouldEqual 15L
        response.Headers.AcceptRanges |> shouldContain "bytes"
        response.Content.Headers.ContentRange
        |> shouldEqual(ContentRangeHeaderValue(12, 26, 62))
        let! content = response.Content.ReadAsByteArrayAsync()
        content
        |> shouldEqual [| 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy |]
    }

[<Fact>]
let ``HTTP GET with range without end and range processing enabled`` () =
    task {
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("Range", "bytes=20-")

        let! response = client.GetAsync(Urls.rangeProcessingEnabled)

        response.StatusCode |> shouldEqual HttpStatusCode.PartialContent
        response.Content.Headers.ContentLength |> shouldEqual 42L
        response.Headers.AcceptRanges |> shouldContain "bytes"
        response.Content.Headers.ContentRange
        |> shouldEqual(ContentRangeHeaderValue(20, 61, 62))
        let! content = response.Content.ReadAsByteArrayAsync()
        content
        |> shouldEqual [| 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP GET middle part of file with range processing disabled`` () =
    task {
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("Range", "bytes=12-26")

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        response.Headers.AcceptRanges |> shouldBeEmpty
        response.Content.Headers.ContentRange |> shouldEqual null
        let! content = response.Content.ReadAsByteArrayAsync()
        content
        |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP HEAD middle part of file with range processing disabled`` () =
    task {
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("Range", "bytes=12-26")

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, Urls.rangeProcessingDisabled))

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        response.Headers.AcceptRanges |> shouldBeEmpty
        response.Content.Headers.ContentRange |> shouldEqual null
        let! content = response.Content.ReadAsByteArrayAsync()
        content |> shouldBeEmpty
    }

[<Fact>]
let ``HTTP GET with invalid range and with range processing enabled`` () =
    task {
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("Range", "bytes=63-70")

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Get, Urls.rangeProcessingEnabled))

        response.StatusCode |> shouldEqual HttpStatusCode.RequestedRangeNotSatisfiable
        response.Content.Headers.ContentLength |> shouldEqual 0L
        response.Headers.AcceptRanges |> shouldContain "bytes"
        response.Content.Headers.ContentRange
        |> shouldEqual(ContentRangeHeaderValue(62))
        let! content = response.Content.ReadAsByteArrayAsync()
        content |> shouldBeEmpty
    }

[<Fact>]
let ``HTTP HEAD with invalid range and with range processing enabled`` () =
    task {
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("Range", "bytes=63-70")

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, Urls.rangeProcessingEnabled))

        response.StatusCode |> shouldEqual HttpStatusCode.RequestedRangeNotSatisfiable
        response.Content.Headers.ContentLength |> shouldEqual(Nullable())
        response.Headers.AcceptRanges |> shouldContain "bytes"
        response.Content.Headers.ContentRange
        |> shouldEqual(ContentRangeHeaderValue(62))
        let! content = response.Content.ReadAsByteArrayAsync()
        content |> shouldBeEmpty
    }

[<Fact>]
let ``HTTP GET with invalid range and with range processing disabled`` () =
    task {
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("Range", "bytes=63-70")

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        response.Headers.AcceptRanges |> shouldBeEmpty
        response.Content.Headers.ContentRange |> shouldEqual null
        let! content = response.Content.ReadAsByteArrayAsync()
        content
        |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP GET with multiple ranges and with range processing enabled`` () =
    task {
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("Range", "bytes=5-10, 20-25, 40-")

        let! response = client.GetAsync(Urls.rangeProcessingEnabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        response.Headers.AcceptRanges |> shouldContain "bytes"
        response.Content.Headers.ContentRange |> shouldEqual null
        let! content = response.Content.ReadAsByteArrayAsync()
        content
        |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }
