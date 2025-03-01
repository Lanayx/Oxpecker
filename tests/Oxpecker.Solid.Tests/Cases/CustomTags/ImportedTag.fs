namespace Oxpecker.Solid.Tests.Cases.ImportedTag

open Fable.Core
open Oxpecker.Solid

[<Import("ImportedName","ImportedModule")>]
type ImportedTag() =
    inherit RegularNode()
