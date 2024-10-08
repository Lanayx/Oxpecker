module Oxpecker.Solid.Tests.Cases.Conditional

open Oxpecker.Solid

[<SolidComponent>]
let Test (show: bool) =
    let x, _ = createSignal 7
    div() {
        Show(when'=show, fallback = div() { "World" }) {
            div() {
                "Hello"
            }
        }
        Switch(fallback= p(){ x(); "is between 5 and 10" }) {
            Match(when'= (x() > 10)) {
                p() { x(); "is greater than 10" }
            }
            Match(when'= (x() < 5)) {
                p() { x(); "is less than 5" }
            }
        }
    }
