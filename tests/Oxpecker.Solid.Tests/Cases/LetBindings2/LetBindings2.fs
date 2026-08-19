module Oxpecker.Solid.Tests.Cases.LetBindings2

open Browser
open Oxpecker.Solid

[<SolidComponent>]
let Test () =
    let x = span()
    body() { x }
