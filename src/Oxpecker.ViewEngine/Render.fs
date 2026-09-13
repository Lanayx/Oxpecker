namespace Oxpecker.ViewEngine

open System
open System.Buffers
open System.IO
open System.Text
open System.Threading
open Oxpecker.ViewEngine.Tools

/// <summary>
/// Renders an `HtmlElement` to a string, to UTF-8 bytes, to an `IBufferWriter<byte>` such as a `PipeWriter`,
/// or asynchronously to a stream or a text writer.
/// </summary>
[<AbstractClass; Sealed>]
type Render =

    /// Copies the content of the builder as UTF-8 into an array of the exact size; like `writeUtf8`, a surrogate pair
    /// split between two chunks is encoded as one scalar instead of two replacement characters.
    static member private copyStringBuilderToBytes(sb: StringBuilder) : byte[] =
        let mutable total = 0
        let mutable endsWithHigh = false
        for chunk in sb.GetChunks() do
            let span = chunk.Span
            if endsWithHigh && span.Length > 0 && Char.IsLowSurrogate(span[0]) then
                // counted as two 3-byte replacement characters, encoded as one 4-byte scalar
                total <- total - 2
            total <- total + Encoding.UTF8.GetByteCount(span)
            endsWithHigh <- span.Length > 0 && Char.IsHighSurrogate(span[span.Length - 1])
        let bytes = GC.AllocateUninitializedArray<byte>(total)
        let mutable written = 0
        let mutable pendingHigh = ReadOnlyMemory<char>.Empty
        for chunk in sb.GetChunks() do
            let mutable span = chunk.Span
            if not pendingHigh.IsEmpty then
                if span.Length > 0 && Char.IsLowSurrogate(span[0]) then
                    written <- written + Rune(pendingHigh.Span[0], span[0]).EncodeToUtf8(bytes.AsSpan(written))
                    span <- span.Slice(1)
                else
                    written <- written + Encoding.UTF8.GetBytes(pendingHigh.Span, bytes.AsSpan(written))
                pendingHigh <- ReadOnlyMemory<char>.Empty
            if span.Length > 0 && Char.IsHighSurrogate(span[span.Length - 1]) then
                pendingHigh <- chunk.Slice(chunk.Length - 1)
                span <- span.Slice(0, span.Length - 1)
            written <- written + Encoding.UTF8.GetBytes(span, bytes.AsSpan(written))
        if not pendingHigh.IsEmpty then
            written <- written + Encoding.UTF8.GetBytes(pendingHigh.Span, bytes.AsSpan(written))
        bytes

    /// Render HtmlElement to normal UTF16 string
    static member toString(view: #HtmlElement) =
        let sb = StringBuilderPool.Get()
        try
            view.Render sb
            sb.ToString()
        finally
            StringBuilderPool.Return(sb)

    /// Render HtmlElement to normal UTF16 string with DOCTYPE prefix
    static member toHtmlDocString(view: #HtmlElement) =
        let sb = StringBuilderPool.Get()
        sb.AppendLine("<!DOCTYPE html>") |> ignore
        try
            view.Render sb
            sb.ToString()
        finally
            StringBuilderPool.Return(sb)

    /// Render HTMLElement to UTF8 encoded bytes
    static member toBytes(view: #HtmlElement) =
        let sb = StringBuilderPool.Get()
        try
            view.Render sb
            Render.copyStringBuilderToBytes sb
        finally
            StringBuilderPool.Return(sb)

    /// Render HTMLElement to UTF8 encoded bytes with DOCTYPE prefix
    static member toHtmlDocBytes(view: #HtmlElement) =
        let sb = StringBuilderPool.Get()
        sb.AppendLine("<!DOCTYPE html>") |> ignore
        try
            view.Render sb
            Render.copyStringBuilderToBytes sb
        finally
            StringBuilderPool.Return(sb)

    /// <summary>
    /// Render HTMLElement as UTF8 encoded bytes into a buffer writer, e.g. a `PipeWriter` such as `HttpResponse.BodyWriter`
    /// </summary>
    /// <param name="writer">The buffer writer to encode into; it is not flushed.</param>
    /// <param name="view">The element to render.</param>
    static member toBufferWriter(writer: IBufferWriter<byte>, view: #HtmlElement) =
        let sb = StringBuilderPool.Get()
        try
            view.Render sb
            writeUtf8 writer sb
        finally
            StringBuilderPool.Return(sb)

    /// <summary>
    /// Render HTMLElement as UTF8 encoded bytes with DOCTYPE prefix into a buffer writer, e.g. a `PipeWriter` such as `HttpResponse.BodyWriter`
    /// </summary>
    /// <param name="writer">The buffer writer to encode into; it is not flushed.</param>
    /// <param name="view">The element to render.</param>
    static member toHtmlDocBufferWriter(writer: IBufferWriter<byte>, view: #HtmlElement) =
        let sb = StringBuilderPool.Get()
        sb.AppendLine("<!DOCTYPE html>") |> ignore
        try
            view.Render sb
            writeUtf8 writer sb
        finally
            StringBuilderPool.Return(sb)

    /// <summary>
    /// Render HTMLElement to stream using UTF8 stream writer
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="view">The element to render.</param>
    /// <param name="cancellationToken">Optional token passed to the writes and the flush, e.g. `HttpContext.RequestAborted`.</param>
    static member toStreamAsync(stream: Stream, view: #HtmlElement, [<Struct>] ?cancellationToken: CancellationToken) =
        let cancellationToken = defaultValueArg cancellationToken CancellationToken.None
        let sb = StringBuilderPool.Get()
        let streamWriter = new StreamWriter(stream, leaveOpen = true)
        task {
            try
                view.Render sb
                do! streamWriter.WriteAsync(sb, cancellationToken)
                do! streamWriter.FlushAsync(cancellationToken)
                // disposed only once the flush has observed the token: disposing after a cancellation
                // would flush the characters still buffered without it
                do! streamWriter.DisposeAsync()
            finally
                StringBuilderPool.Return(sb)
        }

    /// <summary>
    /// Render HTMLElement to stream using UTF8 stream writer with DOCTYPE prefix
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="view">The element to render.</param>
    /// <param name="cancellationToken">Optional token passed to the writes and the flush, e.g. `HttpContext.RequestAborted`.</param>
    static member toHtmlDocStreamAsync
        (stream: Stream, view: #HtmlElement, [<Struct>] ?cancellationToken: CancellationToken)
        =
        let cancellationToken = defaultValueArg cancellationToken CancellationToken.None
        let sb = StringBuilderPool.Get()
        sb.AppendLine("<!DOCTYPE html>") |> ignore
        let streamWriter = new StreamWriter(stream, leaveOpen = true)
        task {
            try
                view.Render sb
                do! streamWriter.WriteAsync(sb, cancellationToken)
                do! streamWriter.FlushAsync(cancellationToken)
                // disposed only once the flush has observed the token: disposing after a cancellation
                // would flush the characters still buffered without it
                do! streamWriter.DisposeAsync()
            finally
                StringBuilderPool.Return(sb)
        }

    /// <summary>
    /// Render HTMLElement to the provided text writer
    /// </summary>
    /// <param name="textWriter">The text writer to write to.</param>
    /// <param name="view">The element to render.</param>
    /// <param name="cancellationToken">Optional token passed to the writes and the flush, e.g. `HttpContext.RequestAborted`.</param>
    static member toTextWriterAsync
        (textWriter: TextWriter, view: #HtmlElement, [<Struct>] ?cancellationToken: CancellationToken)
        =
        let cancellationToken = defaultValueArg cancellationToken CancellationToken.None
        let sb = StringBuilderPool.Get()
        task {
            try
                view.Render sb
                do! textWriter.WriteAsync(sb, cancellationToken)
                return! textWriter.FlushAsync(cancellationToken)
            finally
                StringBuilderPool.Return(sb)
        }

    /// <summary>
    /// Render HTMLElement to the provided text writer with DOCTYPE prefix
    /// </summary>
    /// <param name="textWriter">The text writer to write to.</param>
    /// <param name="view">The element to render.</param>
    /// <param name="cancellationToken">Optional token passed to the writes and the flush, e.g. `HttpContext.RequestAborted`.</param>
    static member toHtmlDocTextWriterAsync
        (textWriter: TextWriter, view: #HtmlElement, [<Struct>] ?cancellationToken: CancellationToken)
        =
        let cancellationToken = defaultValueArg cancellationToken CancellationToken.None
        let sb = StringBuilderPool.Get()
        sb.AppendLine("<!DOCTYPE html>") |> ignore
        task {
            try
                view.Render sb
                do! textWriter.WriteAsync(sb, cancellationToken)
                return! textWriter.FlushAsync(cancellationToken)
            finally
                StringBuilderPool.Return(sb)
        }
