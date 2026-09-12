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
/// <para>A single part of a multipart response, compatible with the htmx 4 `hx-multipart` extension.</para>
/// <para>The built-in implementations <see cref="HtmlPart"/>, <see cref="TextPart"/>, <see cref="JsonPart{T}"/> and <see cref="BytesPart"/>
/// are created directly or through the <see cref="MultipartPart"/> factory members; any other content can be sent by implementing this interface.</para>
/// </summary>
type IMultipartPart =
    /// <summary>
    /// <para>Writes the part to the response writer: the header lines, each ending with a line break, an empty line and then the body.
    /// The delimiter lines around the part are written by the framework, which also flushes the writer after each part.</para>
    /// <para>The built-in parts write a `Content-Type` header line followed by their additional headers. A part without headers starts with
    /// the empty line right away; note that the htmx `hx-multipart` extension expects at least one header line per part.</para>
    /// <para>Text has to be encoded as UTF-8, e.g. with `Encoding.UTF8.GetBytes(text.AsSpan(), writer)`; raw bytes can be copied with
    /// `writer.Write` and a stream with `stream.CopyToAsync writer`.</para>
    /// </summary>
    /// <param name="writer">The response writer to write the part to.</param>
    /// <returns>Task of writing the part.</returns>
    abstract member WriteAsync: writer: PipeWriter -> Task

/// <summary>
/// Subtype of the multipart response, i.e. the `multipart/{subtype}` media type.
/// </summary>
[<RequireQualifiedAccess>]
type MultipartSubtype =
    /// `multipart/mixed`: the htmx `hx-multipart` extension finishes swapping a part before it reads the next one.
    | Mixed
    /// `multipart/parallel`: the htmx `hx-multipart` extension starts swapping a part without waiting for the previous one.
    | Parallel

module internal MultipartHeaders =

    /// Encodes the content of the builder as UTF-8 into the writer chunk by chunk; the stateful encoder
    /// keeps a surrogate pair intact even when it spans two chunks.
    let writeUtf8 (writer: PipeWriter) (sb: StringBuilder) =
        let encoder = Encoding.UTF8.GetEncoder()
        let mutable bytesUsed = 0L
        let mutable completed = false
        for chunk in sb.GetChunks() do
            encoder.Convert(chunk.Span, writer, false, &bytesUsed, &completed)
        encoder.Convert(ReadOnlySpan<char>.Empty, writer, true, &bytesUsed, &completed)

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
                "Multipart part headers must not contain 'Content-Type', pass the content type separately instead."
            )
        validateHeaderLine name value

    let private validateContentType (contentType: string) =
        if String.IsNullOrEmpty contentType then
            raise <| ArgumentException("Multipart part Content-Type must not be empty.")
        validateHeaderLine "Content-Type" contentType

    /// Writes the validated header block: the `Content-Type` header, the additional headers in the given order
    /// and the empty line ending the headers.
    let write (writer: PipeWriter) (contentType: string) (headers: (string * string) seq) =
        let sb = StringBuilderPool.Get()
        try
            validateContentType contentType
            sb.Append("Content-Type: ").Append(contentType).Append("\r\n") |> ignore
            for name, value in headers do
                validateHeader name value
                sb.Append(name).Append(": ").Append(value).Append("\r\n") |> ignore
            sb.Append("\r\n") |> ignore
            writeUtf8 writer sb
        finally
            StringBuilderPool.Return sb

/// <summary>
/// `text/html; charset=utf-8` part rendered from an `HtmlElement` with the Oxpecker view engine (without a DOCTYPE prefix) and written as UTF-8 text.
/// </summary>
/// <param name="view">The HTML element to render as the part body.</param>
/// <param name="headers">Optional additional part headers as name/value pairs, written after `Content-Type` in the given order.</param>
type HtmlPart(view: HtmlElement, [<Struct>] ?headers: (string * string) seq) =

    let headers = defaultValueArg headers Seq.empty

    interface IMultipartPart with
        member this.WriteAsync writer =
            MultipartHeaders.write writer "text/html; charset=utf-8" headers
            let sb = StringBuilderPool.Get()
            try
                view.Render sb
                MultipartHeaders.writeUtf8 writer sb
            finally
                StringBuilderPool.Return sb
            Task.CompletedTask

/// <summary>
/// `text/plain; charset=utf-8` part written as UTF-8 text.
/// </summary>
/// <param name="text">The text to write as the part body.</param>
/// <param name="headers">Optional additional part headers as name/value pairs, written after `Content-Type` in the given order.</param>
type TextPart(text: string, [<Struct>] ?headers: (string * string) seq) =
    let headers = defaultValueArg headers Seq.empty

    interface IMultipartPart with
        member this.WriteAsync writer =
            MultipartHeaders.write writer "text/plain; charset=utf-8" headers
            Encoding.UTF8.GetBytes(text.AsSpan(), writer) |> ignore
            Task.CompletedTask

/// <summary>
/// `application/json; charset=utf-8` part serialized with System.Text.Json and written as UTF-8 JSON.
/// </summary>
/// <param name="value">The value to serialize as the part body, its static type determines the serialization contract.</param>
/// <param name="options">Optional serializer options, `JsonSerializerOptions.Web` by default.</param>
/// <param name="headers">Optional additional part headers as name/value pairs, written after `Content-Type` in the given order.</param>
type JsonPart<'T>(value: 'T, [<Struct>] ?options: JsonSerializerOptions, [<Struct>] ?headers: (string * string) seq) =

    let headers = defaultValueArg headers Seq.empty

    interface IMultipartPart with
        member this.WriteAsync writer =
            MultipartHeaders.write writer "application/json; charset=utf-8" headers
            JsonSerializer.SerializeAsync<'T>(writer, value, defaultValueArg options JsonSerializerOptions.Web)

