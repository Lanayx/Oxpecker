module Oxpecker.ViewEngine.Render

open System.Text
open Microsoft.AspNetCore.Http

let toString (view: HtmlElement) =
    let sb = StringBuilder()
    view.Render sb
    sb.ToString()

let toResponseStream (ctx: HttpContext) (view: HtmlElement) =
    let sb = StringBuilder()
    sb.AppendLine("<!DOCTYPE html>") |> view.Render
    let str = sb.ToString()
    let bytes = Encoding.UTF8.GetBytes(str)
    ctx.Response.ContentType <- "text/html; charset=utf-8"
    ctx.Response.ContentLength <- bytes.LongLength
    ctx.Response.Body.WriteAsync(bytes).AsTask()
