namespace Oxpecker.Solid.Meta

open Fable.Core
open Oxpecker.Solid

[<AutoOpen>]
module Bindings =

    /// Provides the owner context used by `@solidjs/meta` to manage document metadata.
    [<Import("MetaProvider", "@solidjs/meta")>]
    type MetaProvider() =
        interface FragmentNode

    /// Sets the document title through Solid's reactive metadata system.
    [<Import("Title", "@solidjs/meta")>]
    type Title() =
        inherit title()

    /// Registers a stylesheet through Solid's metadata provider.
    [<Import("Style", "@solidjs/meta")>]
    type Style() =
        inherit style()

    /// Adds a reactive link element to the document head.
    [<Import("Link", "@solidjs/meta")>]
    type Link() =
        inherit link()

    /// Adds a reactive metadata element to the document head.
    [<Import("Meta", "@solidjs/meta")>]
    type Meta() =
        inherit meta()

    /// Sets the document base URL through the metadata provider.
    [<Import("Base", "@solidjs/meta")>]
    type Base() =
        inherit base'()
