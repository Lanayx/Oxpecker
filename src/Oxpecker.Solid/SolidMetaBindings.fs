namespace Oxpecker.Solid.Meta

open Fable.Core
open Oxpecker.Solid

[<AutoOpen>]
module Bindings =

    [<Import("MetaProvider", "@solidjs/meta")>]
    type MetaProvider() =
        interface FragmentNode

    [<Import("Title", "@solidjs/meta")>]
    type Title() =
        inherit title()

    [<Import("Style", "@solidjs/meta")>]
    type Style() =
        inherit style()

    [<Import("Link", "@solidjs/meta")>]
    type Link() =
        inherit link()

    [<Import("Meta", "@solidjs/meta")>]
    type Meta() =
        inherit meta()

    [<Import("Base", "@solidjs/meta")>]
    type Base() =
        inherit base'()
