module Oxpecker.Solid.Tests.Cases.Branches

open Oxpecker.Solid

[<SolidComponent>]
let Test (x: int) : HtmlElement =
    let s, setS = createSignal x

    h1() {
        match s() with
        | 0 -> div().bool("visible", true)
        | _ ->
            match x with
            | 1 ->
                h2() {
                    "Hello"
                }
            | 2 ->
                h3()
            | _ ->
                if x > 10 then
                    h1(id="id1")
                else
                    h1(id="id2").attr("abc", "def") {
                        "Hello"
                    }
    }