/// <summary>
/// Part with the given content type written as raw bytes, without any re-encoding.
/// </summary>
/// <param name="contentType">Value of the part `Content-Type` header.</param>
/// <param name="data">The bytes to write as the part body.</param>
/// <param name="headers">Optional additional part headers as name/value pairs, written after `Content-Type` in the given order.</param>
type BytesPart(contentType: string, data: byte array, [<Struct>] ?headers: (string * string) seq) =

    interface IMultipartPart with
        member this.WriteAsync writer =
            MultipartHeaders.write writer contentType (defaultValueArg headers Seq.empty)
            writer.Write(ReadOnlySpan data)
            Task.CompletedTask

/// <summary>
/// Factory members creating the built-in <see cref="IMultipartPart"/> implementations.
/// </summary>
[<AbstractClass; Sealed>]
type MultipartPart =

    /// <summary>
    /// Creates a `text/html; charset=utf-8` part from an `HtmlElement`, see <see cref="HtmlPart"/>.
    /// </summary>
    /// <param name="view">The HTML element to render as the part body.</param>
    /// <param name="headers">Optional additional part headers.</param>
    static member Html(view: #HtmlElement, [<Struct>] ?headers: (string * string) seq) : IMultipartPart =
        HtmlPart(view, ?headers = headers)

    /// <summary>
    /// Creates a `text/plain; charset=utf-8` part from a string, see <see cref="TextPart"/>.
    /// </summary>
    /// <param name="text">The text to write as the part body.</param>
    /// <param name="headers">Optional additional part headers.</param>
    static member Text(text: string, [<Struct>] ?headers: (string * string) seq) : IMultipartPart =
        TextPart(text, ?headers = headers)

    /// <summary>
    /// Creates an `application/json; charset=utf-8` part by serializing a value with System.Text.Json, see <see cref="JsonPart{T}"/>.
    /// </summary>
    /// <param name="value">The value to serialize as the part body.</param>
    /// <param name="options">Optional serializer options, `JsonSerializerOptions.Web` by default.</param>
    /// <param name="headers">Optional additional part headers.</param>
    static member Json<'T>
        (value: 'T, [<Struct>] ?options: JsonSerializerOptions, [<Struct>] ?headers: (string * string) seq)
        : IMultipartPart =
        JsonPart<'T>(value, ?options = options, ?headers = headers)

    /// <summary>
    /// Creates a part with the given content type from raw bytes, see <see cref="BytesPart"/>.
    /// </summary>
    /// <param name="contentType">Value of the part `Content-Type` header.</param>
    /// <param name="data">The bytes to write as the part body.</param>
    /// <param name="headers">Optional additional part headers.</param>
    static member Bytes
        (contentType: string, data: byte array, [<Struct>] ?headers: (string * string) seq)
        : IMultipartPart =
        BytesPart(contentType, data, ?headers = headers)

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

    let raiseEmpty () : 'a =
        raise
        <| ArgumentException("Multipart response must contain at least one part.", "parts")

    /// Writes the opening delimiter `--{boundary}` (without a trailing line break).
    let writeOpening (writer: PipeWriter) (delimiter: byte array) =
        writer.Write(ReadOnlySpan(delimiter, 2, delimiter.Length - 2))

    /// Writes one part: the line break ending the previous delimiter line, the part itself (its headers, an empty line
    /// and the body) and the next delimiter `\r\n--{boundary}` (without a trailing line break).
    let writePartAsync (writer: PipeWriter) (delimiter: byte array) (part: IMultipartPart) =
        writer.Write(ReadOnlySpan "\r\n"B)
        task {
            do! part.WriteAsync writer
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
    static member WriteMultipart(ctx: HttpContext, parts: IMultipartPart seq, [<Struct>] ?subtype: MultipartSubtype) =
        let subtype = defaultValueArg subtype MultipartSubtype.Mixed
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
                // completing the writer returns its pooled buffers, also when validation or a part throws
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
        (ctx: HttpContext, parts: #IAsyncEnumerable<IMultipartPart>, [<Struct>] ?subtype: MultipartSubtype)
        =
        let subtype = defaultValueArg subtype MultipartSubtype.Mixed
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
let multipart (parts: IMultipartPart seq) : EndpointHandler =
    fun (ctx: HttpContext) -> ctx.WriteMultipart(parts)

/// <summary>
/// Writes a stream of parts as a `multipart/mixed` response compatible with the htmx 4 `hx-multipart` extension, using chunked transfer encoding.
/// Each part is flushed to the client as soon as it has been produced. At least one part is required, an `ArgumentException` is thrown otherwise
/// (except for `HEAD` requests, for which the parts are not enumerated).
/// </summary>
/// <param name="parts">The stream of parts to be sent back to the client.</param>
/// <param name="ctx">HttpContext</param>
/// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
let multipartChunked (parts: #IAsyncEnumerable<IMultipartPart>) : EndpointHandler =
    fun (ctx: HttpContext) -> ctx.WriteMultipartChunked(parts)
