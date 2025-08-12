module Oxpecker.Tests.Preconditional

open System
open System.Net
open System.Net.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Xunit
open Oxpecker
open FsUnit.Light


type DateTimeOffset with
    member this.ToHtmlString() = this.ToString("r")

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
    let streamHandler (enableRangeProcessing: bool) args : EndpointHandler =
        args ||> streamFile enableRangeProcessing "TestFiles/streaming2.txt"

    let endpoints args = [
        route Urls.rangeProcessingEnabled (streamHandler true args)
        route Urls.rangeProcessingDisabled (streamHandler false args)
    ]

    let webApp args =
        let builder =
            WebHostBuilder()
                .UseKestrel()
                .Configure(fun app -> app.UseRouting().UseOxpecker(endpoints args) |> ignore)
                .ConfigureServices(fun services -> services.AddRouting() |> ignore)

        new TestServer(builder)

// ---------------------------------
// Tests
// ---------------------------------

[<Fact>]
let ``HTTP GET with If-Match and no ETag`` () =
    task {
        let server = WebApp.webApp (None, None)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Match", "\"111\", \"222\", \"333\"")

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.PreconditionFailed
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [||]
    }

[<Fact>]
let ``HTTP GET with If-Match and not matching ETag`` () =
    task {
        let server = WebApp.webApp (createETag "000" |> Some, None)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Match", "\"111\", \"222\", \"333\"")

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.PreconditionFailed
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [||]
    }

[<Fact>]
let ``HTTP GET with If-Match and matching ETag`` () =
    task {
        let server = WebApp.webApp (createETag "222" |> Some, None)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Match", "\"111\", \"222\", \"333\"")

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP GET with If-Unmodified-Since and no lastModified`` () =
    task {
        let server = WebApp.webApp (None, None)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Unmodified-Since", DateTimeOffset.UtcNow.ToHtmlString())

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP GET with If-Unmodified-Since in the future`` () =
    task {
        let server = WebApp.webApp (None, Some DateTimeOffset.UtcNow)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Unmodified-Since", DateTimeOffset.UtcNow.AddDays(1.0).ToHtmlString())

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP GET with If-Unmodified-Since not in the future but greater than lastModified`` () =
    task {
        let server = WebApp.webApp (None, Some(DateTimeOffset.UtcNow.AddDays(-11.0)))
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Unmodified-Since", DateTimeOffset.UtcNow.AddDays(-10.0).ToHtmlString())

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP GET with If-Unmodified-Since and less than lastModified`` () =
    task {
        let server = WebApp.webApp (None, Some(DateTimeOffset.UtcNow.AddDays(-9.0)))
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Unmodified-Since", DateTimeOffset.UtcNow.AddDays(-10.0).ToHtmlString())

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.PreconditionFailed
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [||]
    }

[<Fact>]
let ``HTTP GET with If-Unmodified-Since not in the future and equal to lastModified`` () =
    task {
        let lastModified = DateTimeOffset(DateTimeOffset.UtcNow.AddDays(-5.0).Date)
        let server = WebApp.webApp (None, Some lastModified)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Unmodified-Since", lastModified.ToHtmlString())

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``ValidatePreconditions with If-Unmodified-Since is equal to lastModified`` () =
    let ctx = DefaultHttpContext() :> HttpContext
    ctx.Request.GetTypedHeaders().IfUnmodifiedSince <- Nullable(DateTimeOffset.Parse "Sat, 01 Jan 2000 00:00:00 GMT")

    let result =
        ctx.ValidatePreconditions(None, Some(DateTimeOffset.Parse "Sat, 01 Jan 2000 00:00:00 GMT"))

    match result with
    | AllConditionsMet -> ()
    | _ -> Assert.True(false, "The request should have met all pre-conditions.")



[<Fact>]
let ``HTTP GET with If-None-Match without ETag`` () =
    task {
        let server = WebApp.webApp (None, None)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-None-Match", "\"111\", \"222\", \"333\"")

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP GET with If-None-Match with non-matching ETag`` () =
    task {
        let server = WebApp.webApp (createETag "444" |> Some, None)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-None-Match", "\"111\", \"222\", \"333\"")

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP GET with If-None-Match with matching ETag`` () =
    task {
        let server = WebApp.webApp (createETag "333" |> Some, None)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-None-Match", "\"111\", \"222\", \"333\"")

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.NotModified
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [||]
    }

[<Fact>]
let ``HTTP HEAD with If-None-Match with matching ETag`` () =
    task {
        let server = WebApp.webApp (createETag "222" |> Some, None)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-None-Match", "\"111\", \"222\", \"333\"")

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, Urls.rangeProcessingDisabled))

        response.StatusCode |> shouldEqual HttpStatusCode.NotModified
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [||]
    }


[<Fact>]
let ``HTTP POST with If-None-Match with matching ETag`` () =
    task {
        let server = WebApp.webApp (createETag "111" |> Some, None)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-None-Match", "\"111\", \"222\", \"333\"")

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Post, Urls.rangeProcessingDisabled))

        response.StatusCode |> shouldEqual HttpStatusCode.PreconditionFailed
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [||]
    }

