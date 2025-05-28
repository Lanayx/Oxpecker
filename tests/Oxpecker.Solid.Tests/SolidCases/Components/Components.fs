module Oxpecker.Solid.Tests.Cases.Components

open Fable.Core.JS
open Oxpecker.Solid

[<SolidComponent>]
let Component (getText: Accessor<string>) =
    h1(onClick = fun _ -> console.log(getText())) {
        getText()
    }

[<SolidComponent>]
let Test () =
    let getText, _ = createSignal "Hello"

    div() {
        Component(getText)
    }

[<SolidComponent>]
let SvgTest () =
    Svg.circle(fillOpacity = "fillOpacity", ``clip-rule`` = "clipRule")
