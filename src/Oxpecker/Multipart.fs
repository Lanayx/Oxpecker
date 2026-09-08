[<AutoOpen>]
module Oxpecker.Multipart

open System
open System.Buffers
open System.Collections.Generic
open System.IO
open System.Runtime.CompilerServices
open System.Text
open System.Text.Json
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.WebUtilities
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Tools

// ---------------------------
// Multipart response model
// ---------------------------

/// <summary>
/// Body of a single part of a multipart response.
/// </summary>
[<RequireQualifiedAccess>]
type MultipartBody =
    /// An `HtmlElement` rendered with the Oxpecker view engine (without a DOCTYPE prefix).
    | Html of HtmlElement
    /// A string written as UTF-8 text.
    | Text of string
    /// A value serialized with System.Text.Json. When no options are given, `JsonSerializerOptions.Web` is used.
    | Json of value: objnull * options: JsonSerializerOptions option
    /// Raw bytes written as is.
    | Bytes of byte array

/// <summary>
/// Subtype of the multipart response, i.e. the `multipart/{subtype}` media type.
/// </summary>
[<RequireQualifiedAccess>]
type MultipartSubtype =
    /// `multipart/mixed`: the htmx `hx-multipart` extension finishes swapping a part before it reads the next one.
    | Mixed
    /// `multipart/parallel`: the htmx `hx-multipart` extension starts swapping a part without waiting for the previous one.
    | Parallel

/// <summary>
/// A single part of a multipart response, compatible with the htmx 4 `hx-multipart` extension.
/// Use the static factory members (`Html`, `Text`, `Json`, `Bytes`) or the constructor, then adjust
/// <see cref="ContentType"/> and <see cref="Headers"/> as needed.
/// </summary>
/// <param name="contentType">Value of the part `Content-Type` header, e.g. `text/html; charset=utf-8`.</param>
/// <param name="body">Body of the part.</param>
type MultipartPart(contentType: string, body: MultipartBody) =
    let headers = Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)

    /// <summary>
    /// Value of the part `Content-Type` header.
    /// </summary>
    member val ContentType = contentType with get, set

    /// <summary>
    /// Body of the part.
    /// </summary>
    member this.Body = body

    /// <summary>
    /// Additional part headers, written after `Content-Type` in insertion order, e.g. `HX-Target` (or `HX-Retarget`),
    /// `HX-Swap` (or `HX-Reswap`), `HX-Trigger`, `HX-Part-ID` or `Content-ID`. Header names must be valid HTTP tokens
    /// and header values must not contain control characters such as line breaks.
    /// </summary>
    member this.Headers = headers

    static member private Create(contentType: string, body: MultipartBody, headers: (string * string) seq option) =
        let part = MultipartPart(contentType, body)
        match headers with
        | Some headers ->
            for name, value in headers do
                part.Headers[name] <- value
        | None -> ()
        part

    /// <summary>
    /// Creates a `text/html; charset=utf-8` part from an `HtmlElement`.
    /// </summary>
    /// <param name="view">The HTML element to render as the part body.</param>
    /// <param name="headers">Optional additional part headers.</param>
    static member Html(view: #HtmlElement, ?headers: (string * string) seq) =
        MultipartPart.Create("text/html; charset=utf-8", MultipartBody.Html(view :> HtmlElement), headers)

    /// <summary>
    /// Creates a `text/plain; charset=utf-8` part from a string.
    /// </summary>
    /// <param name="text">The text to write as the part body.</param>
    /// <param name="headers">Optional additional part headers.</param>
    static member Text(text: string, ?headers: (string * string) seq) =
        MultipartPart.Create("text/plain; charset=utf-8", MultipartBody.Text text, headers)

    /// <summary>
    /// Creates an `application/json; charset=utf-8` part by serializing a value with System.Text.Json.
    /// </summary>
    /// <param name="value">The value to serialize as the part body.</param>
    /// <param name="options">Optional serializer options, `JsonSerializerOptions.Web` by default.</param>
    /// <param name="headers">Optional additional part headers.</param>
    static member Json<'T>(value: 'T, ?options: JsonSerializerOptions, ?headers: (string * string) seq) =
        MultipartPart.Create("application/json; charset=utf-8", MultipartBody.Json(box value, options), headers)

    /// <summary>
    /// Creates a part with the given content type from raw bytes.
    /// </summary>
    /// <param name="contentType">Value of the part `Content-Type` header.</param>
    /// <param name="data">The bytes to write as the part body.</param>
    /// <param name="headers">Optional additional part headers.</param>
    static member Bytes(contentType: string, data: byte array, ?headers: (string * string) seq) =
        MultipartPart.Create(contentType, MultipartBody.Bytes data, headers)

