module Oxpecker.Solid.Tests.SolidCases.Router.App2

open Fable.Core
open Oxpecker.Solid

[<ExportDefault>]
[<SolidComponent>]
let App2 () =
    h1() { "Hello world 2" } :> HtmlElement
