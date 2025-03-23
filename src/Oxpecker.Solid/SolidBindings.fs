namespace Oxpecker.Solid

open System.Runtime.CompilerServices
open Browser.Types
open Fable.Core
open System


[<AutoOpen>]
module Bindings =

    /// <summary>
    /// Calling the setter updates the Signal (triggering dependents to rerun) if the value actually changed.
    /// <br/>The setter takes either the new value for the signal or a function that maps the previous value of the signal to a new value as its only argument. The updated value is also returned by the setter.
    /// </summary>
    /// <remarks>
    /// To pass a handler that maps the previous value, call Invoke on the setter.
    /// <code>
    /// let index,setIndex = createSignal(0)
    /// setIndex.Invoke(fun x -> x + 1)
    /// </code>
    /// To access the returned value, use <c>.InvokeGet</c>
    /// </remarks>
    type Setter<'T> = 'T -> unit
    type Accessor<'T> = unit -> 'T
    type Signal<'T> = Accessor<'T> * Setter<'T>

    /// Solid on* event handlers
    type HtmlTag with
        [<Erase>]
        member this.onClick
            with set (_: MouseEvent -> unit) = ()
        [<Erase>]
        member this.onDblClick
            with set (_: MouseEvent -> unit) = ()
        [<Erase>]
        member this.onBlur
            with set (_: FocusEvent -> unit) = ()
        [<Erase>]
        member this.onFocus
            with set (_: FocusEvent -> unit) = ()
        [<Erase>]
        member this.onContextMenu
            with set (_: MouseEvent -> unit) = ()
        [<Erase>]
        member this.onMouseDown
            with set (_: MouseEvent -> unit) = ()
        [<Erase>]
        member this.onMouseUp
            with set (_: MouseEvent -> unit) = ()
        [<Erase>]
        member this.onMouseEnter
            with set (_: MouseEvent -> unit) = ()
        [<Erase>]
        member this.onMouseLeave
            with set (_: MouseEvent -> unit) = ()
        [<Erase>]
        member this.onMouseOver
            with set (_: MouseEvent -> unit) = ()
        [<Erase>]
        member this.onMouseOut
            with set (_: MouseEvent -> unit) = ()
        [<Erase>]
        member this.onMouseMove
            with set (_: MouseEvent -> unit) = ()
        [<Erase>]
        member this.onWheel
            with set (_: WheelEvent -> unit) = ()
        [<Erase>]
        member this.onKeyDown
            with set (_: KeyboardEvent -> unit) = ()
        [<Erase>]
        member this.onKeyUp
            with set (_: KeyboardEvent -> unit) = ()
        [<Erase>]
        member this.onKeyPress
            with set (_: KeyboardEvent -> unit) = ()
        [<Erase>]
        member this.onDrag
            with set (_: DragEvent -> unit) = ()
        [<Erase>]
        member this.onDragEnd
            with set (_: DragEvent -> unit) = ()
        [<Erase>]
        member this.onDragEnter
            with set (_: DragEvent -> unit) = ()
        [<Erase>]
        member this.onDragLeave
            with set (_: DragEvent -> unit) = ()
        [<Erase>]
        member this.onDragOver
            with set (_: DragEvent -> unit) = ()
        [<Erase>]
        member this.onDragStart
            with set (_: DragEvent -> unit) = ()
        [<Erase>]
        member this.onDrop
            with set (_: DragEvent -> unit) = ()
        [<Erase>]
        member this.onScroll
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onPointerDown
            with set (_: PointerEvent -> unit) = ()
        [<Erase>]
        member this.onPointerMove
            with set (_: PointerEvent -> unit) = ()
        [<Erase>]
        member this.onPointerUp
            with set (_: PointerEvent -> unit) = ()
        [<Erase>]
        member this.onPointerCancel
            with set (_: PointerEvent -> unit) = ()
        [<Erase>]
        member this.onPointerEnter
            with set (_: PointerEvent -> unit) = ()
        [<Erase>]
        member this.onPointerLeave
            with set (_: PointerEvent -> unit) = ()
        [<Erase>]
        member this.onPointerOver
            with set (_: PointerEvent -> unit) = ()
        [<Erase>]
        member this.onPointerOut
            with set (_: PointerEvent -> unit) = ()
        [<Erase>]
        member this.onGotPointerCapture
            with set (_: PointerEvent -> unit) = ()
        [<Erase>]
        member this.onLostPointerCapture
            with set (_: PointerEvent -> unit) = ()
        [<Erase>]
        member this.onAnimationStart
            with set (_: AnimationEvent -> unit) = ()
        [<Erase>]
        member this.onAnimationEnd
            with set (_: AnimationEvent -> unit) = ()
        [<Erase>]
        member this.onAnimationIteration
            with set (_: AnimationEvent -> unit) = ()
        [<Erase>]
        member this.onTransitionEnd
            with set (_: TransitionEvent -> unit) = ()
        [<Erase>]
        member this.onTransitionRun
            with set (_: TransitionEvent -> unit) = ()
        [<Erase>]
        member this.onTransitionStart
            with set (_: TransitionEvent -> unit) = ()
        [<Erase>]
        member this.onTransitionCancel
            with set (_: TransitionEvent -> unit) = ()
        [<Erase>]
        member this.onTouchStart
            with set (_: TouchEvent -> unit) = ()
        [<Erase>]
        member this.onTouchMove
            with set (_: TouchEvent -> unit) = ()
        [<Erase>]
        member this.onTouchEnd
            with set (_: TouchEvent -> unit) = ()
        [<Erase>]
        member this.onTouchCancel
            with set (_: TouchEvent -> unit) = ()
        [<Erase>]
        member this.onCopy
            with set (_: ClipboardEvent -> unit) = ()
        [<Erase>]
        member this.onCut
            with set (_: ClipboardEvent -> unit) = ()
        [<Erase>]
        member this.onPaste
            with set (_: ClipboardEvent -> unit) = ()
        [<Erase>]
        member this.onCompositionStart
            with set (_: CompositionEvent -> unit) = ()
        [<Erase>]
        member this.onCompositionEnd
            with set (_: CompositionEvent -> unit) = ()
        [<Erase>]
        member this.onCompositionUpdate
            with set (_: CompositionEvent -> unit) = ()
        [<Erase>]
        member this.onFocusIn
            with set (_: FocusEvent -> unit) = ()
        [<Erase>]
        member this.onFocusOut
            with set (_: FocusEvent -> unit) = ()
        [<Erase>]
        member this.onEncrypted
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onDragExit
            with set (_: DragEvent -> unit) = ()

    type RegularNode with
        [<Erase>]
        member this.textContent
            with set (value: string) = ()
        [<Erase>]
        member this.innerHTML
            with set (value: string) = ()

    type form with
        [<Erase>]
        member this.onSubmit
            with set (_: SubmitEvent -> unit) = ()
        [<Erase>]
        member this.onReset
            with set (_: Event -> unit) = ()

    type input with
        [<Erase>]
        member this.onChange
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onInvalid
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onInput
            with set (_: InputEvent -> unit) = ()
        [<Erase>]
        member this.onSelect
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type select with
        [<Erase>]
        member this.onChange
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onInput
            with set (_: InputEvent -> unit) = ()

    type textarea with
        [<Erase>]
        member this.onChange
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onInput
            with set (_: InputEvent -> unit) = ()
        [<Erase>]
        member this.onSelect
            with set (_: Event -> unit) = ()

    type details with
        [<Erase>]
        member this.onToggle
            with set (_: ToggleEvent -> unit) = ()

    type img with
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type object' with
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type link with
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type script with
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type style with
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type body with
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type iframe with
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type embed with
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type audio with
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onPlay
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onPause
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onEnded
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onVolumeChange
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onSeeked
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onSeeking
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onTimeUpdate
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onDurationChange
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onRateChange
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onCanPlay
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onCanPlayThrough
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onStalled
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onWaiting
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onEmptied
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onLoadedData
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onLoadedMetadata
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onLoadStart
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onProgress
            with set (_: ProgressEvent -> unit) = ()
        [<Erase>]
        member this.onSuspend
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onAbort
            with set (_: Event -> unit) = ()

    type video with
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onPlay
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onPause
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onEnded
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onVolumeChange
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onSeeked
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onSeeking
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onTimeUpdate
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onDurationChange
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onRateChange
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onCanPlay
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onCanPlayThrough
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onStalled
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onWaiting
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onEmptied
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onLoadedData
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onLoadedMetadata
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onLoadStart
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onProgress
            with set (_: ProgressEvent -> unit) = ()
        [<Erase>]
        member this.onSuspend
            with set (_: Event -> unit) = ()
        [<Erase>]
        member this.onAbort
            with set (_: Event -> unit) = ()

    [<Erase>]
    type For<'T>() =
        interface HtmlElement
        [<Erase>]
        member this.each
            with set (value: 'T[]) = ()
        [<Erase>]
        member inline _.Zero() : HtmlContainerFun = ignore
        [<Erase>]
        member inline _.Yield(value: 'T -> Accessor<int> -> #HtmlElement) : HtmlContainerFun = fun cont -> ignore value

    [<Erase>]
    type Index<'T>() =
        interface HtmlElement
        [<Erase>]
        member this.each
            with set (value: 'T[]) = ()
        [<Erase>]
        member inline _.Zero() : HtmlContainerFun = ignore
        [<Erase>]
        member inline _.Yield(value: Accessor<'T> -> int -> #HtmlElement) : HtmlContainerFun = fun cont -> ignore value

    [<Erase>]
    type Show() =
        interface HtmlContainer
        [<Erase>]
        member this.when'
            with set (value: bool) = ()
        [<Erase>]
        member this.fallback
            with set (value: HtmlElement) = ()
        [<Erase>]
        member this.keyed
            with set (value: bool) = ()

    [<Erase>]
    type Match() =
        interface HtmlContainer
        [<Erase>]
        member this.when'
            with set (value: bool) = ()

    [<Erase>]
    type Switch() =
        interface HtmlElement
        [<Erase>]
        member this.fallback
            with set (value: HtmlElement) = ()
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
        member inline _.Yield(value: Match) : HtmlContainerFun = fun cont -> ignore value

    [<Erase>]
    type Suspense() =
        interface HtmlContainer
        [<Erase>]
        member this.fallback
            with set (value: HtmlElement) = ()

    [<Erase>]
    type SuspenseList() =
        interface HtmlContainer
        [<Erase>]
        member this.revealOrder
            with set (value: string) = ()
        [<Erase>]
        member this.tail
            with set (value: string) = ()
        [<Erase>]
        member this.fallback
            with set (value: HtmlElement) = ()

    [<Erase>]
    type Portal() =
        interface HtmlContainer
        [<Erase>]
        member this.mount
            with set (value: Element) = ()
        [<Erase>]
        member this.useShadow
            with set (value: bool) = ()

    module ErrorBoundary =
        type Fallback = delegate of err: obj * reset: (unit -> unit) -> HtmlElement
    [<Erase>]
    type ErrorBoundary() =
        interface HtmlContainer
        [<Erase>]
        member this.fallback
            with set (value: ErrorBoundary.Fallback) = ()

    [<Erase>]
    type Extensions =

        [<Extension; Erase>]
        static member Run(this: For<'T>, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

        [<Extension; Erase>]
        static member Run(this: Index<'T>, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

        [<Extension; Erase>]
        static member Run(this: Switch, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

        [<Extension; Erase>]
        static member inline Invoke(this: Setter<'T>, handler: 'T -> 'T) : unit = this(unbox<'T> handler)

        [<Extension; Erase>]
        static member inline Invoke(this: Setter<'T>, handler: 'T) : unit = this(unbox<'T> handler)

        [<Extension; Erase>]
        static member inline InvokeAndGet(this: Setter<'T>, handler: 'T -> 'T) : 'T = this(unbox<'T> handler) |> unbox<'T>

        [<Extension; Erase>]
        static member inline InvokeAndGet(this: Setter<'T>, handler: 'T) : 'T = this(unbox<'T> handler) |> unbox<'T>

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

    [<Extension; Erase>]
    type SolidStorePathExtensions =

        /// Select store item by index
        [<Extension; Erase>]
        static member inline Item(this: SolidStorePath<'T, 'Value array>, index: int) =
            SolidStorePath<'T, 'Value>(this.Setter, Array.append this.Path [| index |])

        /// Select store item by predicate
        [<Extension; Erase>]
        static member inline Find(this: SolidStorePath<'T, 'Value array>, predicate: 'Value -> bool) =
            SolidStorePath<'T, 'Value>(this.Setter, Array.append this.Path [| predicate |])


[<AutoOpen>]
[<Erase>]
type Bindings =

    [<ImportMember("solid-js/web")>]
    static member render(code: unit -> #HtmlElement, element: #Element) : unit = jsNative

    [<ImportMember("solid-js/web")>]
    static member renderToString(fn: unit -> #HtmlElement) : string = jsNative

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
    static member produce<'T>(fn: 'T -> unit) : ('T -> 'T) = jsNative

    [<ImportMember("solid-js/store")>]
    static member unwrap<'T>(item: 'T) : 'T = jsNative

    [<ImportMember("solid-js")>]
    static member batch<'T>(fn: unit -> 'T) : 'T = jsNative

    [<ImportMember("solid-js")>]
    static member catchError<'T>(tryFn: unit -> 'T, onError: obj -> unit) : 'T = jsNative

    [<ImportMember("solid-js")>]
    static member onCleanup(fn: unit -> unit) : unit = jsNative

    [<ImportMember("solid-js")>]
    static member onMount(fn: unit -> unit) : unit = jsNative

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
