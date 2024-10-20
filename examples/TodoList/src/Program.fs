open Browser
open App
open Oxpecker.Solid
open Oxpecker.Solid.Router
open Fable.Core.JsInterop

importAll "./index.css"

[<SolidComponent>]
let myRouter() =
    Router() {
        Route(path="/", component'=App)
        Route(path="/about", component'=About)
    }

render (myRouter, document.getElementById "root")
