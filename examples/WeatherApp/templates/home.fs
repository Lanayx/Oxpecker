module WeatherApp.templates.home

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine

let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Home"

    Fragment(){
        h1() { "Hello, world!" }
        "Welcome to your new app."
    }