// ---------------------------
// Wire format
// ---------------------------

module internal MultipartWriter =

    let createBoundary () =
        "multipart-" + Guid.NewGuid().ToString("N")

    let contentType (subtype: MultipartSubtype) (boundary: string) =
        match subtype with
        | MultipartSubtype.Mixed -> $"multipart/mixed; boundary={boundary}"
        | MultipartSubtype.Parallel -> $"multipart/parallel; boundary={boundary}"

    /// Characters allowed in an HTTP token (RFC 9110 section 5.6.2), i.e. in a header name.
    let private tokenChars =
        SearchValues.Create("!#$%&'*+-.^_`|~0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".AsSpan())

    /// ASCII control characters other than the horizontal tab; not allowed in a header value.
    let private controlChars =
        let chars = [|
            for c in 0..127 do
                if (c < 32 && c <> 9) || c = 127 then
                    char c
        |]
        SearchValues.Create(ReadOnlySpan chars)

    let private validateHeaderValue (name: string) (value: string) =
        if value.AsSpan().ContainsAny controlChars then
            raise
            <| ArgumentException($"Multipart header '{name}' value must not contain control characters.")

    let private validateHeader (name: string) (value: string) =
        if String.IsNullOrEmpty name || name.AsSpan().ContainsAnyExcept tokenChars then
            raise
            <| ArgumentException($"Invalid multipart header name '{name}', header names must be valid HTTP tokens.")
        validateHeaderValue name value

    let private validateContentType (contentType: string) =
        if String.IsNullOrEmpty contentType then
            raise <| ArgumentException("Multipart part Content-Type must not be empty.")
        validateHeaderValue "Content-Type" contentType

    let raiseEmpty () : 'a =
        raise
        <| ArgumentException("Multipart response must contain at least one part.", "parts")

    /// Writes the opening delimiter `--{boundary}` (without a trailing line break).
    let writeOpeningAsync (writer: TextWriter) (boundary: string) =
        task {
            do! writer.WriteAsync "--"
            do! writer.WriteAsync boundary
        }

    /// Writes one part: a line break ending the previous delimiter, the part headers, a blank line,
    /// the body and the next delimiter `\r\n--{boundary}` (without a trailing line break).
    let writePartAsync (writer: TextWriter) (stream: Stream) (boundary: string) (part: MultipartPart) =
        let sb = StringBuilderPool.Get()
        task {
            try
                validateContentType part.ContentType
                sb.Append("\r\nContent-Type: ").Append(part.ContentType).Append("\r\n")
                |> ignore
                for KeyValue(name, value) in part.Headers do
                    validateHeader name value
                    sb.Append(name).Append(": ").Append(value).Append("\r\n") |> ignore
                sb.Append("\r\n") |> ignore
                match part.Body with
                | MultipartBody.Html view ->
                    view.Render sb
                    do! writer.WriteAsync sb
                | MultipartBody.Text text ->
                    sb.Append(text) |> ignore
                    do! writer.WriteAsync sb
                | MultipartBody.Json(value, options) ->
                    do! writer.WriteAsync sb
                    do! writer.FlushAsync()
                    do! JsonSerializer.SerializeAsync(stream, value, defaultArg options JsonSerializerOptions.Web)
                | MultipartBody.Bytes data ->
                    do! writer.WriteAsync sb
                    do! writer.FlushAsync()
                    do! stream.WriteAsync(data, 0, data.Length)
                do! writer.WriteAsync "\r\n--"
                do! writer.WriteAsync boundary
            finally
                StringBuilderPool.Return sb
        }

    /// Writes the `--` and line break that turn the last delimiter into the closing delimiter `--{boundary}--`.
    let writeClosingAsync (writer: TextWriter) = writer.WriteAsync "--\r\n"

// ---------------------------
// HttpContext extensions
// ---------------------------

