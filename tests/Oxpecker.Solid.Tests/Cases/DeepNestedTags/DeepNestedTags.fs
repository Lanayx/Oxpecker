module Oxpecker.Solid.Tests.Cases.DeepNestedTags

open Oxpecker.Solid

[<SolidComponent>]
let DeepNestedTags () =
    body() {
        div(id="outer") {
            div(id="inner") {
                h1() { "Hello!" }
            }
        }
    }
