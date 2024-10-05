module Oxpecker.Solid.Tests.Cases.Arguments

open Oxpecker.Solid
open Oxpecker.Solid.Aria

[<SolidComponent>]
let Parameters (id: int) =
    let hello = "Hello "
    div(id=string id, class'="testclass", ariaLabelledBy="testlabel") {
        __() { hello }; "world!"
    }
