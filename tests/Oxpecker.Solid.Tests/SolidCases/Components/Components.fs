module Oxpecker.Solid.Tests.Cases.Components

open Oxpecker.Solid

[<SolidComponent>]
let Child () =
    h1() { "Hello" }

[<SolidComponent>]
let Parent () =
    __() {
        Child()
    }
