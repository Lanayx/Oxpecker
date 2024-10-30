namespace Oxpecker.Solid.Router

open System.Runtime.CompilerServices
open Fable.Core
open Fable.Core.JsInterop
open System
open Oxpecker.Solid

[<AutoOpen>]
module Bindings =

    [<RequireQualifiedAccess>]
    [<StringEnum(CaseRules.None)>]
    type Intent =
        | initial
        | native
        | navigate
        | preload

    type NavigateOptions =
        abstract member resolve: bool with get, set
        abstract member replace: bool with get, set
        abstract member scroll: bool with get, set
        abstract member state: obj with get, set

    type Navigator =
        [<Emit("$0($1...)")>]
        abstract member Invoke: ``to``: string * ?options: NavigateOptions -> unit
        [<Emit("$0($1...)")>]
        abstract member Invoke: delta: float -> unit

    type Path =
        abstract member pathname: string with get, set
        abstract member search: string with get, set
        abstract member hash: string with get, set

    type Location =
        inherit Path
        abstract member query: obj with get, set
        abstract member state: obj option with get, set
        abstract member key: string with get, set

    type BeforeLeaveEventArgs =
        abstract member from: Location with get, set
        abstract member ``to``: U2<string, float> with get, set
        abstract member options: NavigateOptions option with get, set
        abstract member defaultPrevented: bool with get
        abstract member preventDefault: unit -> unit
        abstract member retry: ?force: bool -> unit

    type RoutePreloadFuncArgs =
        abstract member ``params``: obj with get, set
        abstract member location: Location with get, set
        abstract member intent: Intent with get, set

    type RoutePreloadFunc = RoutePreloadFuncArgs -> unit

    type PathMatch =
        abstract member ``params``: obj with get, set
        abstract member path: string with get, set

    type RouteDescription =
        abstract member key: obj with get, set
        abstract member originalPath: string with get, set
        abstract member pattern: string with get, set
        abstract member preload: RoutePreloadFunc option with get, set
        abstract member matcher: (string -> PathMatch option) with get, set
        abstract member matchFilters: obj option with get, set
        abstract member info: obj option with get, set

    type RouteMatch =
        inherit PathMatch
        abstract member route: RouteDescription with get, set

    [<Import("Route", "@solidjs/router")>]
    type Route() =
        interface HtmlElement
        [<Erase>]
        member this.path
            with set (value: string) = ()
        [<Erase>]
        member this.component'
            with set (value: unit -> HtmlElement) = ()
        [<Erase>]
        member this.matchFilters
            with set (value: obj) = ()
        [<Erase>]
        member this.preload
            with set (value: RoutePreloadFunc) = ()
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
    type RootConfig [<ParamObject; Emit("$0")>] (path: string, ``component``: HtmlElement) =
        member val path: string = jsNative with get, set
        member val ``component``: HtmlElement = jsNative with get, set

    [<Import("Router", "@solidjs/router")>]
    type Router() =
        interface HtmlElement
        [<Erase>]
        member this.root
            with set (value: RootProps -> HtmlElement) = ()
        [<Erase>]
        member this.base'
            with set (value: string) = ()
        [<Erase>]
        member this.actionBase
            with set (value: string) = ()
        [<Erase>]
        member this.preload
            with set (value: bool) = ()
        [<Erase>]
        member this.explicitLinks
            with set (value: bool) = ()
        [<Erase>]
        member this.url
            with set (value: string) = ()
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

    [<Import("HashRouter", "@solidjs/router")>]
    type HashRouter() =
        inherit Router()

    [<AllowNullLiteral>]
    [<Global>]
    type PreloadData [<ParamObject; Emit("$0")>] (preloadData: bool) =
        member val preloadData: bool = jsNative with get, set

    [<Erase>]
    type Extensions =
        [<Extension; Erase>]
        static member Run(this: Router, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

        [<Extension; Erase>]
        static member Run(this: Route, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

    [<Import("A", "@solidjs/router")>]
    type A() =
        inherit RegularNode()
        [<Erase>]
        member this.href
            with set (value: string) = ()
        [<Erase>]
        member this.noScroll
            with set (value: bool) = ()
        [<Erase>]
        member this.replace
            with set (value: bool) = ()
        [<Erase>]
        member this.state
            with set (value: obj) = ()
        [<Erase>]
        member this.activeClass
            with set (value: string) = ()
        [<Erase>]
        member this.inactiveClass
            with set (value: string) = ()
        [<Erase>]
        member this.end'
            with set (value: bool) = ()

    [<Import("Navigate", "@solidjs/router")>]
    type Navigate() =
        inherit RegularNode()
        [<Erase>]
        member this.href
            with set (value: string) = ()
        [<Erase>]
        member this.state
            with set (value: obj) = ()


[<AutoOpen>]
[<Erase>]
type Bindings =

    [<ImportMember("@solidjs/router")>]
    static member useNavigate() : Navigator = jsNative

    [<ImportMember("@solidjs/router")>]
    static member useLocation() : Location = jsNative

    [<ImportMember("@solidjs/router")>]
    static member useIsRouting() : (unit -> bool) = jsNative

    [<ImportMember("@solidjs/router")>]
    static member useMatch(fn: unit -> string, ?matchFilters: obj) : (unit -> bool) = jsNative

    [<ImportMember("@solidjs/router")>]
    static member useParams() : obj = jsNative

    [<ImportMember("@solidjs/router")>]
    static member useBeforeLeave(listener: BeforeLeaveEventArgs -> unit) : unit = jsNative

    [<ImportMember("@solidjs/router")>]
    static member useCurrentMatches() : unit -> RouteMatch[] = jsNative

    [<ImportMember("@solidjs/router")>]
    static member usePreloadRoute() : (string -> PreloadData) -> unit = jsNative

    [<ImportMember("@solidjs/router")>]
    static member useSearchParams() : Signal<obj> = jsNative
