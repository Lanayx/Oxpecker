namespace Oxpecker.Solid.Component

open Oxpecker.Solid
open Fable.Core

[<Erase; Import("Faketag","fakemodule")>]
type ImportedTag() =
    inherit RegularNode()
