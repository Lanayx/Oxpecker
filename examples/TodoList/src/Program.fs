open Browser
open App
open Oxpecker.Solid
open Oxpecker.Solid.Meta
open Oxpecker.Solid.Router
open Fable.Core.JsInterop

importAll "./index.css"

[<SolidComponent>]
let Layout (props: RootProps) : HtmlElement =
    MetaProvider() {
        Title() { "TODO list" }
        Suspense() { props.children }
    }

[<SolidComponent>]
let appRouter() =
    Router(root=Layout) {
        Route(path="/", component'=App)
        Route(path="/about", component'=About)
    }

render (appRouter, document.getElementById "root")
