namespace Oxpecker.Solid

open System.Runtime.CompilerServices
open Browser.Types

[<AutoOpen>]
module Solid =

    /// Solid on* event handlers
    type HtmlTag with
        member this.onClick with set (_: MouseEvent -> unit) = ()

