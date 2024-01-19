module Oxpecker.ViewEngine.Render

open System.Buffers
open System.Text

let inline private toStringBuilder (view: HtmlElement) =
    let sb = StringBuilder()
    view.Render sb
    sb

let inline private toHtmlDocStringBuilder (view: HtmlElement) =
    let sb = StringBuilder()
    sb.AppendLine("<!DOCTYPE html>") |> view.Render
    sb

let toString (view: HtmlElement) =
    let sb = toStringBuilder view
    sb.ToString()

let toBytes (view: HtmlElement) =
    let sb = toStringBuilder view
    let chArray = ArrayPool<char>.Shared.Rent(sb.Length)
    sb.CopyTo(0, chArray, 0, sb.Length)
    let bytes = Encoding.UTF8.GetBytes(chArray, 0, sb.Length)
    ArrayPool<char>.Shared.Return(chArray)
    bytes

let toHtmlDocBytes (view: HtmlElement) =
    let sb = toHtmlDocStringBuilder view
    let chArray = ArrayPool<char>.Shared.Rent(sb.Length)
    sb.CopyTo(0, chArray, 0, sb.Length)
    let bytes = Encoding.UTF8.GetBytes(chArray, 0, sb.Length)
    ArrayPool<char>.Shared.Return(chArray)
    bytes