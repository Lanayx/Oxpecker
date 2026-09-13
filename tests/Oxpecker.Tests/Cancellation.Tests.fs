module Oxpecker.Tests.Cancellation

open System
open System.Collections.Generic
open System.IO
open System.IO.Pipelines
open System.Net
open System.Net.Http
open System.Text
open System.Text.RegularExpressions
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Features
open Microsoft.AspNetCore.Http.Timeouts
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Oxpecker
open Oxpecker.ViewEngine
open Xunit
open FsUnit.Light

#nowarn "3391"

// ---------------------------------
// Fakes
// ---------------------------------

/// An asynchronous source of items that records the token it was enumerated with, how many times the next item
/// was requested and whether the enumerator was disposed. `beforeMoveNext` is awaited before each item is delivered;
/// it receives the number of the call (starting at 1) and the enumeration token.
type private AsyncSource<'T>(items: 'T list, beforeMoveNext: int -> CancellationToken -> Task) =
    new(items: 'T list) = AsyncSource<'T>(items, (fun _ _ -> Task.CompletedTask))
    member val Token = CancellationToken.None with get, set
    member val MoveNextCalls = 0 with get, set
    member val Disposed = false with get, set

    interface IAsyncEnumerable<'T> with
        member this.GetAsyncEnumerator token =
            this.Token <- token
            let mutable remaining = items
            let mutable current = Unchecked.defaultof<'T>
            { new IAsyncEnumerator<'T> with
                member _.Current = current
                member _.MoveNextAsync() =
                    ValueTask<bool>(
                        task {
                            do! Task.Yield()
                            this.MoveNextCalls <- this.MoveNextCalls + 1
                            do! beforeMoveNext this.MoveNextCalls token
                            match remaining with
                            | [] -> return false
                            | item :: rest ->
                                current <- item
                                remaining <- rest
                                return true
                        }
                    )
                member _.DisposeAsync() =
                    this.Disposed <- true
                    ValueTask()
            }

/// Cancels the request when the item number `call` is requested and then waits with the enumeration token, so a
/// producer that was not given the request token fails with a `TimeoutException` instead of hanging the test.
let private cancelAt (call: int) (cts: CancellationTokenSource) : int -> CancellationToken -> Task =
    fun currentCall token ->
        if currentCall = call then
            cts.Cancel()
            Task.Delay(Timeout.Infinite, token).WaitAsync(TimeSpan.FromSeconds 5.)
        else
            Task.CompletedTask

/// Fails as soon as the enumeration token is cancelled, like a producer awaiting with it would
let private throwIfCancelled: int -> CancellationToken -> Task =
    fun _ token ->
        token.ThrowIfCancellationRequested()
        Task.CompletedTask

/// A part that records the token it was written with
type private RecordingPart() =
    member val Token = CancellationToken.None with get, set

    interface IMultipartPart with
        member this.WriteAsync(writer, cancellationToken) =
            this.Token <- cancellationToken
            Encoding.UTF8.GetBytes("Content-Type: text/plain\r\n\r\npart".AsSpan(), writer)
            |> ignore
            Task.CompletedTask

type private LogEntry = {
    Category: string
    Level: LogLevel
    Message: string
}

type private RecordingLogger(category: string, entries: ResizeArray<LogEntry>) =
    interface ILogger with
        member _.BeginScope _ = null
        member _.IsEnabled _ = true
        member _.Log(logLevel, _, state, exn, formatter) =
            lock entries (fun () ->
                entries.Add {
                    Category = category
                    Level = logLevel
                    Message = formatter.Invoke(state, exn)
                })

type private RecordingLoggerProvider(entries: ResizeArray<LogEntry>) =
    interface ILoggerProvider with
        member _.CreateLogger category = RecordingLogger(category, entries)
        member _.Dispose() = ()

/// Response feature reporting that the response has already started
type private StartedResponseFeature() =
    inherit HttpResponseFeature()
    override _.HasStarted = true

// ---------------------------------
// Helpers
// ---------------------------------

let private createContext () =
    let ctx = DefaultHttpContext()
    ctx.Response.Body <- new MemoryStream()
    ctx

/// A context whose request has already been aborted
let private abortedContext () =
    let ctx = createContext()
    let cts = new CancellationTokenSource()
    cts.Cancel()
    ctx.RequestAborted <- cts.Token
    ctx

let private configureServices (configure: IServiceCollection -> unit) (ctx: HttpContext) =
    let services = ServiceCollection()
    configure services
    ctx.RequestServices <- services.BuildServiceProvider()
    ctx

let private withJsonSerializer (ctx: HttpContext) =
    ctx
    |> configureServices(fun services -> services.AddSingleton<IJsonSerializer>(SystemTextJsonSerializer()) |> ignore)

let private withModelBinder (ctx: HttpContext) =
    ctx
    |> configureServices(fun services -> services.AddSingleton<IModelBinder>(ModelBinder()) |> ignore)

let private addRecordingLogging (entries: ResizeArray<LogEntry>) (services: IServiceCollection) =
    services.AddLogging(fun builder ->
        builder.SetMinimumLevel(LogLevel.Debug).AddProvider(new RecordingLoggerProvider(entries))
        |> ignore)
    |> ignore

let private withLogging (entries: ResizeArray<LogEntry>) (ctx: HttpContext) =
    ctx |> configureServices(addRecordingLogging entries)

let private readBody (ctx: HttpContext) =
    ctx.Response.Body.Seek(0, SeekOrigin.Begin) |> ignore
    use reader = new StreamReader(ctx.Response.Body, Encoding.UTF8)
    reader.ReadToEnd()

let private responseContentType (ctx: HttpContext) =
    ctx.Response.Headers.ContentType.ToString()

let private getBoundary (contentType: string) =
    let m = Regex.Match(contentType, "boundary=([^;]+)")
    m.Success |> shouldEqual true
    m.Groups[1].Value

/// Asserts that the action fails with an `OperationCanceledException` or a subtype such as `TaskCanceledException`
let private shouldBeCancelled (action: unit -> Task) =
    task {
        let! _ = Assert.ThrowsAnyAsync<OperationCanceledException>(fun () -> action())
        ()
    }

let private oxpeckerEntries (entries: ResizeArray<LogEntry>) =
    lock entries (fun () -> entries |> Seq.filter(fun e -> e.Category.StartsWith "Oxpecker") |> List.ofSeq)

let private shouldHaveLoggedAbortAtDebugOnly (entries: ResizeArray<LogEntry>) =
    let entries = oxpeckerEntries entries
    entries
    |> List.exists(fun e -> e.Level = LogLevel.Debug && e.Message.Contains "Request aborted")
    |> shouldEqual true
    entries
    |> List.exists(fun e -> e.Level >= LogLevel.Warning)
    |> shouldEqual false

let private shouldHaveLoggedTimeoutAsWarningOnly (entries: ResizeArray<LogEntry>) =
    let entries = oxpeckerEntries entries
    entries
    |> List.exists(fun e -> e.Level = LogLevel.Warning && e.Message.Contains "Request timed out")
    |> shouldEqual true
    entries
    |> List.exists(fun e -> e.Level >= LogLevel.Error || e.Message.Contains "Request aborted")
    |> shouldEqual false

[<CLIMutable>]
type Person = { Name: string }

// ---------------------------------
// Token propagation
// ---------------------------------

[<Fact>]
let ``WriteMultipartChunked passes RequestAborted to the enumerator and to each part`` () =
    task {
        let ctx = createContext()
        use cts = new CancellationTokenSource()
        ctx.RequestAborted <- cts.Token
        let parts = [ RecordingPart(); RecordingPart() ]
        let source =
            AsyncSource<IMultipartPart>(parts |> List.map(fun part -> part :> IMultipartPart))

        do! ctx.WriteMultipartChunked source

        source.Token |> shouldEqual cts.Token
        source.Token.CanBeCanceled |> shouldEqual true
        for part in parts do
            part.Token |> shouldEqual cts.Token
        source.Disposed |> shouldEqual true
        let boundary = getBoundary(responseContentType ctx)
        readBody ctx
        |> shouldEqual(
            $"--{boundary}\r\nContent-Type: text/plain\r\n\r\npart"
            + $"\r\n--{boundary}\r\nContent-Type: text/plain\r\n\r\npart"
            + $"\r\n--{boundary}--\r\n"
        )
    }

[<Fact>]
let ``WriteMultipart passes RequestAborted to each part`` () =
    task {
        let ctx = createContext()
        use cts = new CancellationTokenSource()
        ctx.RequestAborted <- cts.Token
        let part = RecordingPart()

        do! ctx.WriteMultipart [ part ]

        part.Token |> shouldEqual cts.Token
        part.Token.CanBeCanceled |> shouldEqual true
    }

[<Fact>]
let ``JsonPart passes the cancellation token to the serializer`` () =
    task {
        let part = MultipartPart.Json {| Id = 1 |}
        use cts = new CancellationTokenSource()
        cts.Cancel()
        let writer = PipeWriter.Create(new MemoryStream())

        do! shouldBeCancelled(fun () -> part.WriteAsync(writer, cts.Token))
    }

[<Fact>]
let ``WriteHtmlChunked passes RequestAborted to the enumerator and disposes it`` () =
    task {
        let ctx = createContext()
        use cts = new CancellationTokenSource()
        ctx.RequestAborted <- cts.Token
        let source =
            AsyncSource<HtmlElement>([ div() { "a" } :> HtmlElement; div() { "b" } :> HtmlElement ])

        do! ctx.WriteHtmlChunked source

        source.Token |> shouldEqual cts.Token
        source.Disposed |> shouldEqual true
        readBody ctx |> shouldEqual "<div>a</div><div>b</div>"
    }

[<Fact>]
let ``WriteJsonChunked passes RequestAborted to the enumerator`` () =
    task {
        let ctx = createContext() |> withJsonSerializer
        use cts = new CancellationTokenSource()
        ctx.RequestAborted <- cts.Token
        let source = AsyncSource<int>([ 1; 2; 3 ])

        do! ctx.WriteJsonChunked source

        source.Token |> shouldEqual cts.Token
        source.Disposed |> shouldEqual true
        readBody ctx |> shouldEqual "[1,2,3]"
    }

// ---------------------------------
// Cancellation while the producer awaits
// ---------------------------------

[<Fact>]
let ``WriteMultipartChunked fails with OperationCanceledException and stops enumerating when the request is aborted``
    ()
    =
    task {
        let ctx = createContext()
        use cts = new CancellationTokenSource()
        ctx.RequestAborted <- cts.Token
        let source =
            AsyncSource<IMultipartPart>([ MultipartPart.Text "first"; MultipartPart.Text "second" ], cancelAt 2 cts)

        do! shouldBeCancelled(fun () -> ctx.WriteMultipartChunked source)

        source.MoveNextCalls |> shouldEqual 2
        source.Disposed |> shouldEqual true
        let boundary = getBoundary(responseContentType ctx)
        readBody ctx
        |> shouldEqual $"--{boundary}\r\nContent-Type: text/plain; charset=utf-8\r\n\r\nfirst\r\n--{boundary}"
    }

[<Fact>]
let ``WriteHtmlChunked fails with OperationCanceledException and disposes the enumerator when the request is aborted``
    ()
    =
    task {
        let ctx = createContext()
        use cts = new CancellationTokenSource()
        ctx.RequestAborted <- cts.Token
        let source =
            AsyncSource<HtmlElement>(
                [ div() { "first" } :> HtmlElement; div() { "second" } :> HtmlElement ],
                cancelAt 2 cts
            )

        do! shouldBeCancelled(fun () -> ctx.WriteHtmlChunked source :> Task)

        source.MoveNextCalls |> shouldEqual 2
        source.Disposed |> shouldEqual true
        readBody ctx |> shouldEqual "<div>first</div>"
    }

[<Fact>]
let ``WriteJsonChunked fails with OperationCanceledException when the request is aborted`` () =
    task {
        let ctx = createContext() |> withJsonSerializer
        use cts = new CancellationTokenSource()
        ctx.RequestAborted <- cts.Token
        let source = AsyncSource<int>([ 1; 2 ], cancelAt 2 cts)

        do! shouldBeCancelled(fun () -> ctx.WriteJsonChunked source)

        source.MoveNextCalls |> shouldEqual 2
        source.Disposed |> shouldEqual true
    }

// ---------------------------------
// Already aborted requests
// ---------------------------------

[<Fact>]
let ``WriteBytes throws OperationCanceledException and writes nothing when the request is already aborted`` () =
    task {
        let ctx = abortedContext()

        do! shouldBeCancelled(fun () -> ctx.WriteBytes(Encoding.UTF8.GetBytes "Hello"))

        readBody ctx |> shouldEqual ""
    }

[<Fact>]
let ``WriteText throws OperationCanceledException and writes nothing when the request is already aborted`` () =
    task {
        let ctx = abortedContext()

        do! shouldBeCancelled(fun () -> ctx.WriteText "Hello")

        readBody ctx |> shouldEqual ""
    }

[<Fact>]
let ``WriteJson throws OperationCanceledException and writes nothing when the request is already aborted`` () =
    task {
        let ctx = abortedContext() |> withJsonSerializer

        do! shouldBeCancelled(fun () -> ctx.WriteJson {| Hello = "World" |})

        readBody ctx |> shouldEqual ""
        ctx.Response.Headers.ContentLength |> shouldEqual(Nullable())
    }

[<Fact>]
let ``WriteJsonChunked throws OperationCanceledException and writes nothing when the request is already aborted`` () =
    task {
        let ctx = abortedContext() |> withJsonSerializer

        do! shouldBeCancelled(fun () -> ctx.WriteJsonChunked {| Hello = "World" |})

        readBody ctx |> shouldEqual ""
    }

[<Fact>]
let ``WriteHtmlView throws OperationCanceledException and writes nothing when the request is already aborted`` () =
    task {
        let ctx = abortedContext()

        do! shouldBeCancelled(fun () -> ctx.WriteHtmlView(div() { "Hello" }))

        readBody ctx |> shouldEqual ""
        ctx.Response.Headers.ContentLength |> shouldEqual(Nullable())
    }

[<Fact>]
let ``WriteHtmlViewChunked throws OperationCanceledException and writes nothing when the request is already aborted``
    ()
    =
    task {
        let ctx = abortedContext()

        do! shouldBeCancelled(fun () -> ctx.WriteHtmlViewChunked(div() { "Hello" }) :> Task)

        readBody ctx |> shouldEqual ""
    }

[<Fact>]
let ``WriteHtmlChunked throws OperationCanceledException and writes nothing when the request is already aborted`` () =
    task {
        let ctx = abortedContext()
        let source =
            AsyncSource<HtmlElement>([ div() { "Hello" } :> HtmlElement ], throwIfCancelled)

        do! shouldBeCancelled(fun () -> ctx.WriteHtmlChunked source :> Task)

        readBody ctx |> shouldEqual ""
        source.Disposed |> shouldEqual true
    }

[<Fact>]
let ``WriteMultipart throws OperationCanceledException and writes nothing when the request is already aborted`` () =
    task {
        let ctx = abortedContext()

        do! shouldBeCancelled(fun () -> ctx.WriteMultipart [ MultipartPart.Text "Hello" ] :> Task)

        readBody ctx |> shouldEqual ""
        responseContentType ctx |> shouldEqual ""
        ctx.Response.Headers.ContentLength |> shouldEqual(Nullable())
    }

[<Fact>]
let ``WriteMultipartChunked throws OperationCanceledException and writes nothing when the request is already aborted``
    ()
    =
    task {
        let ctx = abortedContext()
        let source =
            AsyncSource<IMultipartPart>([ MultipartPart.Text "Hello" ], throwIfCancelled)

        do! shouldBeCancelled(fun () -> ctx.WriteMultipartChunked source)

        readBody ctx |> shouldEqual ""
        responseContentType ctx |> shouldEqual ""
        source.Disposed |> shouldEqual true
    }

[<Fact>]
let ``WriteStream throws OperationCanceledException instead of aborting silently when the request is already aborted``
    ()
    =
    task {
        let ctx = abortedContext()
        use stream = new MemoryStream(Encoding.UTF8.GetBytes "Hello")

        do! shouldBeCancelled(fun () -> ctx.WriteStream(false, stream, None, None) :> Task)

        readBody ctx |> shouldEqual ""
    }

// ---------------------------------
// Requests aborted while the response is rendered in memory
// ---------------------------------

/// An element that aborts the request while it is being rendered
let private abortingElement (cts: CancellationTokenSource) =
    { new HtmlElement with
        member _.Render sb =
            cts.Cancel()
            sb.Append "aborted" |> ignore
    }

/// A part that aborts the request while it is being written
type private AbortingPart(cts: CancellationTokenSource) =
    interface IMultipartPart with
        member _.WriteAsync(writer, _) =
            cts.Cancel()
            Encoding.UTF8.GetBytes("Content-Type: text/plain\r\n\r\naborted".AsSpan(), writer)
            |> ignore
            Task.CompletedTask

[<Fact>]
let ``WriteHtmlView with HEAD fails with OperationCanceledException when the request is aborted while rendering`` () =
    task {
        let ctx = createContext()
        ctx.Request.Method <- HttpMethods.Head
        use cts = new CancellationTokenSource()
        ctx.RequestAborted <- cts.Token

        do! shouldBeCancelled(fun () -> ctx.WriteHtmlView(abortingElement cts))

        ctx.Response.Headers.ContentLength |> shouldEqual(Nullable())
    }

[<Fact>]
let ``WriteMultipart with HEAD fails with OperationCanceledException when the request is aborted while a part is written``
    ()
    =
    task {
        let ctx = createContext()
        ctx.Request.Method <- HttpMethods.Head
        use cts = new CancellationTokenSource()
        ctx.RequestAborted <- cts.Token

        do! shouldBeCancelled(fun () -> ctx.WriteMultipart [ AbortingPart cts ] :> Task)

        responseContentType ctx |> shouldEqual ""
        ctx.Response.Headers.ContentLength |> shouldEqual(Nullable())
    }

// ---------------------------------
// Model binding
// ---------------------------------

[<Fact>]
let ``BindJson throws OperationCanceledException instead of ModelBindException when the request is aborted`` () =
    task {
        let ctx = abortedContext() |> withJsonSerializer
        ctx.Request.Body <- new MemoryStream(Encoding.UTF8.GetBytes """{"name":"x"}""")
        ctx.Request.ContentType <- "application/json"

        do! shouldBeCancelled(fun () -> ctx.BindJson<Person>() :> Task)
    }

[<Fact>]
let ``BindForm throws OperationCanceledException instead of ModelBindException when the request is aborted`` () =
    task {
        let ctx = abortedContext() |> withModelBinder
        ctx.Request.Body <- new MemoryStream(Encoding.UTF8.GetBytes "Name=x")
        ctx.Request.ContentType <- "application/x-www-form-urlencoded"

        do! shouldBeCancelled(fun () -> ctx.BindForm<Person>() :> Task)
    }

[<Fact>]
let ``BindJson still wraps invalid JSON into ModelBindException`` () =
    task {
        let ctx = createContext() |> withJsonSerializer
        ctx.Request.Body <- new MemoryStream(Encoding.UTF8.GetBytes "not json")
        ctx.Request.ContentType <- "application/json"

        let! _ = Assert.ThrowsAsync<ModelBindException>(fun () -> ctx.BindJson<Person>() :> Task)
        ()
    }

[<Fact>]
let ``BindForm still wraps an invalid content type into ModelBindException`` () =
    task {
        let ctx = createContext() |> withModelBinder
        ctx.Request.Body <- new MemoryStream(Encoding.UTF8.GetBytes "Name=x")
        ctx.Request.ContentType <- "text/plain"

        let! _ = Assert.ThrowsAsync<ModelBindException>(fun () -> ctx.BindForm<Person>() :> Task)
        ()
    }

// ---------------------------------
// Default.exceptionMiddleware
// ---------------------------------

[<Fact>]
let ``Default.exceptionMiddleware answers an aborted request with 499 and logs at Debug level`` () =
    task {
        let entries = ResizeArray<LogEntry>()
        let ctx = abortedContext() |> withLogging entries
        // a write that set its headers before observing the cancellation, like WriteText does
        let cancelledWrite =
            RequestDelegate(fun ctx ->
                ctx.Response.ContentType <- "text/plain; charset=utf-8"
                ctx.Response.ContentLength <- 5L
                Task.FromCanceled ctx.RequestAborted)

        do! Default.exceptionMiddleware ctx cancelledWrite

        ctx.Response.StatusCode |> shouldEqual StatusCodes.Status499ClientClosedRequest
        readBody ctx |> shouldEqual ""
        responseContentType ctx |> shouldEqual ""
        ctx.Response.Headers.ContentLength |> shouldEqual(Nullable())
        shouldHaveLoggedAbortAtDebugOnly entries
    }

[<Fact>]
let ``Default.exceptionMiddleware keeps the status code of an already started response when the request is aborted``
    ()
    =
    task {
        let entries = ResizeArray<LogEntry>()
        let ctx = abortedContext() |> withLogging entries
        ctx.Features.Set<IHttpResponseFeature>(StartedResponseFeature())

        do! Default.exceptionMiddleware ctx (RequestDelegate(fun _ -> Task.FromCanceled ctx.RequestAborted))

        ctx.Response.StatusCode |> shouldEqual StatusCodes.Status200OK
        shouldHaveLoggedAbortAtDebugOnly entries
    }

[<Fact>]
let ``Default.exceptionMiddleware treats OperationCanceledException as an error when the request is not aborted`` () =
    task {
        let entries = ResizeArray<LogEntry>()
        let ctx = createContext() |> withLogging entries
        use cts = new CancellationTokenSource()
        cts.Cancel()

        do! Default.exceptionMiddleware ctx (RequestDelegate(fun _ -> Task.FromCanceled cts.Token))

        ctx.Response.StatusCode |> shouldEqual StatusCodes.Status500InternalServerError
        readBody ctx |> shouldEqual "Internal server error"
        oxpeckerEntries entries
        |> List.exists(fun e -> e.Level = LogLevel.Error)
        |> shouldEqual true
    }

/// The feature the request-timeouts middleware exposes, with the timeout already elapsed
type private ElapsedTimeoutFeature(token: CancellationToken) =
    interface IHttpRequestTimeoutFeature with
        member _.RequestTimeoutToken = token
        member _.DisableTimeout() = ()

[<Fact>]
let ``Default.exceptionMiddleware answers a request timeout with 504 and logs a warning`` () =
    task {
        let entries = ResizeArray<LogEntry>()
        let ctx = abortedContext() |> withLogging entries
        // UseRequestTimeouts replaces RequestAborted with a linked token and exposes the timeout token
        ctx.Features.Set<IHttpRequestTimeoutFeature>(ElapsedTimeoutFeature ctx.RequestAborted)
        let cancelledWrite = RequestDelegate(fun _ -> Task.FromCanceled ctx.RequestAborted)

        do! Default.exceptionMiddleware ctx cancelledWrite

        ctx.Response.StatusCode |> shouldEqual StatusCodes.Status504GatewayTimeout
        readBody ctx |> shouldEqual ""
        shouldHaveLoggedTimeoutAsWarningOnly entries
    }

// ---------------------------------
// End to end
// ---------------------------------

module private WebApp =

    /// Completes `finished` with the final status code once the rest of the pipeline is done
    let observe (finished: TaskCompletionSource<int>) (ctx: HttpContext) (next: RequestDelegate) =
        task {
            try
                do! next.Invoke ctx
            finally
                finished.TrySetResult ctx.Response.StatusCode |> ignore
        }
        :> Task

    let startWith
        (useRequestTimeouts: bool)
        (entries: ResizeArray<LogEntry>)
        (finished: TaskCompletionSource<int>)
        (endpoints: Endpoint list)
        =
        task {
            let host =
                HostBuilder()
                    .ConfigureWebHost(fun webHostBuilder ->
                        webHostBuilder
                            .UseTestServer()
                            .Configure(fun app ->
                                let app = app.UseRouting().Use(observe finished)
                                // the request timeouts middleware is registered outside Default.exceptionMiddleware,
                                // so the timeout cancellation reaches the latter, which answers it with 504
                                let app = if useRequestTimeouts then app.UseRequestTimeouts() else app
                                app.Use(Default.exceptionMiddleware).UseOxpecker(endpoints) |> ignore)
                            .ConfigureServices(fun services ->
                                services.AddRouting().AddOxpecker().AddRequestTimeouts()
                                |> addRecordingLogging entries)
                        |> ignore)
                    .Build()
            do! host.StartAsync()
            return host
        }

    let start entries finished endpoints =
        startWith false entries finished endpoints

[<Fact>]
let ``HTTP GET chunked multipart endpoint stops the producer when the client disconnects`` () =
    task {
        let entries = ResizeArray<LogEntry>()
        let finished = TaskCompletionSource<int>()
        let secondPartRequested = TaskCompletionSource()
        let source =
            AsyncSource<IMultipartPart>(
                [ MultipartPart.Text "first"; MultipartPart.Text "second" ],
                fun call token ->
                    if call = 2 then
                        secondPartRequested.TrySetResult() |> ignore
                        Task.Delay(Timeout.Infinite, token).WaitAsync(TimeSpan.FromSeconds 10.)
                    else
                        Task.CompletedTask
            )
        use! host = WebApp.start entries finished [ route "/chunked" (multipartChunked source) ]
        let client = host.GetTestClient()

        let! response = client.GetAsync("/chunked", HttpCompletionOption.ResponseHeadersRead)
        response.StatusCode |> shouldEqual HttpStatusCode.OK
        do! secondPartRequested.Task.WaitAsync(TimeSpan.FromSeconds 10.)
        // disposing the response while it is still being streamed aborts the request on the server
        response.Dispose()
        let! status = finished.Task.WaitAsync(TimeSpan.FromSeconds 10.)

        source.Token.IsCancellationRequested |> shouldEqual true
        source.MoveNextCalls |> shouldEqual 2
        source.Disposed |> shouldEqual true
        // the response had already started, so the status code stays
        status |> shouldEqual StatusCodes.Status200OK
        shouldHaveLoggedAbortAtDebugOnly entries
    }

[<Fact>]
let ``HTTP GET endpoint aborted before the response starts answers 499`` () =
    task {
        let entries = ResizeArray<LogEntry>()
        let finished = TaskCompletionSource<int>()
        let started = TaskCompletionSource()
        let slowHandler: EndpointHandler =
            fun ctx ->
                task {
                    started.TrySetResult() |> ignore
                    do! Task.Delay(Timeout.Infinite, ctx.RequestAborted)
                }
                :> Task
        use! host = WebApp.start entries finished [ route "/slow" slowHandler ]
        let client = host.GetTestClient()
        use cts = new CancellationTokenSource()

        let requestTask = client.GetAsync("/slow", cts.Token)
        do! started.Task.WaitAsync(TimeSpan.FromSeconds 10.)
        cts.Cancel()
        do! shouldBeCancelled(fun () -> requestTask :> Task)
        let! status = finished.Task.WaitAsync(TimeSpan.FromSeconds 10.)

        status |> shouldEqual StatusCodes.Status499ClientClosedRequest
        shouldHaveLoggedAbortAtDebugOnly entries
    }

[<Fact>]
let ``HTTP GET endpoint exceeding its request timeout is answered with 504`` () =
    task {
        // note: the request timeouts middleware does nothing while a debugger is attached
        let entries = ResizeArray<LogEntry>()
        let finished = TaskCompletionSource<int>()
        let slowHandler: EndpointHandler =
            fun ctx -> Task.Delay(Timeout.Infinite, ctx.RequestAborted)
        let endpoints = [
            route "/slow" slowHandler
            |> configureEndpoint _.WithRequestTimeout(TimeSpan.FromMilliseconds 100.)
        ]
        use! host = WebApp.startWith true entries finished endpoints
        let client = host.GetTestClient()

        let! response = client.GetAsync "/slow"
        let! status = finished.Task.WaitAsync(TimeSpan.FromSeconds 10.)

        response.StatusCode |> shouldEqual HttpStatusCode.GatewayTimeout
        status |> shouldEqual StatusCodes.Status504GatewayTimeout
        shouldHaveLoggedTimeoutAsWarningOnly entries
    }
