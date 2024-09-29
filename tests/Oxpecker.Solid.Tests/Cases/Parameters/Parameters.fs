module Oxpecker.Solid.Tests.Cases.Arguments

open Oxpecker.Solid

[<SolidComponent>]
let NestedTags (id: int) =
    let hello = "Hello "
    div(id=string id, class'="testclass") {
        __() { hello }; "world!"
    }
