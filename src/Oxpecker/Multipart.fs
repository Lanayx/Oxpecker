[<AutoOpen>]
module Oxpecker.Multipart

open System
open System.Buffers
open System.Collections.Generic
open System.IO
open System.IO.Pipelines
open System.Runtime.CompilerServices
open System.Text
open System.Text.Json
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Tools

// ---------------------------
// Multipart response model
// ---------------------------

/// <summary>
/// <para>Body of a single part of a multipart response, written after the part headers.</para>
/// <para>The built-in implementations are <see cref="HtmlBody"/>, <see cref="TextBody"/>, <see cref="JsonBody"/> and <see cref="BytesBody"/>;
/// other content can be sent by implementing this interface and passing the body to <see cref="MultipartPart.Create"/>.</para>
/// </summary>
type MultipartBody =
    /// <summary>
    /// <para>Writes the body as bytes to the response writer. Text has to be encoded as UTF-8, e.g. with
    /// `Encoding.UTF8.GetBytes(text.AsSpan(), writer)`; raw bytes can be copied with `writer.Write` and a stream with `stream.CopyToAsync writer`.</para>
    /// <para>The writer is flushed after each part, so the body does not need to flush it.</para>
    /// </summary>
    /// <param name="writer">The response writer to write the body to.</param>
    /// <returns>Task of writing the body.</returns>
    abstract member WriteAsync: writer: PipeWriter -> Task

module internal Utf8 =

    /// Encodes the content of the builder as UTF-8 into the writer chunk by chunk; the stateful encoder
    /// keeps a surrogate pair intact even when it spans two chunks.
    let write (writer: PipeWriter) (sb: StringBuilder) =
        let encoder = Encoding.UTF8.GetEncoder()
        let mutable bytesUsed = 0L
        let mutable completed = false
        for chunk in sb.GetChunks() do
            encoder.Convert(chunk.Span, writer, false, &bytesUsed, &completed)
        encoder.Convert(ReadOnlySpan<char>.Empty, writer, true, &bytesUsed, &completed)

/// <summary>
/// Part body rendered from an `HtmlElement` with the Oxpecker view engine (without a DOCTYPE prefix) and written as UTF-8 text.
/// </summary>
/// <param name="view">The HTML element to render.</param>
type HtmlBody(view: HtmlElement) =

    member this.View = view

    member this.WriteAsync(writer: PipeWriter) : Task =
        let sb = StringBuilderPool.Get()
        try
            view.Render sb
            Utf8.write writer sb
        finally
            StringBuilderPool.Return sb
        Task.CompletedTask

    interface MultipartBody with
        member this.WriteAsync writer = this.WriteAsync writer

/// <summary>
/// Part body written as UTF-8 text.
/// </summary>
/// <param name="text">The text to write.</param>
type TextBody(text: string) =

    member this.Text = text

    member this.WriteAsync(writer: PipeWriter) : Task =
        Encoding.UTF8.GetBytes(text.AsSpan(), writer) |> ignore
        Task.CompletedTask

    interface MultipartBody with
        member this.WriteAsync writer = this.WriteAsync writer

/// <summary>
/// Part body serialized with System.Text.Json and written as UTF-8 JSON.
/// </summary>
/// <param name="value">The value to serialize, its runtime type determines the serialization contract.</param>
/// <param name="options">Optional serializer options, `JsonSerializerOptions.Web` by default.</param>
type JsonBody(value: objnull, ?options: JsonSerializerOptions) =

    member this.Value = value

    member this.Options = options

    member this.WriteAsync(writer: PipeWriter) : Task =
        JsonSerializer.SerializeAsync(writer, value, defaultArg options JsonSerializerOptions.Web)

    interface MultipartBody with
        member this.WriteAsync writer = this.WriteAsync writer

