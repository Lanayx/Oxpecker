module Oxpecker.Solid.Tests.Cases.LibraryImports

open Oxpecker.Solid
open Oxpecker.Solid.Tests.Cases.Types


[<SolidComponent>]
let Button () =
    ImportedTag(class'="TEST").attr("key","value")
