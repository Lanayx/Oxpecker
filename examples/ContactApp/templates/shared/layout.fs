namespace ContactApp.templates.shared

open System
open Microsoft.AspNetCore.Http
open ContactApp.Tools

module layout =
    open Oxpecker.ViewEngine
    open Oxpecker.Htmx

    let html (content: HtmlElement) (ctx: HttpContext)  =
        let flashMessage = getFlashedMessage ctx

        html(lang="") {
            head() {
                title() { "Contact App" }
                script(src="https://cdn.jsdelivr.net/npm/htmx.org@4.0.0-beta2",
                    crossorigin="anonymous")
                link(rel="stylesheet", href="/site.css")
            }
            body(style = "width: 800px; margin: 0 auto").hxInherited("hx-boost", "true") {
                main() {
                    header() {
                        h1() {
                            span(style="text-transform:uppercase;") { "contacts.app" }
                        }
                        h2() { "A Demo Contacts Application" }
                        if String.IsNullOrEmpty flashMessage |> not then
                            div(class'="alert fadeOut") { flashMessage }
                    }
                    hr()
                    content
                }
            }
        }