/// <summary>
/// Part body written as raw bytes, without any re-encoding.
/// </summary>
/// <param name="data">The bytes to write.</param>
type BytesBody(data: byte array) =

    member this.Data = data

    member this.WriteAsync(writer: PipeWriter) : Task =
        writer.Write(ReadOnlySpan data)
        Task.CompletedTask

    interface MultipartBody with
        member this.WriteAsync writer = this.WriteAsync writer

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
/// Use the static factory members (`Html`, `Text`, `Json`, `Bytes`, or `Create` for a custom body), then adjust
/// <see cref="ContentType"/> and <see cref="Headers"/> as needed.
/// </summary>
/// <param name="contentType">Value of the part `Content-Type` header, e.g. `text/html; charset=utf-8`.</param>
/// <param name="body">Body of the part: a <see cref="HtmlBody"/>, <see cref="TextBody"/>, <see cref="JsonBody"/> or <see cref="BytesBody"/>, or a custom <see cref="MultipartBody"/> implementation.</param>
type MultipartPart internal (contentType: string, body: MultipartBody, headers: (string * string) seq) =

    /// <summary>
    /// Value of the part `Content-Type` header.
    /// </summary>
    member val ContentType = contentType with get, set

    /// <summary>
    /// Body of the part.
    /// </summary>
    member this.Body = body

    /// <summary>
    /// Additional part headers as name/value pairs, written after `Content-Type` in the given order, e.g. `HX-Target` (or `HX-Retarget`),
    /// `HX-Swap` (or `HX-Reswap`), `HX-Trigger`, `HX-Part-ID` or `Content-ID`. The collection is stored as is, without copying,
    /// and validated when the part is written: header names must be valid HTTP tokens, header values must not contain
    /// control characters such as line breaks, and a header line must not exceed 998 bytes.
    /// `Content-Type` is not allowed here, set <see cref="ContentType"/> instead.
    /// </summary>
    member val Headers = headers with get, set

    /// <summary>
    /// Creates a part with the given content type and body, e.g. a custom <see cref="MultipartBody"/> implementation.
    /// </summary>
    /// <param name="contentType">Value of the part `Content-Type` header.</param>
    /// <param name="body">Body of the part.</param>
    /// <param name="headers">Optional additional part headers, see <see cref="Headers"/>.</param>
    static member Create(contentType: string, body: MultipartBody, ?headers: (string * string) seq) =
        MultipartPart(contentType, body, defaultArg headers Seq.empty)

    /// <summary>
    /// Creates a `text/html; charset=utf-8` part from an `HtmlElement`.
    /// </summary>
    /// <param name="view">The HTML element to render as the part body.</param>
    /// <param name="headers">Optional additional part headers.</param>
    static member Html(view: #HtmlElement, ?headers: (string * string) seq) =
        MultipartPart.Create("text/html; charset=utf-8", HtmlBody(view :> HtmlElement), ?headers = headers)

    /// <summary>
    /// Creates a `text/plain; charset=utf-8` part from a string.
    /// </summary>
    /// <param name="text">The text to write as the part body.</param>
    /// <param name="headers">Optional additional part headers.</param>
    static member Text(text: string, ?headers: (string * string) seq) =
        MultipartPart.Create("text/plain; charset=utf-8", TextBody text, ?headers = headers)

    /// <summary>
    /// Creates an `application/json; charset=utf-8` part by serializing a value with System.Text.Json.
    /// </summary>
    /// <param name="value">The value to serialize as the part body.</param>
    /// <param name="options">Optional serializer options, `JsonSerializerOptions.Web` by default.</param>
    /// <param name="headers">Optional additional part headers.</param>
    static member Json<'T>(value: 'T, ?options: JsonSerializerOptions, ?headers: (string * string) seq) =
        MultipartPart.Create(
            "application/json; charset=utf-8",
            JsonBody(box value, ?options = options),
            ?headers = headers
        )

    /// <summary>
    /// Creates a part with the given content type from raw bytes.
    /// </summary>
    /// <param name="contentType">Value of the part `Content-Type` header.</param>
    /// <param name="data">The bytes to write as the part body.</param>
    /// <param name="headers">Optional additional part headers.</param>
    static member Bytes(contentType: string, data: byte array, ?headers: (string * string) seq) =
        MultipartPart.Create(contentType, BytesBody data, ?headers = headers)

