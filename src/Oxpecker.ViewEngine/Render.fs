module Oxpecker.ViewEngine.Render

open System
open System.Buffers
open System.IO
open System.Text
open Oxpecker.ViewEngine.Tools

let inline private copyStringBuilderToBytes (sb: StringBuilder) =
    let chArray = ArrayPool<char>.Shared.Rent(sb.Length)
    sb.CopyTo(0, chArray, 0, sb.Length)
    let bytes = Encoding.UTF8.GetBytes(chArray, 0, sb.Length)
    ArrayPool<char>.Shared.Return(chArray)
    bytes

/// Render HtmlElement to normal UTF16 string
let toString (view: #HtmlElement) =
    let sb = StringBuilderPool.Get()
    try
        view.Render sb
        sb.ToString()
    finally
        StringBuilderPool.Return(sb)

/// Render HtmlElement to normal UTF16 string with DOCTYPE prefix
let toHtmlDocString (view: #HtmlElement) =
    let sb = StringBuilderPool.Get()
    sb.AppendLine("<!DOCTYPE html>") |> ignore
    try
        view.Render sb
        sb.ToString()
    finally
        StringBuilderPool.Return(sb)

/// Render HTMLElement to UTF8 encoded bytes
let toBytes (view: #HtmlElement) =
    let sb = StringBuilderPool.Get()
    try
        view.Render sb
        copyStringBuilderToBytes sb
    finally
        StringBuilderPool.Return(sb)

/// Render HTMLElement to UTF8 encoded bytes with DOCTYPE prefix
let toHtmlDocBytes (view: #HtmlElement) =
    let sb = StringBuilderPool.Get()
    sb.AppendLine("<!DOCTYPE html>") |> ignore
    try
        view.Render sb
        copyStringBuilderToBytes sb
    finally
        StringBuilderPool.Return(sb)

/// Render HTMLElement to stream using UTF8 stream writer
let toStreamAsync (stream: Stream) (view: #HtmlElement) =
    let sb = StringBuilderPool.Get()
    let streamWriter = new StreamWriter(stream, leaveOpen = true)
    task {
        use _ = streamWriter :> IAsyncDisposable
        try
            view.Render sb
            do! streamWriter.WriteAsync(sb)
            return! streamWriter.FlushAsync()
        finally
            StringBuilderPool.Return(sb)
    }

/// Render HTMLElement to stream using UTF8 stream writer
let toHtmlDocStreamAsync (stream: Stream) (view: #HtmlElement) =
    let sb = StringBuilderPool.Get()
    sb.AppendLine("<!DOCTYPE html>") |> ignore
    let streamWriter = new StreamWriter(stream, leaveOpen = true)
    task {
        use _ = streamWriter :> IAsyncDisposable
        try
            view.Render sb
            do! streamWriter.WriteAsync(sb)
            return! streamWriter.FlushAsync()
        finally
            StringBuilderPool.Return(sb)
    }

/// Render HTMLElement to stream using UTF8 stream writer
let toTextWriterAsync (textWriter: TextWriter) (view: #HtmlElement) =
    let sb = StringBuilderPool.Get()
    task {
        try
            view.Render sb
            do! textWriter.WriteAsync(sb)
            return! textWriter.FlushAsync()
        finally
            StringBuilderPool.Return(sb)
    }

/// Render HTMLElement to stream using UTF8 stream writer
let toHtmlDocTextWriterAsync (textWriter: TextWriter) (view: #HtmlElement) =
    let sb = StringBuilderPool.Get()
    sb.AppendLine("<!DOCTYPE html>") |> ignore
    task {
        try
            view.Render sb
            do! textWriter.WriteAsync(sb)
            return! textWriter.FlushAsync()
        finally
            StringBuilderPool.Return(sb)
    }
