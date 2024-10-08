module Oxpecker.Solid.Tests.Cases.ForLoop

open Oxpecker.Solid

[<SolidComponent>]
let Test () =
    div() {
        For(each = [|"one"; "two"; "three"|]) {
            yield fun item index ->
                h2(id = (index() |> string)) {
                    item
                }
        }
        Index(each = [|"one"; "two"; "three"|]) {
            yield fun item index ->
                h2(id = (index |> string)) {
                    item()
                }
        }
    }
