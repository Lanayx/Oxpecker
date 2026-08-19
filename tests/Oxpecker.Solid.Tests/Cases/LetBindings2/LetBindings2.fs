module Oxpecker.Solid.Tests.Cases.LetBindings2

open Browser
open Oxpecker.Solid

[<SolidComponent>]
let Test () =
    let items, _ = createSignal<string array> [||]
    let emptyMsg = div(class' = "empty") { "Nothing yet" }
    div(class' = "wrapper") {
        Show(when' = (items().Length > 0), fallback = emptyMsg) { div(class' = "list") { "items go here" } }
        form(novalidate = true) {
            input(type' = "text", name = "host")
            button(type' = "submit") { "Add" }
        }
    }