type MultipartExtensions() =

    /// <summary>
    /// <para>Writes the given parts as a `multipart/mixed` (or `multipart/parallel`) response compatible with the htmx 4 `hx-multipart` extension.</para>
    /// <para>The whole response is rendered in memory first, so the `Content-Length` header is set accordingly.
    /// To stream parts as they become available use <see cref="WriteMultipartChunked"/> instead.</para>
    /// <para>At least one part is required, an `ArgumentException` is thrown otherwise.</para>
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="parts">The parts to be sent back to the client.</param>
    /// <param name="subtype">The multipart subtype, `MultipartSubtype.Mixed` by default.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteMultipart(ctx: HttpContext, parts: MultipartPart seq, ?subtype: MultipartSubtype) =
        let subtype = defaultArg subtype MultipartSubtype.Mixed
        let boundary = MultipartWriter.createBoundary()
        let memoryStream = recyclableMemoryStreamManager.Value.GetStream()
        task {
            try
                use writer = new StreamWriter(memoryStream, leaveOpen = true)
                do! MultipartWriter.writeOpeningAsync writer boundary
                let mutable isEmpty = true
                for part in parts do
                    isEmpty <- false
                    do! MultipartWriter.writePartAsync writer memoryStream boundary part
                if isEmpty then
                    MultipartWriter.raiseEmpty()
                do! MultipartWriter.writeClosingAsync writer
                do! writer.FlushAsync()
                ctx.Response.ContentType <- MultipartWriter.contentType subtype boundary
                ctx.Response.ContentLength <- memoryStream.Length
                if ctx.Request.Method <> HttpMethods.Head then
                    memoryStream.Seek(0, SeekOrigin.Begin) |> ignore
                    do! memoryStream.CopyToAsync ctx.Response.Body
            finally
                memoryStream.Dispose()
        }

    /// <summary>
    /// <para>Writes a stream of parts as a `multipart/mixed` (or `multipart/parallel`) response compatible with the htmx 4 `hx-multipart` extension, using chunked transfer encoding.</para>
    /// <para>Each part is flushed to the client as soon as it has been produced, so the client can process it while the next part is still being generated.</para>
    /// <para>At least one part is required: if the stream completes without producing any, an `ArgumentException` is thrown before anything is written to the response.</para>
    /// </summary>
    /// <param name="ctx">The current http context object.</param>
    /// <param name="parts">The stream of parts to be sent back to the client.</param>
    /// <param name="subtype">The multipart subtype, `MultipartSubtype.Mixed` by default.</param>
    /// <returns>Task of writing to the body of the response.</returns>
    [<Extension>]
    static member WriteMultipartChunked
        (ctx: HttpContext, parts: #IAsyncEnumerable<MultipartPart>, ?subtype: MultipartSubtype)
        =
        let subtype = defaultArg subtype MultipartSubtype.Mixed
        let boundary = MultipartWriter.createBoundary()
        if ctx.Request.Method <> HttpMethods.Head then
            task {
                let enumerator = parts.GetAsyncEnumerator(ctx.RequestAborted)
                use _ = enumerator :> IAsyncDisposable
                let! hasParts = enumerator.MoveNextAsync()
                if not hasParts then
                    MultipartWriter.raiseEmpty()
                ctx.Response.ContentType <- MultipartWriter.contentType subtype boundary
                let writer = new HttpResponseStreamWriter(ctx.Response.Body, Encoding.UTF8)
                use _ = writer :> IAsyncDisposable
                do! MultipartWriter.writeOpeningAsync writer boundary
                let mutable hasNext = hasParts
                while hasNext do
                    do! MultipartWriter.writePartAsync writer ctx.Response.Body boundary enumerator.Current
                    do! writer.FlushAsync()
                    let! next = enumerator.MoveNextAsync()
                    hasNext <- next
                do! MultipartWriter.writeClosingAsync writer
            }
            :> Task
        else
            ctx.Response.ContentType <- MultipartWriter.contentType subtype boundary
            Task.CompletedTask

// ---------------------------
// HttpHandler functions
// ---------------------------

/// <summary>
/// Writes the given parts as a `multipart/mixed` response compatible with the htmx 4 `hx-multipart` extension.
/// The whole response is rendered in memory first, so the `Content-Length` header is set accordingly.
/// At least one part is required, an `ArgumentException` is thrown otherwise.
/// </summary>
/// <param name="parts">The parts to be sent back to the client.</param>
/// <param name="ctx">HttpContext</param>
/// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
let multipart (parts: MultipartPart seq) : EndpointHandler =
    fun (ctx: HttpContext) -> ctx.WriteMultipart(parts)

/// <summary>
/// Writes a stream of parts as a `multipart/mixed` response compatible with the htmx 4 `hx-multipart` extension, using chunked transfer encoding.
/// Each part is flushed to the client as soon as it has been produced. At least one part is required, an `ArgumentException` is thrown otherwise.
/// </summary>
/// <param name="parts">The stream of parts to be sent back to the client.</param>
/// <param name="ctx">HttpContext</param>
/// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
let multipartChunked (parts: #IAsyncEnumerable<MultipartPart>) : EndpointHandler =
    fun (ctx: HttpContext) -> ctx.WriteMultipartChunked(parts)
