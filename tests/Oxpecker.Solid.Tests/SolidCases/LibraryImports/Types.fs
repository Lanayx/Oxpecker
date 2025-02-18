module Oxpecker.Solid.Tests.Cases.Types

open Oxpecker.Solid
open Fable.Core

[<Import("Faketag","fakemodule")>]
type ImportedTag() =
    inherit RegularNode()
