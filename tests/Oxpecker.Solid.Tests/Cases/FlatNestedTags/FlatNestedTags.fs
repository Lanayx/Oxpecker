module Oxpecker.Solid.Tests.Cases.FlatNestedTags

open Oxpecker.Solid

[<SolidComponent>]
let Test () =
    body() {
        "zero"
        h1(id="one")
        div(id="two") { "2" }
        div() { "3" }
        br()
        "five"
    }
