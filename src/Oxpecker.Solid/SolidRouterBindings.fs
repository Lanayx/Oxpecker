namespace Oxpecker.Solid.Router

#nowarn "46" // suppress warning about component being reserved
open System.Runtime.CompilerServices
open Fable.Core
open Oxpecker.Solid

[<AutoOpen>]
module Bindings =

    [<Import("Route", "@solidjs/router")>]
    type Route() =
        interface HtmlElement
        member this.path
            with set (value: string) = ()
        member this.component'
            with set (value: unit -> HtmlElement) = ()
        member inline _.Combine
            ([<InlineIfLambda>] first: HtmlContainerFun, [<InlineIfLambda>] second: HtmlContainerFun)
            : HtmlContainerFun =
            fun builder ->
                first builder
                second builder
        member inline _.Delay([<InlineIfLambda>] delay: unit -> HtmlContainerFun) : HtmlContainerFun = delay()
        member inline _.Zero() : HtmlContainerFun = ignore
        member inline _.Yield(value: Route) : HtmlContainerFun = fun cont -> ignore value

    [<AllowNullLiteral>]
    [<Global>]
    type RootProps [<ParamObject; Emit("$0")>] (children: HtmlElement) =
        member val children: HtmlElement = jsNative with get, set

    [<AllowNullLiteral>]
    [<Global>]
    type RootConfig [<ParamObject; Emit("$0")>] (path: string, component: HtmlElement) =
        member val path: string = jsNative with get, set
        member val component: HtmlElement = jsNative with get, set

    [<Import("Router", "@solidjs/router")>]
    type Router() =
        interface HtmlElement
        member this.root
            with set (value: RootProps -> HtmlElement) = ()
        member inline _.Combine
            ([<InlineIfLambda>] first: HtmlContainerFun, [<InlineIfLambda>] second: HtmlContainerFun)
            : HtmlContainerFun =
            fun builder ->
                first builder
                second builder
        member inline _.Delay([<InlineIfLambda>] delay: unit -> HtmlContainerFun) : HtmlContainerFun = delay()
        member inline _.Zero() : HtmlContainerFun = ignore
        member inline _.Yield(value: Route) : HtmlContainerFun = fun cont -> ignore value
        member inline _.Yield(value: RootConfig[]) : HtmlContainerFun = fun cont -> ignore value

    type Extensions =
        [<Extension>]
        static member Run(this: Router, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

        [<Extension>]
        static member Run(this: Route, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this


[<AutoOpen>]
type Bindings =

    [<ImportMember("@solidjs/router")>]
    static member redirect(path: string) : unit = jsNative

    [<ImportMember("@solidjs/router")>]
    static member redirect(path: string, options) : unit = jsNative
