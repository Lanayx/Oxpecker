module ContactApp.templates.layout
open Oxpecker.ViewEngine


let html (content: HtmlElement) =
    html(lang="") {
        head() {
            //title() { "Contact App" }
            script()
        }
        body(style = "width: 800px; margin: 0 auto") {
            //main() {
            div() {
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