[<Fact>]
let ``HTTP GET with If-Modified-Since witout lastModified`` () =
    task {
        let server = WebApp.webApp (None, None)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Modified-Since", DateTimeOffset.UtcNow.AddDays(-4.0).ToHtmlString())

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP GET with If-Modified-Since in the future and with lastModified`` () =
    task {
        let server = WebApp.webApp (None, Some(DateTimeOffset.UtcNow.AddDays(5.0)))
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Modified-Since", DateTimeOffset.UtcNow.AddDays(10.0).ToHtmlString())

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.NotModified
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [||]
    }

[<Fact>]
let ``HTTP GET with If-Modified-Since not in the future and with greater lastModified`` () =
    task {
        let server = WebApp.webApp (None, Some(DateTimeOffset.UtcNow.AddDays(-5.0)))
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Modified-Since", DateTimeOffset.UtcNow.AddDays(-10.0).ToHtmlString())

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP GET with If-Modified-Since not in the future and with equal lastModified`` () =
    task {
        let lastModified = DateTimeOffset(DateTimeOffset.UtcNow.AddDays(-7.0).Date)
        let server = WebApp.webApp (None, Some lastModified)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Modified-Since", lastModified.ToHtmlString())

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.NotModified
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [||]
    }

[<Fact>]
let ``HTTP GET with If-Modified-Since not in the future and with smaller lastModified`` () =
    task {
        let server = WebApp.webApp (None, Some(DateTimeOffset.UtcNow.AddDays(-11.0)))
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Modified-Since", DateTimeOffset.UtcNow.AddDays(-10.0).ToHtmlString())

        let! response = client.GetAsync(Urls.rangeProcessingDisabled)

        response.StatusCode |> shouldEqual HttpStatusCode.NotModified
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [||]
    }

[<Fact>]
let ``HTTP POST with If-Modified-Since not in the future and with smaller lastModified`` () =
    task {
        let server = WebApp.webApp (None, Some(DateTimeOffset.UtcNow.AddDays(-11.0)))
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Modified-Since", DateTimeOffset.UtcNow.AddDays(-10.0).ToHtmlString())

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Post, Urls.rangeProcessingDisabled))

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``Endpoint with eTag has ETag HTTP header set`` () =
    task {
        let server = WebApp.webApp (createETag "abc" |> Some, None)
        let client = server.CreateClient()

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Post, Urls.rangeProcessingDisabled))

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Headers.ETag |> string |> shouldEqual "\"abc\""
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``Endpoint with weak eTag has ETag HTTP header set`` () =
    task {
        let server = WebApp.webApp (createWeakETag "abc" |> Some, None)
        let client = server.CreateClient()

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Post, Urls.rangeProcessingDisabled))

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        string response.Headers.ETag |> string |> shouldEqual "W/\"abc\""
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``Endpoint with lastModified has Last-Modified HTTP header set`` () =
    task {
        let lastModified = DateTimeOffset(DateTimeOffset.UtcNow.AddDays(-7.0).Date)
        let server = WebApp.webApp (None, Some lastModified)
        let client = server.CreateClient()

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Post, Urls.rangeProcessingDisabled))

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.LastModified |> shouldEqual lastModified
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP GET with matching If-Match ignores non-matching If-Unmodified-Since`` () =
    task {
        let lastModified = DateTimeOffset.UtcNow.AddDays(-9.0)
        let ifUnmodifiedSince = lastModified.AddDays(-1.0).ToHtmlString()
        let server = WebApp.webApp (createETag "abc" |> Some, Some lastModified)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-Match", "\"abc\"")
        client.DefaultRequestHeaders.Add("If-Unmodified-Since", ifUnmodifiedSince)

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Post, Urls.rangeProcessingDisabled))

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }

[<Fact>]
let ``HTTP GET with non-matching If-None-Match ignores not matching If-Modified-Since`` () =
    task {
        let ifNoneMatch = "\"123\""
        let lastModified = DateTimeOffset.UtcNow.AddDays(-5.0)
        let ifModifiedSince = lastModified.AddDays(1.0).ToHtmlString()
        let server = WebApp.webApp (createETag "abc" |> Some, Some lastModified)
        let client = server.CreateClient()
        client.DefaultRequestHeaders.Add("If-None-Match", ifNoneMatch)
        client.DefaultRequestHeaders.Add("If-Modified-Since", ifModifiedSince)

        let! response = client.SendAsync(new HttpRequestMessage(HttpMethod.Post, Urls.rangeProcessingDisabled))

        response.StatusCode |> shouldEqual HttpStatusCode.OK
        response.Content.Headers.ContentLength |> shouldEqual 62L
        let! bytes = response.Content.ReadAsByteArrayAsync()
        bytes |> shouldEqual [| 48uy; 49uy; 50uy; 51uy; 52uy; 53uy; 54uy; 55uy; 56uy; 57uy; 97uy; 98uy; 99uy; 100uy; 101uy; 102uy; 103uy; 104uy; 105uy; 106uy; 107uy; 108uy; 109uy; 110uy; 111uy; 112uy; 113uy; 114uy; 115uy; 116uy; 117uy; 118uy; 119uy; 120uy; 121uy; 122uy; 65uy; 66uy; 67uy; 68uy; 69uy; 70uy; 71uy; 72uy; 73uy; 74uy; 75uy; 76uy; 77uy; 78uy; 79uy; 80uy; 81uy; 82uy; 83uy; 84uy; 85uy; 86uy; 87uy; 88uy; 89uy; 90uy |]
    }
