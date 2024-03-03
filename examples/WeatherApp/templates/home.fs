module WeatherApp.templates.home

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open WeatherApp.templates.shared

let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Home"

    __(){
        h1() { "Hello, world!" }
        "Welcome to your new app."
    }
    |> layout.html ctx

