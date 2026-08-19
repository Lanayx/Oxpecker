module Oxpecker.Solid.Tests.Cases.LetBindings2

open Browser
open Oxpecker.Solid

[<SolidComponent>]
let Test () =
    let count, _ = createSignal 0
    let x = span()
    body() { x }
