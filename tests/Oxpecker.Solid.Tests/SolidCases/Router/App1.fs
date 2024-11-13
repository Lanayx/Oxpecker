module Oxpecker.Solid.Tests.SolidCases.App1

open Fable.Core
open Oxpecker.Solid

[<ExportDefault>]
[<SolidComponent>]
let App1 () : HtmlElement =
    h1() { "Hello world 1" }
