open Browser
open App
open Oxpecker.Solid
open Fable.Core.JsInterop

importAll "./index.css"

// HMR doesn't work in Root for some reason
[<SolidComponent>]
let Root() =
    __() {
        App()
    }

render (Root, document.getElementById "root")
