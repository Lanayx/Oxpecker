module Oxpecker.Solid.Tests.Cases.Branches

open Oxpecker.Solid
open MyUnions

[<SolidComponent>]
let Test (x: Step) : HtmlElement =
    let s, setS = createSignal x

    h1() {
        match s() with
        | One -> div().bool("visible", true)
        | Two x ->
            match x() with
            | A ->
                h2() {
                    "Hello"
                }
            | B y when y() < 0  ->
                h3()
            | B z ->
                h1(id="id2").attr("abc", "def") {
                    if z() > 10 then
                        h1(id="id1")
                    else
                        "Hello"
                }
    }
