open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Oxpecker
open Oxpecker.ViewEngine
open Oxpecker.Datastar

let page () =
    html(lang = "en") {
        head() {
            meta(charset = "utf-8")
            meta(name = "viewport", content = "width=device-width, initial-scale=1")
            title() { "Oxpecker + Rocket" }
            link(rel = "stylesheet", href = "/site.css")
            script(type' = "importmap") {
                raw
                    """{"imports":{"datastar-rocket":"https://cdn.jsdelivr.net/gh/starfederation/datastar@v1.0.4/bundles/datastar-rocket.js"}}"""
            }
            script(type' = "module", src = "/dist/Program.js") { }
        }
        body() {
            main() {
                h1() { "Oxpecker + Rocket" }
                p() { "The same F# view syntax, on the server and in the browser." }
                section() {
                    h2() { "Browser: Rocket counters" }
                    p() { "Each component owns its count. Changing one does not affect the other or call the server." }
                    RegularNode("ox-counter").attr("start", "0") { }
                    RegularNode("ox-counter").attr("start", "3") { }
                }
                section() {
                    h2() { "Server: Datastar HTML patch" }
                    p() { "Fetch a fragment from Oxpecker without reloading the page. Your counters keep their state." }
                    button(type' = "button").dataOn("click", "@get('/hello')") { "Say hello from the server" }
                    div(id = "greeting").attr("aria-live", "polite") { "Waiting for the server..." }
                }
            }
        }
    }

let greeting (ctx: HttpContext) =
    let timestamp = DateTimeOffset.UtcNow.ToString("HH:mm:ss.fff")
    let fragment =
        div(id = "greeting").attr("aria-live", "polite") {
            p() { "Hello from Oxpecker!" }
            p() { $"Server time: {timestamp} UTC" }
        }
    // Datastar patches the existing element with the matching ID.
    // Render a fragment, without the document doctype added by htmlView.
    ctx.Response.ContentType <- "text/html; charset=utf-8"
    ctx.Response.Headers.CacheControl <- "no-store"
    ctx.Response.WriteAsync(Render.toString fragment, ctx.RequestAborted)

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.Services.AddRouting().AddOxpecker() |> ignore
    let app = builder.Build()
    app
        .UseStaticFiles()
        .UseRouting()
        .UseOxpecker(
            [
                GET [ route "/" (fun ctx -> htmlView (page()) ctx); route "/hello" greeting ]
            ]
        )
    |> ignore
    app.Run()
    0
