module Oxpecker.Solid

open System.Runtime.CompilerServices
open Browser.Types

[<Struct>]
type HtmlAttribute = { Name: string; Value: obj }

type HtmlElement = interface end
type HtmlTag = interface end
type HtmlContainer = interface end

and
    RegularNode () =
    interface HtmlElement
    interface HtmlTag
    interface HtmlContainer

type HtmlContainerFun = HtmlContainer -> unit

type HtmlContainer with
    member inline _.Combine
        ([<InlineIfLambda>] first: HtmlContainerFun, [<InlineIfLambda>] second: HtmlContainerFun)
        : HtmlContainerFun =
            fun builder ->
                first builder
                second builder

    member inline _.Zero() : HtmlContainerFun = ignore

    member inline _.Delay([<InlineIfLambda>] delay: unit -> HtmlContainerFun) : HtmlContainerFun =
        delay()

    member inline _.For(values: #seq<'T>, [<InlineIfLambda>] body: 'T -> HtmlContainerFun) : HtmlContainerFun =
        ignore

    member inline _.Yield(element: #HtmlElement) : HtmlContainerFun = ignore

    member inline _.YieldFrom(elements: #seq<#HtmlElement>) : HtmlContainerFun = ignore

    member inline _.Yield(text: string) : HtmlContainerFun = fun txt -> text |> ignore

type HtmlContainerExtensions =
    [<Extension>]
    static member Run(this: #HtmlElement, runExpr: HtmlContainerFun) =
        runExpr this
        this

type HtmlTag with
    member this.id
        with set (_: string) = ()
    member this.class'
        with set (_: string) = ()
    member this.onClick
        with set (_: MouseEvent -> unit) = ()

type head() = inherit RegularNode()
type body() = inherit RegularNode()
type div() = inherit RegularNode()
type h1() = inherit RegularNode()

