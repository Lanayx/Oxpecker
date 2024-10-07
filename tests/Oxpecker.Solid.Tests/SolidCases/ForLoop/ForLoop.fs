module Oxpecker.Solid.Tests.Cases.ForLoop

open Oxpecker.Solid

[<SolidComponent>]
let Test () =
    div() {
        For(each = [|"one"; "two"; "three"|]) {
            fun (item: string) index ->
                h2(id = (index() |> string)) {
                    item
                }
        }
    }
