module Oxpecker.ViewEngine.Render

open System.Buffers
open System.IO
open System.Text

let inline private toStringWriter (view: HtmlElement) =
    let sw = new StringWriter()
    view.Render sw
    sw

let inline private toHtmlDocStringWriter (view: HtmlElement) =
    let sw = new StringWriter()
    sw.WriteLine("<!DOCTYPE html>")
    view.Render sw
    sw

let inline private copyStringBuilderToBytes(sb: StringBuilder) =
    let chArray = ArrayPool<char>.Shared.Rent(sb.Length)
    sb.CopyTo(0, chArray, 0, sb.Length)
    let bytes = Encoding.UTF8.GetBytes(chArray, 0, sb.Length)
    ArrayPool<char>.Shared.Return(chArray)
    bytes

let toString (view: HtmlElement) =
    let sb = toStringWriter view
    sb.ToString()

let toBytes (view: HtmlElement) =
    use sw = toStringWriter view
    let sb = sw.GetStringBuilder()
    copyStringBuilderToBytes sb

let toHtmlDocBytes (view: HtmlElement) =
    use sw = toHtmlDocStringWriter view
    let sb = sw.GetStringBuilder()
    copyStringBuilderToBytes sb

