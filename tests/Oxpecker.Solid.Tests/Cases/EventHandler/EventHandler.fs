module Oxpecker.Solid.Tests.Cases.EventHandler

open Browser.Types
open Oxpecker.Solid
open Browser

let clicked2 = fun (evt: MouseEvent) -> console.log(evt.``type``)

[<SolidComponent>]
let EventHandler () =
    div(onClick = fun _ -> console.log("clicked1")) {
        h1(onClick = clicked2) {
            "Hello"
        }
        h2()
            .on("click", fun _ -> console.log("clicked3"))
            .on("change", fun _ -> console.log("changed"))
            .on("load", fun _ -> console.log("loaded")){
            "Hello"
        }
        h3(id="test", onClick = fun _ -> console.log("clicked4"))
            .on("change", fun _ -> console.log("changed"))
            .on("load", fun _ -> console.log("loaded"))
    }
