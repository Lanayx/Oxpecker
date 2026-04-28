module WeatherApp.templates.counter

open Microsoft.AspNetCore.Http
open Oxpecker.Alpine
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria

let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Counter"

    div().xData("{ currentCount: 0 }") {
        h1() { "Counter" }
        p(role="status").xText("'CurrentCount: ' + currentCount")
        button(class'="btn btn-primary").xOn("click", "currentCount++") { "Click me" }
    }
