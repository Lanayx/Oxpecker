module Oxpecker.Solid.Tests.Cases.Components

open Fable.Core.JS
open Oxpecker.Solid

[<SolidComponent>]
let Child (getText: Accessor<string>) =
    h1(onClick = fun _ -> console.log(getText())) {
        getText()
    }

[<SolidComponent>]
let Parent () =
    let getText, _ = createSignal "Hello"

    div() {
        Child(getText)
    }
