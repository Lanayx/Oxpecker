namespace Oxpecker

open System
open System.Globalization
open System.Runtime.CompilerServices
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Extensions
open Microsoft.Extensions.Logging
open Microsoft.Net.Http.Headers
open Oxpecker.ViewEngine

type MissingDependencyException(dependencyName: string) =
    inherit Exception $"Could not retrieve object of type '%s{dependencyName}' from ASP.NET Core's dependency container."

type RouteParseException (message: string, ex) =
    inherit Exception(message, ex)

type ModelBindException (message: string, ex) =
    inherit Exception(message, ex)

[<Extension>]
type HttpContextExtensions() =

    /// <summary>
    /// Returns the entire request URL in a fully escaped form, which is suitable for use in HTTP headers and other operations.
    /// </summary>
    /// <returns>Returns an instance of `'T`.</returns>
    [<Extension>]
    static member GetRequestUrl(ctx: HttpContext) =
        ctx.Request.GetEncodedUrl()

    /// <summary>
    /// Tries to get the value from the route values collection and cast it to `'T`.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The name of the HTTP header.</param>
    /// <returns>Returns a <see cref="System.String"/> URL.</returns>
    [<Extension>]
    static member TryGetRouteValue<'T>(ctx: HttpContext, key: string) =
        match ctx.Request.RouteValues.TryGetValue key with
        | true, value -> Some (value :?> 'T)
        | _           -> None

    /// <summary>
    /// Tries to get the <see cref="System.String"/> value of a HTTP header from the request.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The name of the HTTP header.</param>
    /// <returns> Returns Some string if the HTTP header was present in the request, otherwise returns None.</returns>
    [<Extension>]
    static member TryGetRequestHeader (ctx: HttpContext, key: string) =
        match ctx.Request.Headers.TryGetValue key with
        | true, value -> Some (string value)
        | _           -> None

    /// <summary>
    ///  Tries to get the <see cref="System.String"/> value of a query string parameter from the request.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The name of the query string parameter.</param>
    /// <returns>Returns Some string if the parameter was present in the request's query string, otherwise returns None.</returns>
    [<Extension>]
    static member TryGetQueryStringValue (ctx: HttpContext, key: string) =
        match ctx.Request.Query.TryGetValue key with
        | true, value -> Some (value.ToString())
        | _           -> None

    /// <summary>
    /// Gets an instance of `'T` from the request's service container.
    /// </summary>
    /// <returns>Returns an instance of `'T`.</returns>
    [<Extension>]
    static member GetService<'T>(ctx: HttpContext) =
        let t = typeof<'T>
        match ctx.RequestServices.GetService t with
        | null    -> raise <| MissingDependencyException t.Name
        | service -> service :?> 'T

    /// <summary>
    /// Gets an instance of <see cref="Microsoft.Extensions.Logging.ILogger{T}" /> from the request's service container.
    ///
    /// The type `'T` should represent the class or module from where the logger gets instantiated.
    /// </summary>
    /// <returns> Returns an instance of <see cref="Microsoft.Extensions.Logging.ILogger{T}" />.</returns>
    [<Extension>]
    static member GetLogger<'T>(ctx: HttpContext) =
        ctx.GetService<ILogger<'T>>()

    /// <summary>
    /// Gets an instance of <see cref="Microsoft.Extensions.Logging.ILogger" /> from the request's service container.    ///
    /// </summary>
    /// <returns> Returns an instance of <see cref="Microsoft.Extensions.Logging.ILogger" />.</returns>
    [<Extension>]
    static member GetLogger(ctx: HttpContext) =
        ctx.GetService<ILogger>()

    /// <summary>
    /// Gets an instance of <see cref="Microsoft.AspNetCore.Hosting.IWebHostEnvironment"/> from the request's service container.
    /// </summary>
    /// <returns>Returns an instance of <see cref="Microsoft.AspNetCore.Hosting.IWebHostEnvironment"/>.</returns>
    [<Extension>]
    static member GetHostingEnvironment(ctx: HttpContext) =
        ctx.GetService<IWebHostEnvironment>()

    /// <summary>
    /// Gets an instance of <see cref="Oxpecker.Json.ISerializer"/> from the request's service container.
    /// </summary>
    /// <returns>Returns an instance of <see cref="Oxpecker.Json.ISerializer"/>.</returns>
    [<Extension>]
    static member GetJsonSerializer(ctx: HttpContext): Json.ISerializer =
        ctx.GetService<Json.ISerializer>()

    /// <summary>
    /// Sets the HTTP status code of the response.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="httpStatusCode">The status code to be set in the response. For convenience you can use the static <see cref="Microsoft.AspNetCore.Http.StatusCodes"/> class for passing in named status codes instead of using pure int values.</param>
    [<Extension>]
    static member SetStatusCode (ctx: HttpContext, httpStatusCode: int) =
        ctx.Response.StatusCode <- httpStatusCode

    /// <summary>
    /// Adds or sets a HTTP header in the response.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The HTTP header name. For convenience you can use the static <see cref="Microsoft.Net.Http.Headers.HeaderNames"/> class for passing in strongly typed header names instead of using pure `string` values.</param>
    /// <param name="value">The value to be set. Non string values will be converted to a string using the object's ToString() method.</param>
    [<Extension>]
    static member SetHttpHeader (ctx: HttpContext, key: string, value: string) =
        ctx.Response.Headers[key] <- value

    /// <summary>
    /// Sets the Content-Type HTTP header in the response.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="contentType">The mime type of the response (e.g.: application/json or text/html).</param>
    [<Extension>]
    static member SetContentType (ctx: HttpContext, contentType: string) =
        ctx.SetHttpHeader(HeaderNames.ContentType, contentType)

    /// <summary>
    /// Writes a byte array to the body of the HTTP response and sets the HTTP Content-Length header accordingly.<br />
    /// <br />
    /// There are exceptions to be taken care of according to the RFC.<br />
    /// 1. Don't send Content-Length headers on 1xx and 204 responses and on 2xx responses to CONNECT requests (https://httpwg.org/specs/rfc7230.html#rfc.section.3.3.2)<br />
    /// 2. Don't send non-zero Content-Length headers for 205 responses (https://httpwg.org/specs/rfc7231.html#rfc.section.6.3.6)<br />
    /// <br />
    /// Since .NET 7 these rules are enforced by Kestrel (https://github.com/dotnet/aspnetcore/pull/43103)
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="bytes">The byte array to be send back to the client.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteBytes(ctx: HttpContext, bytes: byte[]) =
        let canIncludeContentLengthHeader =
            match ctx.Response.StatusCode, ctx.Request.Method with
            | statusCode, _ when statusCode |> is1xxStatusCode || statusCode = 204 -> false
            | statusCode, method when method = HttpMethods.Connect && statusCode |> is2xxStatusCode -> false
            | _ -> true
        let is205StatusCode = ctx.Response.StatusCode = 205
        if canIncludeContentLengthHeader then
            let contentLength = if is205StatusCode then 0 else bytes.Length
            ctx.SetHttpHeader(HeaderNames.ContentLength, string contentLength)
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
    static member WriteText (ctx: HttpContext, str: string) =
        ctx.SetContentType "text/plain; charset=utf-8"
        ctx.WriteBytes(Encoding.UTF8.GetBytes str)

    /// <summary>
    /// Writes an UTF-8 encoded string to the body of the HTTP response and sets the HTTP `Content-Length` header accordingly, as well as the `Content-Type` header to `text/plain`.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="html">The string html value to be send back to the client.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteHtmlString (ctx: HttpContext, html: string) =
        ctx.SetContentType "text/html; charset=utf-8"
        ctx.WriteBytes(Encoding.UTF8.GetBytes html)

    /// <summary>
    /// Serializes an object to JSON and writes the output to the body of the HTTP response.
    /// It also sets the HTTP Content-Type header to application/json and sets the Content-Length header accordingly.
    /// The JSON serializer can be configured in the ASP.NET Core startup code by registering a custom class of type <see cref="Json.ISerializer"/>
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="value">The object to be send back to the client.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteJson<'T> (ctx: HttpContext, value: 'T) =
        let serializer = ctx.GetJsonSerializer()
        serializer.Serialize(value, ctx, false)

    /// <summary>
    /// Serializes an object to JSON and writes the output to the body of the HTTP response using chunked transfer encoding.
    /// It also sets the HTTP Content-Type header to application/json and sets the Transfer-Encoding header to chunked.
    /// The JSON serializer can be configured in the ASP.NET Core startup code by registering a custom class of type <see cref="Json.ISerializer"/>.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="value">The object to be send back to the client.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteJsonChunked<'T> (ctx: HttpContext, value: 'T) =
        let serializer = ctx.GetJsonSerializer()
        serializer.Serialize(value, ctx, true)


    /// <summary>
    /// <para>Compiles a `Oxpecker.OxpeckerViewEngine.Builder.HtmlElement` object to a HTML view and writes the output to the body of the HTTP response.</para>
    /// <para>It also sets the HTTP header `Content-Type` to `text/html` and sets the `Content-Length` header accordingly.</para>
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="htmlView">An `HtmlElement` object to be send back to the client and which represents a valid HTML view.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteHtmlView (ctx: HttpContext, htmlView: HtmlElement) =
        let bytes = Render.toHtmlDocBytes htmlView
        ctx.Response.ContentType <- "text/html; charset=utf-8"
        ctx.Response.ContentLength <- bytes.LongLength
        ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length)

    /// <summary>
    /// Uses the <see cref="Json.ISerializer"/> to deserialize the entire body of the <see cref="Microsoft.AspNetCore.Http.HttpRequest"/> asynchronously into an object of type 'T.
    /// </summary>
    /// <typeparam name="'T"></typeparam>
    /// <returns>Retruns a <see cref="System.Threading.Tasks.Task{T}"/></returns>
    [<Extension>]
    static member BindJson<'T>(ctx: HttpContext) =
        let serializer = ctx.GetJsonSerializer()
        task {
            try
                return! serializer.Deserialize<'T>(ctx)
            with ex ->
                return raise <| ModelBindException("Unable to deserialize model", ex)
        }

    /// <summary>
    /// Parses all input elements from an HTML form into an object of type 'T.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="cultureInfo">An optional <see cref="System.Globalization.CultureInfo"/> element to be used when parsing culture specific data such as float, DateTime or decimal values.</param>
    /// <typeparam name="'T"></typeparam>
    /// <returns>Returns a <see cref="System.Threading.Tasks.Task{T}"/></returns>
    [<Extension>]
    static member BindForm<'T> (ctx: HttpContext, ?cultureInfo: CultureInfo) =
        task {
            let! form = ctx.Request.ReadFormAsync()
            return
                form
                |> Seq.map (fun i -> i.Key, i.Value)
                |> dict
                |> ModelParser.parse<'T> cultureInfo false
                |> function
                    | Ok value -> value
                    | Error msg -> raise <| ModelBindException($"Unexpected error during non-strict model parsing: {msg}", null)
        }

    /// <summary>
    /// Parses all parameters of a request's query string into an object of type 'T.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="cultureInfo">An optional <see cref="System.Globalization.CultureInfo"/> element to be used when parsing culture specific data such as float, DateTime or decimal values.</param>
    /// <typeparam name="'T"></typeparam>
    /// <returns>Returns an instance of type 'T</returns>
    [<Extension>]
    static member BindQuery<'T> (ctx: HttpContext, ?cultureInfo: CultureInfo) =
        ctx.Request.Query
        |> Seq.map (fun i -> i.Key, i.Value)
        |> dict
        |> ModelParser.parse<'T> cultureInfo false
        |> function
            | Ok objData -> objData
            | Error msg -> raise <| ModelBindException($"Unexpected error during non-strict model parsing: {msg}", null)
