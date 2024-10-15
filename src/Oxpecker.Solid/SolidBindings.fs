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
        member this.onDblClick
            with set (_: MouseEvent -> unit) = ()
        member this.onBlur
            with set (_: FocusEvent -> unit) = ()
        member this.onFocus
            with set (_: FocusEvent -> unit) = ()
        member this.onContextMenu
            with set (_: MouseEvent -> unit) = ()
        member this.onMouseDown
            with set (_: MouseEvent -> unit) = ()
        member this.onMouseUp
            with set (_: MouseEvent -> unit) = ()
        member this.onMouseEnter
            with set (_: MouseEvent -> unit) = ()
        member this.onMouseLeave
            with set (_: MouseEvent -> unit) = ()
        member this.onMouseOver
            with set (_: MouseEvent -> unit) = ()
        member this.onMouseOut
            with set (_: MouseEvent -> unit) = ()
        member this.onMouseMove
            with set (_: MouseEvent -> unit) = ()
        member this.onWheel
            with set (_: WheelEvent -> unit) = ()
        member this.onKeyDown
            with set (_: KeyboardEvent -> unit) = ()
        member this.onKeyUp
            with set (_: KeyboardEvent -> unit) = ()
        member this.onKeyPress
            with set (_: KeyboardEvent -> unit) = ()
        member this.onDrag
            with set (_: DragEvent -> unit) = ()
        member this.onDragEnd
            with set (_: DragEvent -> unit) = ()
        member this.onDragEnter
            with set (_: DragEvent -> unit) = ()
        member this.onDragLeave
            with set (_: DragEvent -> unit) = ()
        member this.onDragOver
            with set (_: DragEvent -> unit) = ()
        member this.onDragStart
            with set (_: DragEvent -> unit) = ()
        member this.onDrop
            with set (_: DragEvent -> unit) = ()
        member this.onScroll
            with set (_: Event -> unit) = ()
        member this.onPointerDown
            with set (_: PointerEvent -> unit) = ()
        member this.onPointerMove
            with set (_: PointerEvent -> unit) = ()
        member this.onPointerUp
            with set (_: PointerEvent -> unit) = ()
        member this.onPointerCancel
            with set (_: PointerEvent -> unit) = ()
        member this.onPointerEnter
            with set (_: PointerEvent -> unit) = ()
        member this.onPointerLeave
            with set (_: PointerEvent -> unit) = ()
        member this.onPointerOver
            with set (_: PointerEvent -> unit) = ()
        member this.onPointerOut
            with set (_: PointerEvent -> unit) = ()
        member this.onGotPointerCapture
            with set (_: PointerEvent -> unit) = ()
        member this.onLostPointerCapture
            with set (_: PointerEvent -> unit) = ()
        member this.onAnimationStart
            with set (_: AnimationEvent -> unit) = ()
        member this.onAnimationEnd
            with set (_: AnimationEvent -> unit) = ()
        member this.onAnimationIteration
            with set (_: AnimationEvent -> unit) = ()
        member this.onTransitionEnd
            with set (_: TransitionEvent -> unit) = ()
        member this.onTransitionRun
            with set (_: TransitionEvent -> unit) = ()
        member this.onTransitionStart
            with set (_: TransitionEvent -> unit) = ()
        member this.onTransitionCancel
            with set (_: TransitionEvent -> unit) = ()
        member this.onTouchStart
            with set (_: TouchEvent -> unit) = ()
        member this.onTouchMove
            with set (_: TouchEvent -> unit) = ()
        member this.onTouchEnd
            with set (_: TouchEvent -> unit) = ()
        member this.onTouchCancel
            with set (_: TouchEvent -> unit) = ()

    type form with
        member this.onSubmit
            with set (_: Event -> unit) = ()
        member this.onReset
            with set (_: Event -> unit) = ()

    type input with
        member this.onChange
            with set (_: Event -> unit) = ()
        member this.onInvalid
            with set (_: Event -> unit) = ()
        member this.onInput
            with set (_: UIEvent -> unit) = ()
        member this.onSelect
            with set (_: Event -> unit) = ()
        member this.onError
            with set (_: Event -> unit) = ()
        member this.onLoad
            with set (_: Event -> unit) = ()

    type select with
        member this.onChange
            with set (_: Event -> unit) = ()

    type textarea with
        member this.onChange
            with set (_: Event -> unit) = ()
        member this.onInput
            with set (_: UIEvent -> unit) = ()
        member this.onSelect
            with set (_: Event -> unit) = ()

    type details with
        member this.onToggle
            with set (_: Event -> unit) = ()

    type img with
        member this.onError
            with set (_: Event -> unit) = ()
        member this.onLoad
            with set (_: Event -> unit) = ()

    type object' with
        member this.onError
            with set (_: Event -> unit) = ()
        member this.onLoad
            with set (_: Event -> unit) = ()

    type link with
        member this.onError
            with set (_: Event -> unit) = ()
        member this.onLoad
            with set (_: Event -> unit) = ()

    type script with
        member this.onError
            with set (_: Event -> unit) = ()
        member this.onLoad
            with set (_: Event -> unit) = ()

    type style with
        member this.onError
            with set (_: Event -> unit) = ()
        member this.onLoad
            with set (_: Event -> unit) = ()

    type body with
        member this.onLoad
            with set (_: Event -> unit) = ()

    type iframe with
        member this.onLoad
            with set (_: Event -> unit) = ()

    type embed with
        member this.onLoad
            with set (_: Event -> unit) = ()

    type audio with
        member this.onError
            with set (_: Event -> unit) = ()
        member this.onPlay
            with set (_: Event -> unit) = ()
        member this.onPause
            with set (_: Event -> unit) = ()
        member this.onEnded
            with set (_: Event -> unit) = ()
        member this.onVolumeChange
            with set (_: Event -> unit) = ()
        member this.onSeeked
            with set (_: Event -> unit) = ()
        member this.onSeeking
            with set (_: Event -> unit) = ()
        member this.onTimeUpdate
            with set (_: Event -> unit) = ()
        member this.onDurationChange
            with set (_: Event -> unit) = ()
        member this.onRateChange
            with set (_: Event -> unit) = ()
        member this.onCanPlay
            with set (_: Event -> unit) = ()
        member this.onCanPlayThrough
            with set (_: Event -> unit) = ()
        member this.onStalled
            with set (_: Event -> unit) = ()
        member this.onWaiting
            with set (_: Event -> unit) = ()
        member this.onEmptied
            with set (_: Event -> unit) = ()
        member this.onLoadedData
            with set (_: Event -> unit) = ()
        member this.onLoadedMetadata
            with set (_: Event -> unit) = ()
        member this.onLoadStart
            with set (_: Event -> unit) = ()
        member this.onProgress
            with set (_: ProgressEvent -> unit) = ()
        member this.onSuspend
            with set (_: Event -> unit) = ()
        member this.onAbort
            with set (_: Event -> unit) = ()

    type video with
        member this.onError
            with set (_: Event -> unit) = ()
        member this.onPlay
            with set (_: Event -> unit) = ()
        member this.onPause
            with set (_: Event -> unit) = ()
        member this.onEnded
            with set (_: Event -> unit) = ()
        member this.onVolumeChange
            with set (_: Event -> unit) = ()
        member this.onSeeked
            with set (_: Event -> unit) = ()
        member this.onSeeking
            with set (_: Event -> unit) = ()
        member this.onTimeUpdate
            with set (_: Event -> unit) = ()
        member this.onDurationChange
            with set (_: Event -> unit) = ()
        member this.onRateChange
            with set (_: Event -> unit) = ()
        member this.onCanPlay
            with set (_: Event -> unit) = ()
        member this.onCanPlayThrough
            with set (_: Event -> unit) = ()
        member this.onStalled
            with set (_: Event -> unit) = ()
        member this.onWaiting
            with set (_: Event -> unit) = ()
        member this.onEmptied
            with set (_: Event -> unit) = ()
        member this.onLoadedData
            with set (_: Event -> unit) = ()
        member this.onLoadedMetadata
            with set (_: Event -> unit) = ()
        member this.onLoadStart
            with set (_: Event -> unit) = ()
        member this.onProgress
            with set (_: ProgressEvent -> unit) = ()
        member this.onSuspend
            with set (_: Event -> unit) = ()
        member this.onAbort
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

    type Suspense() =
        interface HtmlContainer
        member this.fallback
            with set (value: HtmlElement) = ()

    type SuspenseList() =
        interface HtmlContainer
        member this.revealOrder
            with set (value: string) = ()
        member this.tail
            with set (value: string) = ()
        member this.fallback
            with set (value: HtmlElement) = ()

    type Portal() =
        interface HtmlContainer
        member this.mount
            with set (value: Element) = ()
        member this.useShadow
            with set (value: bool) = ()

    type ErrorBoundary() =
        interface HtmlContainer
        member this.fallback
            with set (value: HtmlElement) = ()

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

    [<ImportMember("solid-js/web")>]
    static member renderToString(fn: (unit -> #HtmlElement)) : string = jsNative

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

    [<ImportMember("solid-js/store")>]
    static member reconcile<'T, 'U>(value: 'T) : ('U -> 'T) = jsNative

    [<ImportMember("solid-js/store")>]
    static member produce<'T>(fn: ('T -> unit)) : ('T -> 'T) = jsNative

    [<ImportMember("solid-js/store")>]
    static member unwrap<'T>(item: 'T) : 'T = jsNative

    [<ImportMember("solid-js")>]
    static member batch<'T>(fn: (unit -> 'T)) : 'T = jsNative

    [<ImportMember("solid-js")>]
    static member catchError<'T>(tryFn: (unit -> 'T), onError: (obj -> unit)) : 'T = jsNative

    [<ImportMember("solid-js")>]
    static member onCleanup(fn: (unit -> unit)) : unit = jsNative

    [<ImportMember("solid-js")>]
    static member onMount(fn: (unit -> unit)) : unit = jsNative

    [<ImportMember("solid-js")>]
    static member useTransition() : (unit -> bool) * ((unit -> unit) -> JS.Promise<unit>) = jsNative

    [<ImportMember("solid-js")>]
    static member startTransition() : ((unit -> unit) -> JS.Promise<unit>) = jsNative

    [<ImportMember("solid-js")>]
    static member untrack<'T>(fn: Accessor<'T>) : 'T = jsNative

    /// Component should be decorated by `ExportDefaultAttribute`. Use in combination with `lazy'`.
    [<Emit("import($0)")>]
    static member importComponent(path: string) : JS.Promise<HtmlElement> = jsNative

    /// Component lazy loading. Use in combination with `importComponent`
    [<Import("lazy", "solid-js")>]
    static member lazy'(import: unit -> JS.Promise<HtmlElement>) : HtmlElement = jsNative
