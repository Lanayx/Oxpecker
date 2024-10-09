module Oxpecker.Solid.Tests.Cases.HelloWorld

open Browser
open Oxpecker.Solid

[<SolidComponent>]
let Test () =
    console.log "Hello, World!"
    body()
