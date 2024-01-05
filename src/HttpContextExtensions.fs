namespace Oxpecker

open System
open System.Runtime.CompilerServices
open System.Text
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open Microsoft.Net.Http.Headers

[<AutoOpen>]
module Helpers =
    open System
    open System.IO

    /// <summary>
    /// Checks if an object is not null.
    /// </summary>
    /// <param name="x">The object to validate against `null`.</param>
    /// <returns>Returns true if the object is not null otherwise false.</returns>
    let inline isNotNull x = not (isNull x)

    /// <summary>
    /// Converts a string into a string option where null or an empty string will be converted to None and everything else to Some string.
    /// </summary>
    /// <param name="str">The string value to be converted into an option of string.</param>
    /// <returns>Returns None if the string was null or empty otherwise Some string.</returns>
    let inline strOption (str : string) =
        if String.IsNullOrEmpty str then None else Some str

    /// <summary>
    /// Reads a file asynchronously from the file system.
    /// </summary>
    /// <param name="filePath">The absolute path of the file.</param>
    /// <returns>Returns the string contents of the file wrapped in a Task.</returns>
    let readFileAsStringAsync (filePath : string) =
        task {
            use reader = new StreamReader(filePath)
            return! reader.ReadToEndAsync()
        }

    /// <summary>
    /// Utility function for matching 1xx HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>Returns true if the status code is between 100 and 199.</returns>
    let is1xxStatusCode (statusCode : int) =
        100 <= statusCode && statusCode <= 199

    /// <summary>
    /// Utility function for matching 2xx HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>Returns true if the status code is between 200 and 299.</returns>
    let is2xxStatusCode (statusCode : int) =
        200 <= statusCode && statusCode <= 299

    /// <summary>
    /// Utility function for matching 3xx HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>Returns true if the status code is between 300 and 399.</returns>
    let is3xxStatusCode (statusCode : int) =
        300 <= statusCode && statusCode <= 399

    /// <summary>
    /// Utility function for matching 4xx HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>Returns true if the status code is between 400 and 499.</returns>
    let is4xxStatusCode (statusCode : int) =
        400 <= statusCode && statusCode <= 499

    /// <summary>
    /// Utility function for matching 5xx HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>Returns true if the status code is between 500 and 599.</returns>
    let is5xxStatusCode (statusCode : int) =
        500 <= statusCode && statusCode <= 599

type MissingDependencyException(dependencyName : string) =
    inherit Exception $"Could not retrieve object of type '%s{dependencyName}' from ASP.NET Core's dependency container."

[<Extension>]
type HttpContextExtensions() =

    /// <summary>
    /// Gets an instance of `'T` from the request's service container.
    /// </summary>
    /// <returns>Returns an instance of `'T`.</returns>
    [<Extension>]
    static member GetService<'T>(ctx : HttpContext) =
        let t = typeof<'T>
        match ctx.RequestServices.GetService t with
        | null    -> raise <| MissingDependencyException t.Name
        | service -> service :?> 'T

    /// <summary>
    /// Sets the HTTP status code of the response.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="httpStatusCode">The status code to be set in the response. For convenience you can use the static <see cref="Microsoft.AspNetCore.Http.StatusCodes"/> class for passing in named status codes instead of using pure int values.</param>
    [<Extension>]
    static member SetStatusCode (ctx : HttpContext, httpStatusCode : int) =
        ctx.Response.StatusCode <- httpStatusCode

    /// <summary>
    /// Adds or sets a HTTP header in the response.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="key">The HTTP header name. For convenience you can use the static <see cref="Microsoft.Net.Http.Headers.HeaderNames"/> class for passing in strongly typed header names instead of using pure `string` values.</param>
    /// <param name="value">The value to be set. Non string values will be converted to a string using the object's ToString() method.</param>
    [<Extension>]
    static member SetHttpHeader (ctx : HttpContext, key : string, value : obj) =
        ctx.Response.Headers.[key] <- StringValues(value.ToString())

    /// <summary>
    /// Sets the Content-Type HTTP header in the response.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="contentType">The mime type of the response (e.g.: application/json or text/html).</param>
    [<Extension>]
    static member SetContentType (ctx : HttpContext, contentType : string) =
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
    /// <returns>Task of Some HttpContext after writing to the body of the response.</returns>
    [<Extension>]
    static member WriteBytesAsync (ctx : HttpContext, bytes : byte[]) =
        task {
            let canIncludeContentLengthHeader =
                match ctx.Response.StatusCode, ctx.Request.Method with
                | statusCode, _ when statusCode |> is1xxStatusCode || statusCode = 204 -> false
                | statusCode, method when method = "CONNECT" && statusCode |> is2xxStatusCode -> false
                | _ -> true
            let is205StatusCode = ctx.Response.StatusCode = 205
            if canIncludeContentLengthHeader then
                let contentLength = if is205StatusCode then 0 else bytes.Length
                ctx.SetHttpHeader(HeaderNames.ContentLength, contentLength)
            if ctx.Request.Method <> HttpMethods.Head then
                do! ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length)
            return Some ctx
        }

    /// <summary>
    /// Writes an UTF-8 encoded string to the body of the HTTP response and sets the HTTP Content-Length header accordingly.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="str">The string value to be send back to the client.</param>
    /// <returns>Task of Some HttpContext after writing to the body of the response.</returns>
    [<Extension>]
    static member WriteStringAsync (ctx : HttpContext, str : string) =
        ctx.WriteBytesAsync(Encoding.UTF8.GetBytes str)

    /// <summary>
    /// Writes an UTF-8 encoded string to the body of the HTTP response and sets the HTTP `Content-Length` header accordingly, as well as the `Content-Type` header to `text/plain`.
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="str">The string value to be send back to the client.</param>
    /// <returns>Task of Some HttpContext after writing to the body of the response.</returns>
    [<Extension>]
    static member WriteTextAsync (ctx : HttpContext, str : string) =
        ctx.SetContentType "text/plain; charset=utf-8"
        ctx.WriteStringAsync str

