module Oxpecker.Solid.Tests.Cases.DeepNestedTags

open Oxpecker.Solid

[<SolidComponent>]
let DeepNestedTags () =
    html(xmlns = "http://www.w3.org/1999/xhtml") {
        body() {
            div(id="outer") {
                div(id="inner") {
                    h1() { "Hello!" }
                }
            }
        }
    }

