module ContactApp.templates.layout
open Oxpecker.ViewEngine
open Oxpecker.Htmx


let html (content: HtmlElement) =
    html(lang="") {
        head() {
            title() { "Contact App" }
            script(src="https://unpkg.com/htmx.org@1.9.10",
                crossorigin="anonymous")
        }
        body(style = "width: 800px; margin: 0 auto",
             hxBoost=true) {
            main() {
                header() {
                    h1() {
                        span(style="text-transform:uppercase;") { "contacts.app" }
                    }
                    h2() { "A Demo Contacts Application" }
                }
                hr()
                content
            }
        }
    }
