namespace Oxpecker.Solid

open Browser.Types
open Fable.Core

type Setter<'T> = 'T -> unit
type Accessor<'T> = unit -> 'T
type Signal<'T> = Accessor<'T> * Setter<'T>

[<AutoOpen>]
module Bindings =

    /// Solid on* event handlers
    type HtmlTag with
        member this.onClick with set (_: MouseEvent -> unit) = ()

    type input with
        member this.onChange with set (_: Event -> unit) = ()

    type form with
        member this.onSubmit with set (_: Event -> unit) = ()

    type For<'T>() =
        interface HtmlContainer
        member this.each with set (value: 'T[]) = ()
        member inline _.Yield(value: 'T -> int -> #HtmlElement) : HtmlContainerFun = fun cont -> ignore value

[<AutoOpen>]
type Bindings =
    [<ImportMember("solid-js/web")>]
    static member render(f: unit -> #HtmlElement, el: #Element): unit = jsNative

    [<ImportMember("solid-js")>]
    static member createSignal(value: 'T): Signal<'T> = jsNative
