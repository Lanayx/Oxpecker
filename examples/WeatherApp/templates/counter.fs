module WeatherApp.templates.counter

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria

let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Counter"

    div().attr("x-data", "{ currentCount: 0 }") {
        h1() { "Counter" }
        p(role="status").attr("x-text", "'CurrentCount: ' + currentCount")
        button(class'="btn btn-primary").attr("x-on:click", "currentCount++") { "Click me" }
    }
