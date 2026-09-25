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
    /// let value, setValue = createSignal(0)
    /// setValue.Invoke(fun x -> x + 1)
    /// </code>
    /// To access the returned value, use <c>.InvokeAndGet</c>
    /// </remarks>
    type Setter<'T> = 'T -> unit
    /// Function that reads a reactive value and tracks it as a dependency.
    type Accessor<'T> = unit -> 'T
    /// Pair containing a reactive value accessor and its setter.
    type Signal<'T> = Accessor<'T> * Setter<'T>

    /// Solid on* event handlers
    type HtmlTag with
        /// Handles click.
        [<Erase>]
        member this.onClick
            with set (_: MouseEvent -> unit) = ()
        /// Handles double-click.
        [<Erase>]
        member this.onDblClick
            with set (_: MouseEvent -> unit) = ()
        /// Handles blur (focus lost).
        [<Erase>]
        member this.onBlur
            with set (_: FocusEvent -> unit) = ()
        /// Handles focus gained.
        [<Erase>]
        member this.onFocus
            with set (_: FocusEvent -> unit) = ()
        /// Handles context menu.
        [<Erase>]
        member this.onContextMenu
            with set (_: MouseEvent -> unit) = ()
        /// Handles mouse button press.
        [<Erase>]
        member this.onMouseDown
            with set (_: MouseEvent -> unit) = ()
        /// Handles mouse button release.
        [<Erase>]
        member this.onMouseUp
            with set (_: MouseEvent -> unit) = ()
        /// Handles pointer entering the element.
        [<Erase>]
        member this.onMouseEnter
            with set (_: MouseEvent -> unit) = ()
        /// Handles pointer leaving the element.
        [<Erase>]
        member this.onMouseLeave
            with set (_: MouseEvent -> unit) = ()
        /// Handles pointer moving onto the element or a descendant.
        [<Erase>]
        member this.onMouseOver
            with set (_: MouseEvent -> unit) = ()
        /// Handles pointer moving off the element or a descendant.
        [<Erase>]
        member this.onMouseOut
            with set (_: MouseEvent -> unit) = ()
        /// Handles pointer movement.
        [<Erase>]
        member this.onMouseMove
            with set (_: MouseEvent -> unit) = ()
        /// Handles wheel or trackpad scroll.
        [<Erase>]
        member this.onWheel
            with set (_: WheelEvent -> unit) = ()
        /// Handles key press.
        [<Erase>]
        member this.onKeyDown
            with set (_: KeyboardEvent -> unit) = ()
        /// Handles key release.
        [<Erase>]
        member this.onKeyUp
            with set (_: KeyboardEvent -> unit) = ()
        /// Handles character-producing key press; prefer `onKeyDown` for general keyboard handling.
        [<Erase>]
        member this.onKeyPress
            with set (_: KeyboardEvent -> unit) = ()
        /// Handles ongoing drag.
        [<Erase>]
        member this.onDrag
            with set (_: DragEvent -> unit) = ()
        /// Handles drag completion.
        [<Erase>]
        member this.onDragEnd
            with set (_: DragEvent -> unit) = ()
        /// Handles dragged item entering the element.
        [<Erase>]
        member this.onDragEnter
            with set (_: DragEvent -> unit) = ()
        /// Handles dragged item leaving the element.
        [<Erase>]
        member this.onDragLeave
            with set (_: DragEvent -> unit) = ()
        /// Handles dragged item moving over the element.
        [<Erase>]
        member this.onDragOver
            with set (_: DragEvent -> unit) = ()
        /// Handles drag start.
        [<Erase>]
        member this.onDragStart
            with set (_: DragEvent -> unit) = ()
        /// Handles drop on the element.
        [<Erase>]
        member this.onDrop
            with set (_: DragEvent -> unit) = ()
        /// Handles element scrolling.
        [<Erase>]
        member this.onScroll
            with set (_: Event -> unit) = ()
        /// Handles pointer press.
        [<Erase>]
        member this.onPointerDown
            with set (_: PointerEvent -> unit) = ()
        /// Handles pointer movement.
        [<Erase>]
        member this.onPointerMove
            with set (_: PointerEvent -> unit) = ()
        /// Handles pointer release.
        [<Erase>]
        member this.onPointerUp
            with set (_: PointerEvent -> unit) = ()
        /// Handles pointer interaction cancellation.
        [<Erase>]
        member this.onPointerCancel
            with set (_: PointerEvent -> unit) = ()
        /// Handles pointer entering the element.
        [<Erase>]
        member this.onPointerEnter
            with set (_: PointerEvent -> unit) = ()
        /// Handles pointer leaving the element.
        [<Erase>]
        member this.onPointerLeave
            with set (_: PointerEvent -> unit) = ()
        /// Handles pointer moving onto the element or a descendant.
        [<Erase>]
        member this.onPointerOver
            with set (_: PointerEvent -> unit) = ()
        /// Handles pointer moving off the element or a descendant.
        [<Erase>]
        member this.onPointerOut
            with set (_: PointerEvent -> unit) = ()
        /// Handles element receiving pointer capture.
        [<Erase>]
        member this.onGotPointerCapture
            with set (_: PointerEvent -> unit) = ()
        /// Handles element losing pointer capture.
        [<Erase>]
        member this.onLostPointerCapture
            with set (_: PointerEvent -> unit) = ()
        /// Handles CSS animation start.
        [<Erase>]
        member this.onAnimationStart
            with set (_: AnimationEvent -> unit) = ()
        /// Handles CSS animation completion.
        [<Erase>]
        member this.onAnimationEnd
            with set (_: AnimationEvent -> unit) = ()
        /// Handles CSS animation iteration.
        [<Erase>]
        member this.onAnimationIteration
            with set (_: AnimationEvent -> unit) = ()
        /// Handles CSS transition completion.
        [<Erase>]
        member this.onTransitionEnd
            with set (_: TransitionEvent -> unit) = ()
        /// Handles CSS transition creation.
        [<Erase>]
        member this.onTransitionRun
            with set (_: TransitionEvent -> unit) = ()
        /// Handles CSS transition start.
        [<Erase>]
        member this.onTransitionStart
            with set (_: TransitionEvent -> unit) = ()
        /// Handles CSS transition cancellation.
        [<Erase>]
        member this.onTransitionCancel
            with set (_: TransitionEvent -> unit) = ()
        /// Handles touch contact beginning.
        [<Erase>]
        member this.onTouchStart
            with set (_: TouchEvent -> unit) = ()
        /// Handles active touch contact moving.
        [<Erase>]
        member this.onTouchMove
            with set (_: TouchEvent -> unit) = ()
        /// Handles touch contact ending.
        [<Erase>]
        member this.onTouchEnd
            with set (_: TouchEvent -> unit) = ()
        /// Handles touch contact cancellation.
        [<Erase>]
        member this.onTouchCancel
            with set (_: TouchEvent -> unit) = ()
        /// Handles copying selected content.
        [<Erase>]
        member this.onCopy
            with set (_: ClipboardEvent -> unit) = ()
        /// Handles cutting selected content.
        [<Erase>]
        member this.onCut
            with set (_: ClipboardEvent -> unit) = ()
        /// Handles pasted content.
        [<Erase>]
        member this.onPaste
            with set (_: ClipboardEvent -> unit) = ()
        /// Handles input method editor composition start.
        [<Erase>]
        member this.onCompositionStart
            with set (_: CompositionEvent -> unit) = ()
        /// Handles input method editor composition completion.
        [<Erase>]
        member this.onCompositionEnd
            with set (_: CompositionEvent -> unit) = ()
        /// Handles input method editor composition update.
        [<Erase>]
        member this.onCompositionUpdate
            with set (_: CompositionEvent -> unit) = ()
        /// Handles focus entering the element or a descendant.
        [<Erase>]
        member this.onFocusIn
            with set (_: FocusEvent -> unit) = ()
        /// Handles focus leaving the element or a descendant.
        [<Erase>]
        member this.onFocusOut
            with set (_: FocusEvent -> unit) = ()
        /// Handles media encrypted event.
        [<Erase>]
        member this.onEncrypted
            with set (_: Event -> unit) = ()
        /// Handles dragged item leaving the current drop target.
        [<Erase>]
        member this.onDragExit
            with set (_: DragEvent -> unit) = ()

    type RegularNode with
        /// Sets the node text content, replacing any existing children.
        [<Erase>]
        member this.textContent
            with set (value: string) = ()
        /// Sets node markup, replacing its existing children; only use with trusted or sanitized HTML.
        [<Erase>]
        member this.innerHTML
            with set (value: string) = ()

    type form with
        /// Handles form submission.
        [<Erase>]
        member this.onSubmit
            with set (_: SubmitEvent -> unit) = ()
        /// Handles form reset.
        [<Erase>]
        member this.onReset
            with set (_: Event -> unit) = ()

    type input with
        /// Handles committed form control value change.
        [<Erase>]
        member this.onChange
            with set (_: Event -> unit) = ()
        /// Handles constraint validation failure.
        [<Erase>]
        member this.onInvalid
            with set (_: Event -> unit) = ()
        /// Handles form value changing during editing.
        [<Erase>]
        member this.onInput
            with set (_: InputEvent -> unit) = ()
        /// Handles text or control selection.
        [<Erase>]
        member this.onSelect
            with set (_: Event -> unit) = ()
        /// Handles resource or media error.
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        /// Handles resource load completion.
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type select with
        /// Handles committed form control value change.
        [<Erase>]
        member this.onChange
            with set (_: Event -> unit) = ()
        /// Handles form value changing during editing.
        [<Erase>]
        member this.onInput
            with set (_: InputEvent -> unit) = ()

    type textarea with
        /// Handles committed form control value change.
        [<Erase>]
        member this.onChange
            with set (_: Event -> unit) = ()
        /// Handles form value changing during editing.
        [<Erase>]
        member this.onInput
            with set (_: InputEvent -> unit) = ()
        /// Handles text or control selection.
        [<Erase>]
        member this.onSelect
            with set (_: Event -> unit) = ()

    type details with
        /// Handles details or popover open-state change.
        [<Erase>]
        member this.onToggle
            with set (_: ToggleEvent -> unit) = ()

    type img with
        /// Handles resource or media error.
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        /// Handles resource load completion.
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type object' with
        /// Handles resource or media error.
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        /// Handles resource load completion.
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type link with
        /// Handles resource or media error.
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        /// Handles resource load completion.
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type script with
        /// Handles resource or media error.
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        /// Handles resource load completion.
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type style with
        /// Handles resource or media error.
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        /// Handles resource load completion.
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type body with
        /// Handles resource load completion.
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type iframe with
        /// Handles resource load completion.
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type embed with
        /// Handles resource load completion.
        [<Erase>]
        member this.onLoad
            with set (_: Event -> unit) = ()

    type audio with
        /// Handles resource or media error.
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        /// Handles media playback starting or resuming.
        [<Erase>]
        member this.onPlay
            with set (_: Event -> unit) = ()
        /// Handles media playback pausing.
        [<Erase>]
        member this.onPause
            with set (_: Event -> unit) = ()
        /// Handles media playback reaching its end.
        [<Erase>]
        member this.onEnded
            with set (_: Event -> unit) = ()
        /// Handles media volume or muted-state change.
        [<Erase>]
        member this.onVolumeChange
            with set (_: Event -> unit) = ()
        /// Handles media seek completion.
        [<Erase>]
        member this.onSeeked
            with set (_: Event -> unit) = ()
        /// Handles media seek beginning or continuing.
        [<Erase>]
        member this.onSeeking
            with set (_: Event -> unit) = ()
        /// Handles media playback position change.
        [<Erase>]
        member this.onTimeUpdate
            with set (_: Event -> unit) = ()
        /// Handles media duration change.
        [<Erase>]
        member this.onDurationChange
            with set (_: Event -> unit) = ()
        /// Handles media playback rate change.
        [<Erase>]
        member this.onRateChange
            with set (_: Event -> unit) = ()
        /// Handles media becoming ready to play.
        [<Erase>]
        member this.onCanPlay
            with set (_: Event -> unit) = ()
        /// Handles media likely to play through without buffering.
        [<Erase>]
        member this.onCanPlayThrough
            with set (_: Event -> unit) = ()
        /// Handles media data unexpectedly stopping.
        [<Erase>]
        member this.onStalled
            with set (_: Event -> unit) = ()
        /// Handles playback waiting for more data.
        [<Erase>]
        member this.onWaiting
            with set (_: Event -> unit) = ()
        /// Handles media element becoming empty.
        [<Erase>]
        member this.onEmptied
            with set (_: Event -> unit) = ()
        /// Handles current media frame finishing loading.
        [<Erase>]
        member this.onLoadedData
            with set (_: Event -> unit) = ()
        /// Handles media metadata finishing loading.
        [<Erase>]
        member this.onLoadedMetadata
            with set (_: Event -> unit) = ()
        /// Handles media loading beginning.
        [<Erase>]
        member this.onLoadStart
            with set (_: Event -> unit) = ()
        /// Handles media data download progress.
        [<Erase>]
        member this.onProgress
            with set (_: ProgressEvent -> unit) = ()
        /// Handles media loading being intentionally suspended.
        [<Erase>]
        member this.onSuspend
            with set (_: Event -> unit) = ()
        /// Handles media loading being aborted.
        [<Erase>]
        member this.onAbort
            with set (_: Event -> unit) = ()

    type video with
        /// Handles resource or media error.
        [<Erase>]
        member this.onError
            with set (_: Event -> unit) = ()
        /// Handles media playback starting or resuming.
        [<Erase>]
        member this.onPlay
            with set (_: Event -> unit) = ()
        /// Handles media playback pausing.
        [<Erase>]
        member this.onPause
            with set (_: Event -> unit) = ()
        /// Handles media playback reaching its end.
        [<Erase>]
        member this.onEnded
            with set (_: Event -> unit) = ()
        /// Handles media volume or muted-state change.
        [<Erase>]
        member this.onVolumeChange
            with set (_: Event -> unit) = ()
        /// Handles media seek completion.
        [<Erase>]
        member this.onSeeked
            with set (_: Event -> unit) = ()
        /// Handles media seek beginning or continuing.
        [<Erase>]
        member this.onSeeking
            with set (_: Event -> unit) = ()
        /// Handles media playback position change.
        [<Erase>]
        member this.onTimeUpdate
            with set (_: Event -> unit) = ()
        /// Handles media duration change.
        [<Erase>]
        member this.onDurationChange
            with set (_: Event -> unit) = ()
        /// Handles media playback rate change.
        [<Erase>]
        member this.onRateChange
            with set (_: Event -> unit) = ()
        /// Handles media becoming ready to play.
        [<Erase>]
        member this.onCanPlay
            with set (_: Event -> unit) = ()
        /// Handles media likely to play through without buffering.
        [<Erase>]
        member this.onCanPlayThrough
            with set (_: Event -> unit) = ()
        /// Handles media data unexpectedly stopping.
        [<Erase>]
        member this.onStalled
            with set (_: Event -> unit) = ()
        /// Handles playback waiting for more data.
        [<Erase>]
        member this.onWaiting
            with set (_: Event -> unit) = ()
        /// Handles media element becoming empty.
        [<Erase>]
        member this.onEmptied
            with set (_: Event -> unit) = ()
        /// Handles current media frame finishing loading.
        [<Erase>]
        member this.onLoadedData
            with set (_: Event -> unit) = ()
        /// Handles media metadata finishing loading.
        [<Erase>]
        member this.onLoadedMetadata
            with set (_: Event -> unit) = ()
        /// Handles media loading beginning.
        [<Erase>]
        member this.onLoadStart
            with set (_: Event -> unit) = ()
        /// Handles media data download progress.
        [<Erase>]
        member this.onProgress
            with set (_: ProgressEvent -> unit) = ()
        /// Handles media loading being intentionally suspended.
        [<Erase>]
        member this.onSuspend
            with set (_: Event -> unit) = ()
        /// Handles media loading being aborted.
        [<Erase>]
        member this.onAbort
            with set (_: Event -> unit) = ()

    /// Renders a list of items using keyed reconciliation.
    [<Erase>]
    type For<'T>() =
        interface HtmlElement
        /// Items to render as a list; each item is passed to the row function with its index accessor.
        [<Erase>]
        member this.each
            with set (value: 'T[]) = ()
        [<Erase>]
        member inline _.Zero() : HtmlContainerFun = ignore
        [<Erase>]
        member inline _.Yield(value: 'T -> Accessor<int> -> #HtmlElement) : HtmlContainerFun = fun cont -> ignore value

    /// Renders a list by position while preserving each row accessor across item changes.
    [<Erase>]
    type Index<'T>() =
        interface HtmlElement
        /// Items to render by position; the row function receives an item accessor and a fixed integer index.
        [<Erase>]
        member this.each
            with set (value: 'T[]) = ()
        [<Erase>]
        member inline _.Zero() : HtmlContainerFun = ignore
        [<Erase>]
        member inline _.Yield(value: Accessor<'T> -> int -> #HtmlElement) : HtmlContainerFun = fun cont -> ignore value

    /// Conditionally renders children when its condition is truthy, with an optional fallback.
    [<Erase>]
    type Show() =
        interface HtmlContainer
        /// Condition that determines whether the branch is rendered.
        [<Erase>]
        member this.when'
            with set (value: bool) = ()
        /// Content rendered when a condition is false or an async boundary has no primary content.
        [<Erase>]
        member this.fallback
            with set (value: HtmlElement) = ()
        /// Whether a Show condition is compared by value and passed as a stable keyed value.
        [<Erase>]
        member this.keyed
            with set (value: bool) = ()

    /// Defines one condition and its child branch inside a `Switch`.
    [<Erase>]
    type Match() =
        interface HtmlContainer
        /// Condition that determines whether the branch is rendered.
        [<Erase>]
        member this.when'
            with set (value: bool) = ()

    /// Renders the first matching `Match` branch, or its fallback.
    [<Erase>]
    type Switch() =
        interface HtmlElement
        /// Content rendered when a condition is false or an async boundary has no primary content.
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

    /// Shows fallback content while a descendant resource is pending.
    [<Erase>]
    type Suspense() =
        interface HtmlContainer
        /// Content rendered when a condition is false or an async boundary has no primary content.
        [<Erase>]
        member this.fallback
            with set (value: HtmlElement) = ()

    /// Coordinates the reveal order of multiple Suspense boundaries.
    [<Erase>]
    type SuspenseList() =
        interface HtmlContainer
        /// SuspenseList reveal order: `forwards`, `backwards`, or `together`.
        [<Erase>]
        member this.revealOrder
            with set (value: string) = ()
        /// How unrevealed SuspenseList fallbacks are displayed: `collapsed` or `hidden`.
        [<Erase>]
        member this.tail
            with set (value: string) = ()
        /// Content rendered when a condition is false or an async boundary has no primary content.
        [<Erase>]
        member this.fallback
            with set (value: HtmlElement) = ()

    /// Renders children into a separate DOM mount point.
    [<Erase>]
    type Portal() =
        interface HtmlContainer
        /// DOM element where a Portal mounts its children.
        [<Erase>]
        member this.mount
            with set (value: Element) = ()
        /// Whether a Portal should render into a shadow root.
        [<Erase>]
        member this.useShadow
            with set (value: bool) = ()

    module ErrorBoundary =
        type Fallback = delegate of err: obj * reset: (unit -> unit) -> HtmlElement
    /// Catches errors from descendant components and renders a recovery fallback.
    [<Erase>]
    type ErrorBoundary() =
        interface HtmlContainer
        /// Fallback rendered after a descendant throws; receives the error and a reset callback.
        [<Erase>]
        member this.fallback
            with set (value: ErrorBoundary.Fallback) = ()

    [<Erase>]
    type Extensions =

        /// Runs the child builder expression for the control-flow component.
        [<Extension; Erase>]
        static member Run(this: For<'T>, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

        /// Runs the child builder expression for the control-flow component.
        [<Extension; Erase>]
        static member Run(this: Index<'T>, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

        /// Runs the child builder expression for the control-flow component.
        [<Extension; Erase>]
        static member Run(this: Switch, runExpr: HtmlContainerFun) =
            runExpr Unchecked.defaultof<_>
            this

        /// Calls a signal setter with a replacement value or an updater function.
        [<Extension; Erase>]
        static member inline Invoke(this: Setter<'T>, handler: 'T -> 'T) : unit = this(unbox<'T> handler)

        /// Calls a signal setter with a replacement value or an updater function.
        [<Extension; Erase>]
        static member inline Invoke(this: Setter<'T>, handler: 'T) : unit = this(unbox<'T> handler)

        /// Calls a signal setter and returns the resulting value.
        [<Extension; Erase>]
        static member inline InvokeAndGet(this: Setter<'T>, handler: 'T -> 'T) : 'T =
            this(unbox<'T> handler) |> unbox<'T>

        /// Calls a signal setter and returns the resulting value.
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

    /// Reactive state and values returned by an asynchronous resource.
    type SolidResource<'T> =
        /// Attention, will be undefined while loading
        [<Emit("$0()")>]
        abstract current: 'T
        /// Current resource state: unresolved, pending, ready, refreshing, or errored.
        abstract state: SolidResourceState
        /// Whether the resource is currently loading or refreshing.
        abstract loading: bool
        /// Fetcher error, if the resource is in the errored state.
        abstract error: exn option
        /// Unlike `current`, it keeps the latest value while re-loading
        /// Attention, will be undefined until first value has been loaded
        abstract latest: 'T

    /// Operations for mutating or refetching an asynchronous resource.
    type SolidResourceManager<'T> =
        /// Directly updates the resource value without running the fetcher.
        abstract mutate: 'T -> 'T
        /// Runs the resource fetcher again and returns a promise for the new value.
        abstract refetch: unit -> JS.Promise<'T>

    /// Functions for updating a Solid store, including nested paths.
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

    /// A typed path to a nested value in a Solid store.
    type SolidStorePath<'T, 'Value>(setter: SolidStoreSetter<'T>, path: obj[]) =
        /// Setter used to update the store value at this path.
        member _.Setter = setter
        /// Property path identifying the nested store value.
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

    /// Convenience selectors for addressing values in a Solid store.
    [<Extension; Erase>]
    type SolidStorePathExtensions =

        /// Selects a store array entry by its zero-based index.
        [<Extension; Erase>]
        static member inline Item(this: SolidStorePath<'T, 'Value array>, index: int) =
            SolidStorePath<'T, 'Value>(this.Setter, Array.append this.Path [| index |])

        /// Selects the first store array entry matching a predicate.
        [<Extension; Erase>]
        static member inline Find(this: SolidStorePath<'T, 'Value array>, predicate: 'Value -> bool) =
            SolidStorePath<'T, 'Value>(this.Setter, Array.append this.Path [| predicate |])


[<AutoOpen>]
[<Erase>]
type Bindings =

    /// Mounts a Solid view into a DOM element and manages its reactive lifecycle.
    [<ImportMember("solid-js/web")>]
    static member render(code: unit -> #HtmlElement, element: #Element) : unit = jsNative

    /// Renders a Solid view to an HTML string for server-side rendering.
    [<ImportMember("solid-js/web")>]
    static member renderToString(fn: unit -> #HtmlElement) : string = jsNative

    /// Creates a reactive value and its setter; dependents rerun when the value changes.
    [<ImportMember("solid-js"); ParamObject(1)>]
    static member createSignal
        (value: 'T, ?equals: ('T -> 'T -> bool), ?name: string, ?``internal``: bool)
        : Signal<'T> =
        jsNative
    /// Creates a reactive value and its setter; dependents rerun when the value changes.
    [<ImportMember("solid-js")>]
    static member createSignal(value: 'T) : Signal<'T> = jsNative

    /// Creates a cached derived value that updates when tracked dependencies change.
    [<ImportMember("solid-js")>]
    static member createMemo(value: unit -> 'T) : (unit -> 'T) = jsNative

    /// Runs a side effect reactively after its tracked dependencies change.
    [<ImportMember("solid-js")>]
    static member createEffect(effect: unit -> unit) : unit = jsNative

    /// Runs a side effect reactively after its tracked dependencies change.
    [<ImportMember("solid-js")>]
    static member createEffect(effect: 'T -> 'T, initialValue: 'T) : unit = jsNative

    /// Fetcher will be called immediately
    /// Creates an async resource and a manager; the fetcher starts immediately or when its optional source becomes defined.
    [<ImportMember("solid-js"); ParamObject(fromIndex = 1)>]
    static member createResource
        (fetcher: unit -> JS.Promise<'T>, ?initialValue: 'T)
        : SolidResource<'T> * SolidResourceManager<'T> =
        jsNative

    /// Fetcher will be called only when source signal returns `Some('U)`
    /// Creates an async resource and a manager; the fetcher starts immediately or when its optional source becomes defined.
    [<ImportMember("solid-js"); ParamObject(fromIndex = 2)>]
    static member createResource
        (source: unit -> 'U option, fetcher: 'U -> JS.Promise<'T>, ?initialValue: 'T)
        : SolidResource<'T> * SolidResourceManager<'T> =
        jsNative

    /// Creates a reactive owner scope and passes its disposer to the callback.
    [<ImportMember("solid-js")>]
    static member createRoot(fn (* dispose *) : Action -> 'T) : 'T = jsNative

    /// Creates a stable unique identifier within the current Solid owner.
    [<ImportMember("solid-js")>]
    static member createUniqueId() : string = jsNative

    /// Creates a deeply reactive store and a setter for updating it.
    [<ImportMember("solid-js/store")>]
    static member createStore(store: 'T) : 'T * SolidStoreSetter<'T> = jsNative

    /// Creates an updater that reconciles incoming data with existing store state.
    [<ImportMember("solid-js/store")>]
    static member reconcile<'T, 'U>(value: 'T) : ('U -> 'T) = jsNative

    /// Creates an updater that mutates a draft while preserving immutable store updates.
    [<ImportMember("solid-js/store")>]
    static member produce<'T>(fn: 'T -> unit) : ('T -> 'T) = jsNative

    /// Returns the underlying non-proxy value of a store object.
    [<ImportMember("solid-js/store")>]
    static member unwrap<'T>(item: 'T) : 'T = jsNative

    /// Batches reactive updates so dependents run after the batch completes.
    [<ImportMember("solid-js")>]
    static member batch<'T>(fn: unit -> 'T) : 'T = jsNative

    /// Runs a callback and forwards errors from reactive descendants to an error handler.
    [<ImportMember("solid-js")>]
    static member catchError<'T>(tryFn: unit -> 'T, onError: obj -> unit) : 'T = jsNative

    /// Registers a callback to run when the current reactive owner is disposed.
    [<ImportMember("solid-js")>]
    static member onCleanup(fn: unit -> unit) : unit = jsNative

    /// Registers a callback to run once after the component is mounted in the DOM.
    [<ImportMember("solid-js")>]
    static member onMount(fn: unit -> unit) : unit = jsNative

    /// Returns transition-pending state and a function for scheduling a transition.
    [<ImportMember("solid-js")>]
    static member useTransition() : (unit -> bool) * ((unit -> unit) -> JS.Promise<unit>) = jsNative

    /// Schedules reactive updates as a transition and returns a promise for completion.
    [<ImportMember("solid-js")>]
    static member startTransition() : ((unit -> unit) -> JS.Promise<unit>) = jsNative

    /// Evaluates a function without tracking signals it reads as dependencies.
    [<ImportMember("solid-js")>]
    static member untrack<'T>(fn: Accessor<'T>) : 'T = jsNative

    /// Component should be decorated by `ExportDefaultAttribute`. Use in combination with `lazy'`.
    /// Dynamically imports a component module by path.
    [<Emit("import($0)")>]
    static member importComponent(path: string) : JS.Promise<HtmlElement> = jsNative

    /// Component lazy loading. Use in combination with `importComponent`
    /// Creates a component that loads its implementation on demand.
    [<Import("lazy", "solid-js")>]
    static member lazy'(import: unit -> JS.Promise<HtmlElement>) : HtmlElement = jsNative
    /// <summary>
    /// <c>createComputed</c> creates a new computation that immediately runs the given function in a tracking,
    /// thus automatically tracking its dependencies, and automatically reruns the function whenever the dependencies
    /// changes. The function gets called with an argument equal to the value returned from the function's last
    /// execution, or on the first call, equal to the optional second argument. Note that the return value of the
    /// function is not otherwise exposed; in particular, createComputed has no return value.<br/><br/>
    /// <c>createComputed</c> is the most immediate form of reactivity in Solid, and is most useful for building
    /// other reactive primitives. For example, some other Solid primitives are built from <c>createComputed</c>.
    /// However, it should be used with care, as <c>createComputed</c> can easily cause more unnecessary updates
    /// than other reactive primitives. Before using it, consider the closely related primitives <c>createMemo</c>
    /// and <c>createRenderEffect</c>.
    /// </summary>
    /// <param name="fn">The function to run in a tracking scope.</param>
    /// <param name="value">The initial value to pass to the function.</param>
    /// Creates an immediately running tracked computation; consider memo or effect primitives when appropriate.
    [<ImportMember("solid-js")>]
    static member createComputed<'T>(fn: 'T -> 'T, ?value: 'T) : unit = jsNative
    /// <summary>
    /// Creates a readonly that only notifies downstream changes when the browser is idle. <c>timeoutMs</c> is the
    /// maximum time to wait before forcing the update.
    /// </summary>
    /// Returns a deferred signal that updates when the browser is idle, subject to an optional timeout.
    [<ImportMember("solid-js"); ParamObject(1)>]
    static member createDeferred<'T>
        (source: unit -> 'T, ?timeoutMs: int, ?equals: 'T -> 'T -> bool, ?name: string)
        : unit -> 'T =
        jsNative
    /// <summary>
    /// Creates a readonly that only notifies downstream changes when the browser is idle. <c>timeoutMs</c> is the
    /// maximum time to wait before forcing the update.
    /// </summary>
    /// Returns a deferred signal that updates when the browser is idle, subject to an optional timeout.
    [<ImportMember("solid-js")>]
    static member createDeferred<'T>(source: unit -> 'T) : unit -> 'T = jsNative
    /// <summary>
    /// Sometimes it is useful to separate tracking from re-execution. This primitive registers a side-effect
    /// that is run the first time the expression wrapped by the returned tracking is notified of a change.
    /// </summary>
    /// Creates a one-shot invalidation callback that must be re-armed by invoking the returned function.
    [<ImportMember("solid-js")>]
    static member createReaction(onInvalidate: unit -> unit) : (unit -> unit) -> unit = jsNative
    /// <summary>
    /// A render effect is a computation similar to a regular effect, but differs in when Solid schedules
    /// the first execution of the effect function. While createEffect waits for the current rendering
    /// phase to be complete, createRenderEffect immediately calls the function. Thus the effect runs as
    /// DOM elements are being created and updated, but possibly before specific elements of interest have
    /// been created, and probably before those elements have been connected to the document. In particular, refs
    /// will not be set before the initial effect call. Indeed, Solid uses <c>createRenderEffect</c> to implement
    /// the rendering phase of itself, including setting of <b>refs</b>
    /// </summary>
    /// Creates an effect that runs during rendering, before the DOM is necessarily connected.
    [<ImportMember("solid-js")>]
    static member createRenderEffect<'T>(fn: 'T -> 'T, ?value: 'T) : unit = jsNative
    /// <summary>
    /// Creates a parameterised derived boolean signal <c>selector(key)</c> that indicates whether <c>key</c>
    /// is equal to the current value of the <c>source</c> signal. These signals are optimised to notify
    /// each subscriber only when their <c>key</c> starts or stops matching the reactive <c>source</c> value
    /// (instead of every time <c>key</c> changes). If you have <i>n</i> different subscribers with different
    /// keys, and the <c>source</c> value changes from <c>a</c> to <c>b</c>, then instead of all <i>n</i> subscribers
    /// updating, at most two subscribers will update: the signal with key <c>a</c> will change to <c>false</c>, and
    /// the signal with key <c>b</c> will change to <c>true</c>. Thus it reduces from <i>n</i> updates to 2 updates.<br/>
    /// <br/>Useful for defining the selection state of several selectable elements.
    /// </summary>
    /// <param name="source">The source signal to get the value from and compare with keys.</param>
    /// <param name="fn">A function to compare the key and the value, returning whether they should be treated as equal. Default: <c>=</c></param>
    /// Creates a keyed selector that only notifies when a key starts or stops matching the source.
    [<ImportMember("solid-js")>]
    static member createSelector<'T, 'U>(source: unit -> 'T, ?fn: 'U -> 'T -> bool) : 'U -> bool = jsNative
