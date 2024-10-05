module Oxpecker.Solid.Tests.Cases.ForLoop

open Oxpecker.Solid

[<SolidComponent>]
let ForLoop () =
    div() {
        For(each = ["one"; "two"; "three"]) {
            fun (item: string) index ->
                h2(id = string index) {
                    item
                }
        }
    }
