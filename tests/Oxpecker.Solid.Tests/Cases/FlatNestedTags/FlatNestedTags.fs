module Oxpecker.Solid.Tests.Cases.FlatNestedTags

open Oxpecker.Solid

[<SolidComponent>]
let FlatNestedTags () =
    body() {
        "zero"
        h1(id="one")
        div(id="two") { "2" }
        div() { "3" }
        h1()
        "five"
    }
