module Oxpecker.ViewEngine.Render

open System.Buffers
open System.Globalization
open System.IO
open System.Text

let inline private toStringWriter (view: HtmlElement) =
    let sw = new StringWriter(Tools.stringBuilderPool.Get(), CultureInfo.InvariantCulture)
    view.Render sw
    sw

let inline private toHtmlDocStringWriter (view: HtmlElement) =
    let sw = new StringWriter(Tools.stringBuilderPool.Get(), CultureInfo.InvariantCulture)
    sw.WriteLine("<!DOCTYPE html>")
    view.Render sw
    sw

let inline private copyStringBuilderToBytes (sb: StringBuilder) =
    let chArray = ArrayPool<char>.Shared.Rent(sb.Length)
    sb.CopyTo(0, chArray, 0, sb.Length)
    let bytes = Encoding.UTF8.GetBytes(chArray, 0, sb.Length)
    ArrayPool<char>.Shared.Return(chArray)
    bytes

let toString (view: HtmlElement) =
    let sb = toStringWriter view
    let result = sb.ToString()
    Tools.stringBuilderPool.Return(sb.GetStringBuilder())
    result

let toBytes (view: HtmlElement) =
    use sw = toStringWriter view
    let sb = sw.GetStringBuilder()
    let result = copyStringBuilderToBytes sb
    Tools.stringBuilderPool.Return(sb)
    result

let toHtmlDocBytes (view: HtmlElement) =
    use sw = toHtmlDocStringWriter view
    let sb = sw.GetStringBuilder()
    let result = copyStringBuilderToBytes sb
    Tools.stringBuilderPool.Return(sb)
    result
