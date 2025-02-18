module Oxpecker.Solid.Tests.Cases.LibraryImportsChildren

open Oxpecker.Solid
open Oxpecker.Solid.Tests.Cases.Types


[<SolidComponent>]
let Button () =
    ImportedTag(class'="TEST").attr("key","value") { "Child" }
