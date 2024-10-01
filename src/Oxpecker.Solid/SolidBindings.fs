namespace Oxpecker.Solid

open Browser.Types
open Fable.Core

[<AutoOpen>]
module Bindings =

    /// Solid on* event handlers
    type HtmlTag with
        member this.onClick with set (_: MouseEvent -> unit) = ()

[<AutoOpen>]
type Bindings =
    [<ImportMember("solid-js/web")>]
    static member render(f: unit -> HtmlElement, el: Element): unit = jsNative

    [<ImportMember("solid-js")>]
    static member createSignal(value: 'T): (unit -> 'T) * ('T -> unit) = jsNative
