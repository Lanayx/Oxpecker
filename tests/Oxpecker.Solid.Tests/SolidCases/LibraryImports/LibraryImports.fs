module Oxpecker.Solid.Tests.Cases.LibraryImports

open Oxpecker.Solid
open Oxpecker.Solid.Tests.Cases.Types


[<SolidComponent>]
let Button () =
    div() {
        ImportedTag(class'="TEST").attr("key","value")
        ImportedTag().attr("key","value")
        ImportedTag().attr("key","value").attr("some","other")
    }
