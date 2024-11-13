module Oxpecker.Solid.Tests.Cases.LetBindings

open System
open Oxpecker.Solid

[<SolidComponent>]
let Test () =
    body() {
        let x = DateTime.Now
        h1 () {
            string x
        }
    }
