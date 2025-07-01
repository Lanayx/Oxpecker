namespace Oxpecker.Solid

open System.Runtime.CompilerServices
open Fable.Core

[<AutoOpen>]
module Builder =

    [<Struct>]
    [<Erase>]
    type HtmlAttribute = { Name: string; Value: obj }

    type HtmlElement = interface end
    type HtmlTag =
        inherit HtmlElement
    type HtmlContainer =
        inherit HtmlElement
    type RegularNode =
        inherit HtmlTag
        inherit HtmlContainer
    type FragmentNode =
        inherit HtmlContainer
    type VoidNode =
        inherit HtmlTag

    type HtmlContainerFun = HtmlContainer -> unit

    type HtmlContainer with
        member inline _.Combine
            ([<InlineIfLambda>] first: HtmlContainerFun, [<InlineIfLambda>] second: HtmlContainerFun)
            : HtmlContainerFun =
            fun builder ->
                first builder
                second builder

        [<Erase>]
        member inline _.Zero() : HtmlContainerFun = ignore

        [<Erase>]
        member inline _.Delay([<InlineIfLambda>] delay: unit -> HtmlContainerFun) : HtmlContainerFun = delay()

        [<Erase>]
        member inline _.Yield(element: #HtmlElement) : HtmlContainerFun = fun cont -> ignore element

        [<Erase>]
        member inline _.Yield(text: string) : HtmlContainerFun = fun cont -> ignore text

        [<Erase>]
        member inline _.Yield(text: int) : HtmlContainerFun = fun cont -> ignore text

        [<Erase>]
        member inline _.Yield(element: JSX.Element) : HtmlContainerFun = fun cont -> ignore element

    [<Erase>]
    type HtmlContainerExtensions =
        [<Extension; Erase>]
        static member Run(this: #HtmlContainer, runExpr: HtmlContainerFun) =
            runExpr this
            this
