namespace WeatherApp.templates.shared

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria
open Oxpecker.Htmx

#nowarn "3391"

module layout =

    let navLink (attrs: {| Href: string; Class: string; Ctx: HttpContext|}) =
        let finalClass =
            if attrs.Href = attrs.Ctx.Request.Path  then
                attrs.Class + " active"
            else
                attrs.Class
        a(href = attrs.Href, class' = finalClass)

    let navMenu (ctx: HttpContext) =
        __() {
            div(class'="nav-top-row ps-3 navbar navbar-dark"){
                div(class'="container-fluid"){
                    a(class'="navbar-brand", href="") { "OxpeckerApp" }
                }
            }
            input(type'="checkbox", title="Navigation menu", class'="navbar-toggler")

            div(class'="nav-scrollable").on("click", "document.querySelector('.navbar-toggler').click()") {
                nav(class'="flex-column") {
                    div(class'="nav-item px-3") {
                        navLink {| Class="nav-link"; Href="/"; Ctx=ctx |} {
                             span(class'="bi bi-house-door-fill-nav-menu", ariaHidden=true)
                             " Home"
                        }
                    }

                    div(class'="nav-item px-3") {
                        navLink {| Class="nav-link"; Href="/counter"; Ctx=ctx |} {
                            span(class'="bi bi-plus-square-fill-nav-menu", ariaHidden=true)
                            " Counter"
                        }
                    }

                    div(class'="nav-item px-3") {
                        navLink {| Class="nav-link"; Href="/weather"; Ctx=ctx |} {
                            span(class'="bi bi-list-nested-nav-menu", ariaHidden=true)
                            " Weather"
                        }
                    }
                }
            }
        }


    let mainLayout (ctx: HttpContext) (content: HtmlElement) =
        div(class'="page") {
            div(class'="sidebar"){
                navMenu ctx
            }
            main() {
                div(class'="top-row px-4"){
                    a(href="https://github.com/Lanayx/Oxpecker", target="_blank") { "About" }
                }
                article(class'="content px-4"){
                    content
                }
            }
        }


    let html (ctx: HttpContext) (content: HtmlElement) =
        html(lang="en") {
            head() {
                title() {
                    match ctx.Items.TryGetValue "Title" with
                    | true, title -> string title
                    | false, _ -> "WeatherApp"
                }
                meta(charset="utf-8")
                meta(name="viewport", content="width=device-width, initial-scale=1.0")
                base'(href="/")
                link(rel="stylesheet", href="/bootstrap/bootstrap.min.css")
                link(rel="stylesheet", href="/app.css")
                link(rel="icon", type'="image/png", href="/favicon.png")
                script(src="https://unpkg.com/htmx.org@1.9.10", crossorigin="anonymous")
                script(src="https://cdn.jsdelivr.net/npm/alpinejs@3.13.5/dist/cdn.min.js", crossorigin="anonymous")
            }
            body(hxBoost=true) {
                mainLayout ctx content
            }
        }
