module Oxpecker.Solid.Tests.Cases.HelloWorld

open Browser
open Oxpecker.Solid

[<SolidComponent>]
let helloWorld () =
    console.log "Hello, World!"
    body()