// ---------------------------
// Wire format
// ---------------------------

module internal MultipartWriter =

    let createBoundary () =
        "multipart-" + Guid.NewGuid().ToString("N")

    let contentType (subtype: MultipartSubtype) (boundary: string) =
        match subtype with
        | MultipartSubtype.Mixed -> $"multipart/mixed; boundary=%s{boundary}"
        | MultipartSubtype.Parallel -> $"multipart/parallel; boundary=%s{boundary}"

    /// The delimiter `\r\n--{boundary}` as bytes (the boundary consists of ASCII characters only).
    let delimiter (boundary: string) =
        Encoding.ASCII.GetBytes("\r\n--" + boundary)

    /// Characters allowed in an HTTP token (RFC 9110 section 5.6.2), i.e. in a header name.
    let private tokenChars =
        SearchValues.Create("!#$%&'*+-.^_`|~0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".AsSpan())

    /// Control characters (C0 and C1 controls as well as DEL) other than the horizontal tab; not allowed in a header value.
    let private controlChars =
        let chars = [|
            for c in Char.MinValue .. Char.MaxValue do
                if Char.IsControl c && c <> '\t' then
                    c
        |]
        SearchValues.Create(ReadOnlySpan chars)

    /// Maximum length of a MIME header line in bytes, excluding the line break (RFC 5322 section 2.1.1).
    [<Literal>]
    let private MaxHeaderLineLength = 998

    /// Validates the header value and the length of the `{name}: {value}` line.
    let private validateHeaderLine (name: string) (value: string) =
        // tuples do not prevent null values, e.g. from C# callers or Unchecked.defaultof
        if obj.ReferenceEquals(value, null) then
            raise
            <| ArgumentException($"Multipart header '%s{name}' value must not be null.")
        if value.AsSpan().ContainsAny controlChars then
            raise
            <| ArgumentException($"Multipart header '%s{name}' value must not contain control characters.")
        if Encoding.UTF8.GetByteCount name + 2 + Encoding.UTF8.GetByteCount value > MaxHeaderLineLength then
            raise
            <| ArgumentException($"Multipart header '%s{name}' line must not exceed {MaxHeaderLineLength} bytes.")

    let private validateHeader (name: string) (value: string) =
        if String.IsNullOrEmpty name || name.AsSpan().ContainsAnyExcept tokenChars then
            raise
            <| ArgumentException($"Invalid multipart header name '%s{name}', header names must be valid HTTP tokens.")
        if name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) then
            raise
            <| ArgumentException(
                "Multipart part headers must not contain 'Content-Type', set MultipartPart.ContentType instead."
            )
        validateHeaderLine name value

    let private validateContentType (contentType: string) =
        if String.IsNullOrEmpty contentType then
            raise <| ArgumentException("Multipart part Content-Type must not be empty.")
        validateHeaderLine "Content-Type" contentType

    let raiseEmpty () : 'a =
        raise
        <| ArgumentException("Multipart response must contain at least one part.", "parts")

    /// Writes the opening delimiter `--{boundary}` (without a trailing line break).
    let writeOpening (writer: PipeWriter) (delimiter: byte array) =
        writer.Write(ReadOnlySpan(delimiter, 2, delimiter.Length - 2))

    /// Writes one part: a line break ending the previous delimiter, the part headers, a blank line,
    /// the body and the next delimiter `\r\n--{boundary}` (without a trailing line break).
    let writePartAsync (writer: PipeWriter) (delimiter: byte array) (part: MultipartPart) =
        let sb = StringBuilderPool.Get()
        try
            validateContentType part.ContentType
            sb.Append("\r\nContent-Type: ").Append(part.ContentType).Append("\r\n")
            |> ignore
            for name, value in part.Headers do
                validateHeader name value
                sb.Append(name).Append(": ").Append(value).Append("\r\n") |> ignore
            sb.Append("\r\n") |> ignore
            Utf8.write writer sb
        finally
            StringBuilderPool.Return sb
        task {
            do! part.Body.WriteAsync writer
            writer.Write(ReadOnlySpan delimiter)
        }

    /// Writes the `--` and line break that turn the last delimiter into the closing delimiter `--{boundary}--`.
    let writeClosing (writer: PipeWriter) = writer.Write(ReadOnlySpan "--\r\n"B)

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
        let delimiter = MultipartWriter.delimiter boundary
        let memoryStream = recyclableMemoryStreamManager.Value.GetStream()
        let writer =
            PipeWriter.Create(memoryStream, StreamPipeWriterOptions(leaveOpen = true))
        task {
            try
                MultipartWriter.writeOpening writer delimiter
                let mutable isEmpty = true
                for part in parts do
                    isEmpty <- false
                    do! MultipartWriter.writePartAsync writer delimiter part
                if isEmpty then
                    MultipartWriter.raiseEmpty()
                MultipartWriter.writeClosing writer
                let! _ = writer.FlushAsync()
                ctx.Response.ContentType <- MultipartWriter.contentType subtype boundary
                ctx.Response.ContentLength <- memoryStream.Length
                if ctx.Request.Method <> HttpMethods.Head then
                    memoryStream.Seek(0, SeekOrigin.Begin) |> ignore
                    do! memoryStream.CopyToAsync(ctx.Response.Body)
            finally
                // completing the writer returns its pooled buffers, also when validation or a body throws
                writer.Complete()
                memoryStream.Dispose()
        }

    /// <summary>
    /// <para>Writes a stream of parts as a `multipart/mixed` (or `multipart/parallel`) response compatible with the htmx 4 `hx-multipart` extension, using chunked transfer encoding.</para>
    /// <para>Each part is written to `HttpResponse.BodyWriter` and flushed as soon as it has been produced, so the client can process it while the next part is still being generated.</para>
    /// <para>At least one part is required: if the stream completes without producing any, an `ArgumentException` is thrown before anything is written to the response.
    /// For `HEAD` requests only the `Content-Type` header is set and the parts are not enumerated, so this check does not apply.</para>
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
            let delimiter = MultipartWriter.delimiter boundary
            task {
                let enumerator = parts.GetAsyncEnumerator()
                use _ = enumerator :> IAsyncDisposable
                let! hasParts = enumerator.MoveNextAsync()
                if not hasParts then
                    MultipartWriter.raiseEmpty()
                ctx.Response.ContentType <- MultipartWriter.contentType subtype boundary
                let writer = ctx.Response.BodyWriter
                MultipartWriter.writeOpening writer delimiter
                let mutable hasNext = hasParts
                while hasNext do
                    do! MultipartWriter.writePartAsync writer delimiter enumerator.Current
                    let! _ = writer.FlushAsync()
                    let! next = enumerator.MoveNextAsync()
                    hasNext <- next
                MultipartWriter.writeClosing writer
                let! _ = writer.FlushAsync()
                ()
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
/// Each part is flushed to the client as soon as it has been produced. At least one part is required, an `ArgumentException` is thrown otherwise
/// (except for `HEAD` requests, for which the parts are not enumerated).
/// </summary>
/// <param name="parts">The stream of parts to be sent back to the client.</param>
/// <param name="ctx">HttpContext</param>
/// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
let multipartChunked (parts: #IAsyncEnumerable<MultipartPart>) : EndpointHandler =
    fun (ctx: HttpContext) -> ctx.WriteMultipartChunked(parts)
