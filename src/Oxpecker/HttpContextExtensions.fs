namespace Oxpecker

open System
open System.Collections.Generic
open System.IO
open System.Runtime.CompilerServices
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Extensions
open Microsoft.AspNetCore.WebUtilities
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Net.Http.Headers
open Oxpecker.ViewEngine

type RouteParseException(message: string, ex) =
    inherit Exception(message, ex)

type ModelBindException(message: string, ex) =
    inherit Exception(message, ex)

type HttpContextExtensions() =

    /// <summary>
    /// Returns the entire request URL in a fully escaped form, which is suitable for use in HTTP headers and other operations.
    /// </summary>
    /// <returns>Returns an instance of `'T`.</returns>
    [<Extension>]
    static member GetRequestUrl(ctx: HttpContext) = ctx.Request.GetEncodedUrl()

    /// <summary>
    /// Tries to get the value from the route values collection and cast it to `'T`.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The name of the HTTP header.</param>
    /// <returns>Returns a <see cref="System.String"/> URL.</returns>
    [<Extension>]
    static member TryGetRouteValue<'T>(ctx: HttpContext, key: string) =
        match ctx.Request.RouteValues.TryGetValue key with
        | true, value -> value :?> 'T |> Some
        | _ -> None

    /// <summary>
    /// Tries to retrieve the <see cref="System.String"/> value of a cookie from the request
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The name of the cookie.</param>
    /// <returns>Returns Some string if the cookie was set, otherwise returns None.</returns>
    [<Extension>]
    static member TryGetCookieValue(ctx: HttpContext, key: string) =
        match ctx.Request.Cookies.TryGetValue key with
        | true, value -> value |> Some
        | _ -> None

    /// <summary>
    /// Tries to get the <see cref="System.String"/> value of a HTTP header from the request.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The name of the HTTP header.</param>
    /// <returns> Returns Some string if the HTTP header was present in the request, otherwise returns None.</returns>
    [<Extension>]
    static member TryGetHeaderValue(ctx: HttpContext, key: string) =
        match ctx.Request.Headers.TryGetValue key with
        | true, value -> value |> string |> Some
        | _ -> None

    /// <summary>
    /// Tries to get the string seq value of a HTTP header from the request.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The name of the HTTP header.</param>
    /// <returns> Returns Some string seq if the HTTP header was present in the request, otherwise returns None.</returns>
    [<Extension>]
    static member TryGetHeaderValues(ctx: HttpContext, key: string) =
        match ctx.Request.Headers.TryGetValue key with
        | true, value -> value |> Seq.map string |> Some
        | _ -> None

    /// <summary>
    ///  Tries to get the <see cref="System.String"/> value of a query string parameter from the request.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The name of the query string parameter.</param>
    /// <returns>Returns Some string if the parameter was present in the request's query string, otherwise returns None.</returns>
    [<Extension>]
    static member TryGetQueryValue(ctx: HttpContext, key: string) =
        match ctx.Request.Query.TryGetValue key with
        | true, value -> value |> string |> Some
        | _ -> None

    /// <summary>
    /// Tries to get the string seq value of a query string parameter from the request.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The name of the query string parameter.</param>
    /// <returns>Returns Some string seq if the parameter was present in the request's query string, otherwise returns None.</returns>
    [<Extension>]
    static member TryGetQueryValues(ctx: HttpContext, key: string) =
        match ctx.Request.Query.TryGetValue key with
        | true, value -> value |> Seq.map string |> Some
        | _ -> None

    /// <summary>
    /// Tries to get the <see cref="System.String"/> value of a form parameter from the request.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The name of the query string parameter.</param>
    /// <returns>Returns Some string if the parameter was present in the request's form, otherwise returns None.</returns>
    [<Extension>]
    static member TryGetFormValue(ctx: HttpContext, key: string) =
        match ctx.Request.Form.TryGetValue key with
        | true, value -> value |> string |> Some
        | _ -> None

    /// <summary>
    /// Tries to get the string seq value of a form parameter from the request.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The name of the query string parameter.</param>
    /// <returns>Returns Some string seq if the parameter was present in the request's form, otherwise returns None.</returns>
    [<Extension>]
    static member TryGetFormValues(ctx: HttpContext, key: string) =
        match ctx.Request.Form.TryGetValue key with
        | true, value -> value |> Seq.map string |> Some
        | _ -> None

    /// <summary>
    /// Gets an instance of `'T` from the request's service container.
    /// </summary>
    /// <returns>Returns an instance of `'T`.</returns>
    [<Extension>]
    static member GetService(ctx: HttpContext) =
        ctx.RequestServices.GetRequiredService<'T>()

    /// <summary>
    /// Gets an instance of <see cref="Microsoft.Extensions.Logging.ILogger{T}" /> from the request's service container.
    ///
    /// The type `'T` should represent the class or module from where the logger gets instantiated.
    /// </summary>
    /// <returns> Returns an instance of <see cref="Microsoft.Extensions.Logging.ILogger{T}" />.</returns>
    [<Extension>]
    static member GetLogger<'T>(ctx: HttpContext) = ctx.GetService<ILogger<'T>>()

    /// <summary>
    /// Gets an instance of <see cref="Microsoft.Extensions.Logging.ILogger" /> from the request's service container.    ///
    /// </summary>
    /// <returns> Returns an instance of <see cref="Microsoft.Extensions.Logging.ILogger" />.</returns>
    [<Extension>]
    static member GetLogger(ctx: HttpContext) = ctx.GetService<ILogger>()

    /// <summary>
    /// Gets an instance of <see cref="Microsoft.Extensions.Logging.ILogger"/> from the request's service container.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="categoryName">The category name for messages produced by this logger.</param>
    /// <returns>Returns an instance of <see cref="Microsoft.Extensions.Logging.ILogger"/>.</returns>
    [<Extension>]
    static member GetLogger(ctx: HttpContext, categoryName: string) =
        let loggerFactory = ctx.GetService<ILoggerFactory>()
        loggerFactory.CreateLogger categoryName

    /// <summary>
    /// Gets an instance of <see cref="Microsoft.AspNetCore.Hosting.IWebHostEnvironment"/> from the request's service container.
    /// </summary>
    /// <returns>Returns an instance of <see cref="Microsoft.AspNetCore.Hosting.IWebHostEnvironment"/>.</returns>
    [<Extension>]
    static member GetWebHostEnvironment(ctx: HttpContext) = ctx.GetService<IWebHostEnvironment>()

    /// <summary>
    /// Gets an instance of <see cref="Oxpecker.Serializers.IJsonSerializer"/> from the request's service container.
    /// </summary>
    /// <returns>Returns an instance of <see cref="Oxpecker.Serializers.IJsonSerializer"/>.</returns>
    [<Extension>]
    static member GetJsonSerializer(ctx: HttpContext) : IJsonSerializer = ctx.GetService<IJsonSerializer>()

    /// <summary>
    /// Gets an instance of <see cref="Oxpecker.IModelBinder"/> from the request's service container.
    /// </summary>
    /// <returns>Returns an instance of <see cref="Oxpecker.IModelBinder"/>.</returns>
    [<Extension>]
    static member GetModelBinder(ctx: HttpContext) : IModelBinder = ctx.GetService<IModelBinder>()

    /// <summary>
    /// Sets the HTTP status code of the response.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="httpStatusCode">The status code to be set in the response. For convenience, you can use the static <see cref="Microsoft.AspNetCore.Http.StatusCodes"/> class for passing in named status codes instead of using pure int values.</param>
    [<Extension>]
    static member SetStatusCode(ctx: HttpContext, httpStatusCode: int) =
        ctx.Response.StatusCode <- httpStatusCode

    /// <summary>
    /// Adds or sets an HTTP header in the response.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The HTTP header name. For convenience, you can use the static <see cref="Microsoft.Net.Http.Headers.HeaderNames"/> class for passing in strongly typed header names instead of using pure `string` values.</param>
    /// <param name="value">The value to be set. Non string values will be converted to a string using the object's ToString() method.</param>
    [<Extension>]
    static member SetHttpHeader(ctx: HttpContext, key: string, value: string) = ctx.Response.Headers[key] <- value

    /// <summary>
    /// Sets the Content-Type HTTP header in the response.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="contentType">The mime type of the response (e.g.: application/json or text/html).</param>
    [<Extension>]
    static member SetContentType(ctx: HttpContext, contentType: string) =
        ctx.SetHttpHeader(HeaderNames.ContentType, contentType)

    /// <summary>
    /// Writes a byte array to the body of the HTTP response and sets the HTTP Content-Length header accordingly.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="bytes">The byte array to be sent back to the client.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteBytes(ctx: HttpContext, bytes: byte array) =
        ctx.Response.ContentLength <- bytes.LongLength
        if ctx.Request.Method <> HttpMethods.Head then
            ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length)
        else
            Task.CompletedTask

    /// <summary>
    /// Writes an UTF-8 encoded string to the body of the HTTP response and sets the HTTP `Content-Length` header accordingly, as well as the `Content-Type` header to `text/plain`.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="str">The string value to be send back to the client.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteText(ctx: HttpContext, str: string) =
        ctx.SetContentType "text/plain; charset=utf-8"
        ctx.WriteBytes(Encoding.UTF8.GetBytes str)

    /// <summary>
    /// Writes an UTF-8 encoded string to the body of the HTTP response and sets the HTTP `Content-Length` header accordingly, as well as the `Content-Type` header to `text/plain`.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="html">The string html value to be send back to the client.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteHtmlString(ctx: HttpContext, html: string) =
        ctx.SetContentType "text/html; charset=utf-8"
        ctx.WriteBytes(Encoding.UTF8.GetBytes html)

    /// <summary>
    /// Serializes an object to JSON and writes the output to the body of the HTTP response.
    /// It also sets the HTTP Content-Type header to application/json and sets the Content-Length header accordingly.
    /// The JSON serializer can be configured in the ASP.NET Core startup code by registering a custom class of type <see cref="Json.ISerializer"/>
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="value">The object to be sent back to the client.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteJson<'T>(ctx: HttpContext, value: 'T) =
        let serializer = ctx.GetJsonSerializer()
        serializer.Serialize(value, ctx, false)

    /// <summary>
    /// Serializes an object to JSON and writes the output to the body of the HTTP response using chunked transfer encoding.
    /// It also sets the HTTP Content-Type header to application/json and sets the Transfer-Encoding header to chunked.
    /// The JSON serializer can be configured in the ASP.NET Core startup code by registering a custom class of type <see cref="Json.ISerializer"/>.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="value">The object to be sent back to the client.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteJsonChunked<'T>(ctx: HttpContext, value: 'T) =
        let serializer = ctx.GetJsonSerializer()
        serializer.Serialize(value, ctx, true)

    /// <summary>
    /// <para>Compiles an `HtmlElement` object to a HTML view and writes the output to the body of the HTTP response.</para>
    /// <para>It also sets the HTTP header `Content-Type` to `text/html` and sets the `Content-Length` header accordingly.</para>
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="htmlView">An `HtmlElement` object to be send back to the client and which represents a valid HTML view.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteHtmlView(ctx: HttpContext, htmlView: #HtmlElement) =
        let memoryStream = recyclableMemoryStreamManager.Value.GetStream()
        ctx.Response.ContentType <- "text/html; charset=utf-8"
        if ctx.Request.Method <> HttpMethods.Head then
            task {
                try
                    do! Render.toHtmlDocStreamAsync memoryStream htmlView
                    ctx.Response.ContentLength <- memoryStream.Length
                    memoryStream.Seek(0, SeekOrigin.Begin) |> ignore
                    return! memoryStream.CopyToAsync ctx.Response.Body
                finally
                    memoryStream.Dispose()
            }
            :> Task
        else
            task {
                try
                    do! Render.toHtmlDocStreamAsync memoryStream htmlView
                    ctx.Response.ContentLength <- memoryStream.Length
                finally
                    memoryStream.Dispose()
            }

    /// <summary>
    /// <para>Serializes a stream of HTML elements and writes the output to the body of the HTTP response using chunked transfer encoding.</para>
    /// <para>It also sets the HTTP header `Content-Type` to `text/html` and sets the Transfer-Encoding header to chunked.</para>
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="htmlStream">An `HtmlElement` stream to be send back to the client.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteHtmlChunked(ctx: HttpContext, htmlStream: #IAsyncEnumerable<#HtmlElement>) =
        ctx.Response.ContentType <- "text/html; charset=utf-8"
        let enumerator = htmlStream.GetAsyncEnumerator()
        let textWriter = new HttpResponseStreamWriter(ctx.Response.Body, Encoding.UTF8)
        task {
            use _ = textWriter :> IAsyncDisposable
            while! enumerator.MoveNextAsync() do
                do! Render.toTextWriterAsync textWriter enumerator.Current
        }

    /// <summary>
    /// <para>Serializes an HTML element object and writes the output to the body of the HTTP response using chunked transfer encoding.</para>
    /// <para>It also sets the HTTP header `Content-Type` to `text/html` and sets the Transfer-Encoding header to chunked.</para>
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="htmlElement">An `HtmlElement` object to be send back to the client.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteHtmlViewChunked(ctx: HttpContext, htmlElement: #HtmlElement) =
        ctx.Response.ContentType <- "text/html; charset=utf-8"
        let textWriter = new HttpResponseStreamWriter(ctx.Response.Body, Encoding.UTF8)
        task {
            use _ = textWriter :> IAsyncDisposable
            return! Render.toHtmlDocTextWriterAsync textWriter htmlElement
        }

    /// <summary>
    /// Executes and ASP.NET Core IResult. Note that in most cases the response will be chunked.
    /// </summary>
    [<Extension>]
    static member Write(ctx: HttpContext, result: #IResult) = result.ExecuteAsync(ctx)

    /// <summary>
    /// Uses the <see cref="Serializers.IJsonSerializer"/> to deserialize the entire body of the <see cref="Microsoft.AspNetCore.Http.HttpRequest"/> asynchronously into an object of type 'T.
    /// </summary>
    /// <typeparam name="'T"></typeparam>
    /// <returns>Returns a <see cref="System.Threading.Tasks.Task{T}"/></returns>
    [<Extension>]
    static member BindJson<'T>(ctx: HttpContext) =
        let serializer = ctx.GetJsonSerializer()
        task {
            try
                return! serializer.Deserialize<'T>(ctx)
            with ex ->
                return raise <| ModelBindException("Unable to deserialize model from JSON", ex)
        }

    /// <summary>
    /// Parses all input elements from an HTML form into an object of type 'T.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <typeparam name="'T"></typeparam>
    /// <returns>Returns a <see cref="System.Threading.Tasks.Task{T}"/></returns>
    [<Extension>]
    static member BindForm<'T>(ctx: HttpContext) =
        let binder = ctx.GetModelBinder()
        task {
            try
                let! form = ctx.Request.ReadFormAsync()
                return binder.Bind<'T> form
            with ex ->
                return raise <| ModelBindException("Unable to deserialize model from form", ex)
        }

    /// <summary>
    /// Parses all parameters of a request's query string into an object of type 'T.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <typeparam name="'T"></typeparam>
    /// <returns>Returns an instance of type 'T</returns>
    [<Extension>]
    static member BindQuery<'T>(ctx: HttpContext) =
        try
            let binder = ctx.GetModelBinder()
            binder.Bind<'T> ctx.Request.Query
        with ex ->
            raise <| ModelBindException("Unable to deserialize model from query", ex)
