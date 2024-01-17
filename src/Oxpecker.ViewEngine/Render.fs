module Oxpecker.ViewEngine.Render

open System.Text

let toHtmlDocument (view: HtmlElement) =
    let sb = StringBuilder()
    sb.AppendLine("<!DOCTYPE html>") |> view.Render
    sb.ToString()

let toString (view: HtmlElement) =
    let sb = StringBuilder()
    view.Render sb
    sb.ToString()
