namespace Oxpecker.Solid.Router

open System.Runtime.CompilerServices
open Fable.Core
open Fable.Core.JsInterop
open System
open Oxpecker.Solid

[<AutoOpen>]
module Bindings =

    /// Navigation intent values used to decide when routes are preloaded.
    [<RequireQualifiedAccess>]
    [<StringEnum(CaseRules.None)>]
    type Intent =
        /// Router is initializing at its current URL.
        | initial
        /// Browser-native navigation or link behavior triggered the request.
        | native
        /// Client-side navigation triggered the request.
        | navigate
        /// Explicit preloading triggered the request.
        | preload

    /// Options that control a router navigation.
    type NavigateOptions =
        /// Whether the destination is resolved relative to the current route.
        abstract member resolve: bool with get, set
        /// Whether navigation replaces the current history entry.
        abstract member replace: bool with get, set
        /// Whether navigation restores or resets scroll position.
        abstract member scroll: bool with get, set
        /// User state stored with a history entry.
        abstract member state: obj with get, set

    /// Function for navigating to a path or by a history delta.
    type Navigator =
        /// Navigates to a destination path with optional navigation settings.
        [<Emit("$0($1...)")>]
        abstract member Invoke: ``to``: string * ?options: NavigateOptions -> unit
        /// Navigates by a history delta, such as `-1` to go back.
        [<Emit("$0($1...)")>]
        abstract member Invoke: delta: float -> unit

    /// URL path, query, and fragment components.
    type Path =
        /// URL path component without query string or hash.
        abstract member pathname: string with get, set
        /// URL query string, including the leading `?` when present.
        abstract member search: string with get, set
        /// URL fragment, including the leading `#` when present.
        abstract member hash: string with get, set

    /// Current router location, including parsed query and history state.
    type Location =
        inherit Path
        /// Parsed query parameters for the current location.
        abstract member query: obj with get, set
        /// User state stored with a history entry.
        abstract member state: obj option with get, set
        /// Unique history key for this location.
        abstract member key: string with get, set

    /// Information and controls for navigation about to leave the current route.
    type BeforeLeaveEventArgs =
        /// Location from which navigation is leaving.
        abstract member from: Location with get, set
        /// Navigation destination, either a path or a history delta.
        abstract member ``to``: U2<string, float> with get, set
        /// Options supplied for the pending navigation.
        abstract member options: NavigateOptions option with get, set
        /// Whether this navigation has already been prevented.
        abstract member defaultPrevented: bool with get
        /// Cancels the pending navigation.
        abstract member preventDefault: unit -> unit
        /// Retries the pending navigation; `force` bypasses another leave check.
        abstract member retry: ?force: bool -> unit

    /// Route and navigation details supplied to a preload callback.
    type RoutePreloadFuncArgs =
        /// Parameters extracted from the matched route path.
        abstract member ``params``: obj with get, set
        /// Current route location.
        abstract member location: Location with get, set
        /// Reason or navigation mode that triggered route preloading.
        abstract member intent: Intent with get, set

    /// Callback that preloads data for a matched route.
    type RoutePreloadFunc = RoutePreloadFuncArgs -> unit

    /// Result of matching a URL path to a route.
    type PathMatch =
        /// Parameters extracted from the matched route path.
        abstract member ``params``: obj with get, set
        /// Matched route path pattern or concrete path.
        abstract member path: string with get, set

    /// Route pattern and metadata used by the router.
    type RouteDescription =
        /// Identity key used to preserve matching route instances across navigation.
        abstract member key: obj with get, set
        /// Original route path pattern before normalization.
        abstract member originalPath: string with get, set
        /// Compiled route matching pattern.
        abstract member pattern: string with get, set
        /// Optional function that preloads data for this route.
        abstract member preload: RoutePreloadFunc option with get, set
        /// Matches a pathname and returns its parameters when successful.
        abstract member matcher: (string -> PathMatch option) with get, set
        /// Optional route parameter filters used while matching.
        abstract member matchFilters: obj option with get, set
        /// Optional user metadata attached to the route.
        abstract member info: obj option with get, set

    /// A matched route together with its route definition.
    type RouteMatch =
        inherit PathMatch
        /// Route definition associated with this match.
        abstract member route: RouteDescription with get, set

    /// Declares a route path, component, and optional matching or preload behavior.
    [<Import("Route", "@solidjs/router")>]
    type Route() =
        interface HtmlElement
        /// Matched route path pattern or concrete path.
        [<Erase>]
        member this.path
            with set (value: string) = ()
        /// Component rendered when this route matches.
        [<Erase>]
        member this.component'
            with set (value: unit -> HtmlElement) = ()
        /// Optional route parameter filters used while matching.
        [<Erase>]
        member this.matchFilters
            with set (value: obj) = ()
        /// Optional function that preloads data for this route.
        [<Erase>]
        member this.preload
            with set (value: RoutePreloadFunc) = ()
        [<Erase>]
        member inline _.Combine
            ([<InlineIfLambda>] first: HtmlContainerFun, [<InlineIfLambda>] second: HtmlContainerFun)
            : HtmlContainerFun =
            fun builder ->
                first builder
                second builder
        [<Erase>]
        member inline _.Delay([<InlineIfLambda>] delay: unit -> HtmlContainerFun) : HtmlContainerFun = delay()
        [<Erase>]
        member inline _.Zero() : HtmlContainerFun = ignore
        [<Erase>]
        member inline _.Yield(value: Route) : HtmlContainerFun = fun cont -> ignore value

    /// Properties passed to the custom root component used by the router.
    [<AllowNullLiteral>]
    [<Global>]
    type RootProps [<ParamObject; Emit("$0")>] (children: HtmlElement) =
        /// Child elements rendered by the custom router root.
        member val children: HtmlElement = jsNative with get, set

    /// Route configuration accepted by the router root.
    [<AllowNullLiteral>]
    [<Global>]
    type RootConfig [<ParamObject; Emit("$0")>] (path: string, ``component``: HtmlElement) =
        /// Path pattern associated with this route configuration.
        member val path: string = jsNative with get, set
        /// Component rendered for this configured route.
        member val ``component``: HtmlElement = jsNative with get, set

    /// Creates a client-side router for nested routes.
    [<Import("Router", "@solidjs/router")>]
    type Router() =
        interface HtmlElement
        /// Custom root component that wraps the router tree.
        [<Erase>]
        member this.root
            with set (value: RootProps -> HtmlElement) = ()
        /// Base path prepended to route paths.
        [<Erase>]
        member this.base'
            with set (value: string) = ()
        /// Base path used for form actions.
        [<Erase>]
        member this.actionBase
            with set (value: string) = ()
        /// Whether link navigation intents invoke route preload callbacks.
        [<Erase>]
        member this.preload
            with set (value: bool) = ()
        /// Whether only links with explicit preload attributes are preloaded.
        [<Erase>]
        member this.explicitLinks
            with set (value: bool) = ()
        /// Initial URL used by the router.
        [<Erase>]
        member this.url
            with set (value: string) = ()
        [<Erase>]
        member inline _.Combine
            ([<InlineIfLambda>] first: HtmlContainerFun, [<InlineIfLambda>] second: HtmlContainerFun)
            : HtmlContainerFun =
            fun builder ->
                first builder
                second builder
        [<Erase>]
        member inline _.Delay([<InlineIfLambda>] delay: unit -> HtmlContainerFun) : HtmlContainerFun = delay()
        [<Erase>]
        member inline _.Zero() : HtmlContainerFun = ignore
        [<Erase>]
        member inline _.Yield(value: Route) : HtmlContainerFun = fun cont -> ignore value
        [<Erase>]
        member inline _.Yield(value: RootConfig[]) : HtmlContainerFun = fun cont -> ignore value

    /// Creates a router that stores its location in the URL hash.
    [<Import("HashRouter", "@solidjs/router")>]
    type HashRouter() =
        inherit Router()

    [<AllowNullLiteral>]
    [<Global>]
    type PreloadData [<ParamObject; Emit("$0")>] (preloadData: bool) =
        /// Whether route data should be preloaded.
        member val preloadData: bool = jsNative with get, set

    /// Function for preloading a route URL before navigation.
    type RoutePreloader =
        /// Preloads the route at the URL, optionally controlling whether its data is preloaded.
        [<Emit("$0($1...)")>]
        abstract member Invoke: url: string * ?options: PreloadData -> unit

    [<Erase>]
    type Extensions =
        /// Completes the router computation expression and returns its builder.
        [<Extension; Erase>]
        static member Run(this: Router, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

        /// Completes the route computation expression and returns its builder.
        [<Extension; Erase>]
        static member Run(this: Route, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

    [<Import("A", "@solidjs/router")>]
    type A() =
        interface RegularNode
        /// Destination path for this navigation link.
        [<Erase>]
        member this.href
            with set (value: string) = ()
        /// Prevents automatic scroll restoration on navigation.
        [<Erase>]
        member this.noScroll
            with set (value: bool) = ()
        /// Whether navigation replaces the current history entry.
        [<Erase>]
        member this.replace
            with set (value: bool) = ()
        /// User state stored with a history entry.
        [<Erase>]
        member this.state
            with set (value: obj) = ()
        /// CSS class applied while this link matches the active route.
        [<Erase>]
        member this.activeClass
            with set (value: string) = ()
        /// CSS class applied while this link is inactive.
        [<Erase>]
        member this.inactiveClass
            with set (value: string) = ()
        /// Whether the link must match the route to its end to be considered active.
        [<Erase>]
        member this.end'
            with set (value: bool) = ()

    /// Declarative component that navigates to a path.
    [<Import("Navigate", "@solidjs/router")>]
    type Navigate() =
        interface RegularNode
        /// Destination path for this navigation link.
        [<Erase>]
        member this.href
            with set (value: string) = ()
        /// User state stored with a history entry.
        [<Erase>]
        member this.state
            with set (value: obj) = ()


[<AutoOpen>]
[<Erase>]
type Bindings =

    /// Returns a function for programmatic navigation.
    [<ImportMember("@solidjs/router")>]
    static member useNavigate() : Navigator = jsNative

    /// Returns the reactive current location.
    [<ImportMember("@solidjs/router")>]
    static member useLocation() : Location = jsNative

    /// Returns an accessor indicating whether route navigation is in progress.
    [<ImportMember("@solidjs/router")>]
    static member useIsRouting() : (unit -> bool) = jsNative

    /// Returns an accessor containing the matched path and parameters, or None when the pattern does not match.
    [<ImportMember("@solidjs/router")>]
    static member useMatch(fn: unit -> string, ?matchFilters: obj) : (unit -> PathMatch option) = jsNative

    /// Returns parameters for the currently matched route.
    [<ImportMember("@solidjs/router")>]
    static member useParams() : obj = jsNative

    /// Registers a callback that can inspect or cancel navigation away from the current route.
    [<ImportMember("@solidjs/router")>]
    static member useBeforeLeave(listener: BeforeLeaveEventArgs -> unit) : unit = jsNative

    /// Returns an accessor for route matches active at the current location.
    [<ImportMember("@solidjs/router")>]
    static member useCurrentMatches() : unit -> RouteMatch[] = jsNative

    /// Returns a callable that preloads a route URL, with optional data-preloading settings.
    [<ImportMember("@solidjs/router")>]
    static member usePreloadRoute() : RoutePreloader = jsNative

    /// Returns reactive search parameters and a setter.
    [<ImportMember("@solidjs/router")>]
    static member useSearchParams() : Signal<obj> = jsNative
