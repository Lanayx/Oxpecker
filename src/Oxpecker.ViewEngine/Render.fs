namespace Oxpecker.ViewEngine

open System
open System.IO
open System.Text
open System.Threading
open Oxpecker.ViewEngine.Tools

/// <summary>
/// Renders an `HtmlElement` to a string, to UTF-8 bytes, or asynchronously to a stream or a text writer.
/// </summary>
[<AbstractClass; Sealed>]
type Render =

    static member private copyStringBuilderToBytes(sb: StringBuilder) : byte[] =
        let mutable total = 0
        for chunk in sb.GetChunks() do
            total <- total + Encoding.UTF8.GetByteCount(chunk.Span)
        let bytes = GC.AllocateUninitializedArray<byte>(total)
        let mutable written = 0
        for chunk in sb.GetChunks() do
            written <- written + Encoding.UTF8.GetBytes(chunk.Span, bytes.AsSpan(written))
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
            use _ = streamWriter :> IAsyncDisposable
            try
                view.Render sb
                do! streamWriter.WriteAsync(sb, cancellationToken)
                return! streamWriter.FlushAsync(cancellationToken)
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
            use _ = streamWriter :> IAsyncDisposable
            try
                view.Render sb
                do! streamWriter.WriteAsync(sb, cancellationToken)
                return! streamWriter.FlushAsync(cancellationToken)
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
