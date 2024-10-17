module Oxpecker.Solid.Tests.Cases.CustomAttributes

open Oxpecker.Solid

[<SolidComponent>]
let Test () =
    body().attr("hello", "world") {
        h1().data("abcd", "efgh")
    }
