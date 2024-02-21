module Oxpecker.ViewEngine.Render

open System.Buffers
open System.Text
open Oxpecker.ViewEngine.Tools

let inline private toStringBuilder (view: HtmlElement) =
    let sb = StringBuilderPool.Get()
    view.Render sb
    sb

let inline private toHtmlDocStringBuilder (view: HtmlElement) =
    let sb = StringBuilderPool.Get()
    sb.AppendLine("<!DOCTYPE html>") |> ignore
    view.Render sb
    sb

let inline private copyStringBuilderToBytes (sb: StringBuilder) =
    let chArray = ArrayPool<char>.Shared.Rent(sb.Length)
    sb.CopyTo(0, chArray, 0, sb.Length)
    let bytes = Encoding.UTF8.GetBytes(chArray, 0, sb.Length)
    ArrayPool<char>.Shared.Return(chArray)
    bytes

/// Render HtmlElement to normal UTF16 string
let toString (view: HtmlElement) =
    let sb = toStringBuilder view
    let result = sb.ToString()
    StringBuilderPool.Return(sb)
    result

/// Render HTMLElement to UTF8 encoded bytes
let toBytes (view: HtmlElement) =
    let sb = toStringBuilder view
    let result = copyStringBuilderToBytes sb
    StringBuilderPool.Return(sb)
    result

/// Render HTMLElement to UTF8 encoded bytes with DOCTYPE prefix
let toHtmlDocBytes (view: HtmlElement) =
    let sb = toHtmlDocStringBuilder view
    let result = copyStringBuilderToBytes sb
    StringBuilderPool.Return(sb)
    result
