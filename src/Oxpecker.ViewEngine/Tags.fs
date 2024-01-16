namespace Oxpecker.ViewEngine

[<AutoOpen>]
module Tags =

    open System.Text
    open Oxpecker.ViewEngine.Builder

    type html() = inherit HtmlElement("html")
    type div() = inherit HtmlElement("div")

    type br() =
        inherit HtmlElement("")
        override this.Render() = StringBuilder("<br>")