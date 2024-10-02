open Browser
open App
open Oxpecker.Solid

// HMR doesn't work in Root for some reason
[<SolidComponent>]
let Root() =
    __() {
        App()
    }

render (Root, document.getElementById "root")
