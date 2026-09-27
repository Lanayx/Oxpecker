namespace Oxpecker.ViewEngine

open Oxpecker.ViewEngine.Tools

/// <summary>
/// Renders an `HtmlElement` to an HTML string in JavaScript.
/// </summary>
[<AbstractClass; Sealed>]
type Render =
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
