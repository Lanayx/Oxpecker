namespace Oxpecker.Solid

open System.Runtime.CompilerServices
open Browser.Types
open Fable.Core

type Setter<'T> = 'T -> unit
type Accessor<'T> = unit -> 'T
type Signal<'T> = Accessor<'T> * Setter<'T>

[<AutoOpen>]
module Bindings =

    /// Solid on* event handlers
    type HtmlTag with
        member this.onClick
            with set (_: MouseEvent -> unit) = ()

    type input with
        member this.onChange
            with set (_: Event -> unit) = ()

    type form with
        member this.onSubmit
            with set (_: Event -> unit) = ()

    type For<'T>() =
        interface HtmlElement
        member this.each
            with set (value: 'T[]) = ()
        member inline _.Zero() : HtmlContainerFun = ignore
        member inline _.Yield(value: 'T -> Accessor<int> -> #HtmlElement) : HtmlContainerFun = fun cont -> ignore value

    type Index<'T>() =
        interface HtmlElement
        member this.each
            with set (value: 'T[]) = ()
        member inline _.Zero() : HtmlContainerFun = ignore
        member inline _.Yield(value: Accessor<'T> -> int -> #HtmlElement) : HtmlContainerFun = fun cont -> ignore value

    type Show() =
        interface HtmlContainer
        member this.when'
            with set (value: bool) = ()
        member this.fallback
            with set (value: HtmlElement) = ()

    type Match() =
        interface HtmlContainer
        member this.when'
            with set (value: bool) = ()

    type Switch() =
        interface HtmlElement
        member this.fallback
            with set (value: HtmlElement) = ()
        member inline _.Combine
            ([<InlineIfLambda>] first: HtmlContainerFun, [<InlineIfLambda>] second: HtmlContainerFun)
            : HtmlContainerFun =
            fun builder ->
                first builder
                second builder

        member inline _.Delay([<InlineIfLambda>] delay: unit -> HtmlContainerFun) : HtmlContainerFun = delay()
        member inline _.Zero() : HtmlContainerFun = ignore
        member inline _.Yield(value: Match) : HtmlContainerFun = fun cont -> ignore value


    type Extensions =

        [<Extension>]
        static member Run(this: For<'T>, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

        [<Extension>]
        static member Run(this: Index<'T>, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

        [<Extension>]
        static member Run(this: Switch, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this


[<AutoOpen>]
type Bindings =
    [<ImportMember("solid-js/web")>]
    static member render(f: unit -> #HtmlElement, el: #Element) : unit = jsNative

    [<ImportMember("solid-js")>]
    static member createSignal(value: 'T) : Signal<'T> = jsNative
