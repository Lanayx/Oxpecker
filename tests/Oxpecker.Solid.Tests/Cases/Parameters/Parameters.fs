module Oxpecker.Solid.Tests.Cases.Parameters

open Oxpecker.Solid
open Oxpecker.Solid.Aria

[<SolidComponent>]
let Test (id: int) =
    let hello = "Hello "
    div(id=string id, class'="testclass", ariaLabelledBy="testlabel") {
        Fragment() { hello }; "world!"
    }
