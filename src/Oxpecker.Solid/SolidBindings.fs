namespace Oxpecker.Solid

open System.Runtime.CompilerServices
open Browser.Types
open Fable.Core
open System

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


    [<RequireQualifiedAccess; StringEnum>]
    type SolidResourceState =
        /// Hasn't started loading, no value yet
        | Unresolved
        /// It's loading, no value yet
        | Pending
        /// Finished loading, has value
        | Ready
        /// It's re-loading, `latest` has value
        | Refreshing
        /// Finished loading with an error, no value
        | Errored

    type SolidResource<'T> =
        /// Attention, will be undefined while loading
        [<Emit("$0()")>]
        abstract current: 'T
        abstract state: SolidResourceState
        abstract loading: bool
        abstract error: exn option
        /// Unlike `current`, it keeps the latest value while re-loading
        /// Attention, will be undefined until first value has been loaded
        abstract latest: 'T

    type SolidResourceManager<'T> =
        abstract mutate: 'T -> 'T
        abstract refetch: unit -> JS.Promise<'T>

    type SolidStoreSetter<'T> =
        /// Replace old store value with new
        [<Emit("$0($1)")>]
        abstract Update: newValue: 'T -> unit
        /// Update store specifying updater function from old value to new value
        [<Emit("$0($1)")>]
        abstract Update: updater: ('T -> 'T) -> unit
        /// Update store using native solid path syntax
        [<Emit("$0(...$1)")>]
        abstract UpdatePath: pathArgs: obj[] -> unit

    type SolidStorePath<'T, 'Value>(setter: SolidStoreSetter<'T>, path: obj[]) =
        member _.Setter = setter
        member _.Path = path
        /// Choose the store item that should be updated
        member inline this.Map(map: 'Value -> 'Value2) =
            SolidStorePath<'T, 'Value2>(
                this.Setter,
                Experimental.namesofLambda map |> Array.map box |> Array.append this.Path
            )
        /// Update store item using new value
        member this.Update(value: 'Value) : unit =
            this.Setter.UpdatePath(Array.append this.Path [| value |])
        /// Update store item specifying updater function from old value to new value
        member this.Update(updater: 'Value -> 'Value) : unit =
            this.Setter.UpdatePath(Array.append this.Path [| updater |])

    [<AutoOpen>]
    module SolidExtensions =

        type SolidStoreSetter<'T> with
            /// Access more convenient way of updating store items
            member this.Path = SolidStorePath<'T, 'T>(this, [||])

    [<Runtime.CompilerServices.Extension>]
    type SolidStorePathExtensions =

        /// Select store item by index
        [<Runtime.CompilerServices.Extension>]
        static member inline Item(this: SolidStorePath<'T, 'Value array>, index: int) =
            SolidStorePath<'T, 'Value>(this.Setter, Array.append this.Path [| index |])

        /// Select store item by predicate
        [<Runtime.CompilerServices.Extension>]
        static member inline Find(this: SolidStorePath<'T, 'Value array>, predicate: 'Value -> bool) =
            SolidStorePath<'T, 'Value>(this.Setter, Array.append this.Path [| predicate |])


[<AutoOpen>]
type Bindings =

    [<ImportMember("solid-js/web")>]
    static member render(code: unit -> #HtmlElement, element: #Element) : unit = jsNative

    [<ImportMember("solid-js")>]
    static member createSignal(value: 'T) : Signal<'T> = jsNative

    [<ImportMember("solid-js")>]
    static member createMemo(value: unit -> 'T) : (unit -> 'T) = jsNative

    [<ImportMember("solid-js")>]
    static member createEffect(effect: unit -> unit) : unit = jsNative

    [<ImportMember("solid-js")>]
    static member createEffect(effect: 'T -> 'T, initialValue: 'T) : unit = jsNative

    /// Fetcher will be called immediately
    [<ImportMember("solid-js"); ParamObject(fromIndex = 1)>]
    static member createResource
        (fetcher: unit -> JS.Promise<'T>, ?initialValue: 'T)
        : SolidResource<'T> * SolidResourceManager<'T> =
        jsNative

    /// Fetcher will be called only when source signal returns `Some('U)`
    [<ImportMember("solid-js"); ParamObject(fromIndex = 2)>]
    static member createResource
        (source: unit -> 'U option, fetcher: 'U -> JS.Promise<'T>, ?initialValue: 'T)
        : SolidResource<'T> * SolidResourceManager<'T> =
        jsNative

    [<ImportMember("solid-js")>]
    static member createRoot(fn (* dispose *) : Action -> 'T) : 'T = jsNative

    [<ImportMember("solid-js")>]
    static member createUniqueId() : string = jsNative

    [<ImportMember("solid-js/store")>]
    static member createStore(store: 'T) : 'T * SolidStoreSetter<'T> = jsNative
