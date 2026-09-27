namespace Oxpecker.Rocket

open Fable.Core
open Fable.Core.JsInterop
open Oxpecker.ViewEngine

/// Where a component renders. Rocket defaults to an open shadow root.
[<StringEnum; RequireQualifiedAccess>]
type RocketMode =
    | [<CompiledName "light">] Light
    | [<CompiledName "open">] Open
    | [<CompiledName "closed">] Closed

/// Common codec operations. Values arriving through HTML attributes are decoded by Rocket.
type Codec<'T> =
    abstract decode: value: obj -> 'T
    abstract encode: value: 'T -> string

/// A codec with a typed default value or factory.
type ValueCodec<'T> =
    inherit Codec<'T>
    [<Emit("$0.default($1)")>]
    abstract withDefault: value: 'T -> ValueCodec<'T>
    [<Emit("$0.default($1)")>]
    abstract withDefaultFactory: factory: (unit -> 'T) -> ValueCodec<'T>

/// String-only transforms; every transform returns another string codec.
type StringCodec =
    inherit Codec<string>
    abstract trim: StringCodec
    abstract upper: StringCodec
    abstract lower: StringCodec
    abstract kebab: StringCodec
    abstract camel: StringCodec
    abstract snake: StringCodec
    abstract pascal: StringCodec
    abstract title: StringCodec
    abstract prefix: value: string -> StringCodec
    abstract suffix: value: string -> StringCodec
    abstract maxLength: length: int -> StringCodec
    [<Emit("$0.default($1)")>]
    abstract withDefault: value: string -> StringCodec
    [<Emit("$0.default($1)")>]
    abstract withDefaultFactory: factory: (unit -> string) -> StringCodec

/// Number-only transforms. Decimal-place counts are integers, values are JavaScript numbers.
type NumberCodec =
    inherit Codec<float>
    abstract min: value: float -> NumberCodec
    abstract max: value: float -> NumberCodec
    abstract clamp: minimum: float * maximum: float -> NumberCodec
    abstract step: step: float * ?basis: float -> NumberCodec
    abstract round: NumberCodec
    abstract ceil: ?decimals: int -> NumberCodec
    abstract floor: ?decimals: int -> NumberCodec
    abstract fit:
        inMin: float * inMax: float * outMin: float * outMax: float * ?clamped: bool * ?rounded: bool -> NumberCodec
    [<Emit("$0.default($1)")>]
    abstract withDefault: value: float -> NumberCodec
    [<Emit("$0.default($1)")>]
    abstract withDefaultFactory: factory: (unit -> float) -> NumberCodec

/// Registry supplied to the Props callback. Structured prop shapes remain dynamic in this initial API.
type CodecRegistry =
    abstract string: StringCodec
    abstract number: NumberCodec
    abstract bool: ValueCodec<bool>
    abstract date: ValueCodec<System.DateTime>
    abstract json: ValueCodec<obj>
    abstract js: ValueCodec<obj>
    abstract bin: ValueCodec<JS.Uint8Array>

/// Context for a component-local action with no trailing arguments.
type RocketActionContext =
    abstract host: obj
    abstract props: obj
    abstract state: obj
    abstract el: obj | null
    abstract evt: obj | null

/// Per-connection setup. Use dynamic access for props and local signals, e.g. ctx.``$$``?count.
type SetupContext =
    abstract props: obj
    abstract ``$$``: obj
    abstract ``$``: obj
    abstract host: obj
    abstract cleanup: callback: (unit -> unit) -> unit
    /// Returns a function that stops the effect early; Rocket also tracks disconnect cleanup.
    abstract effect: callback: (unit -> unit) -> (unit -> unit)
    abstract action: name: string * handler: (RocketActionContext -> unit) -> unit
    /// Watch every prop. The returned function unsubscribes.
    abstract observeProps: callback: (unit -> unit) -> (unit -> unit)
    /// Watch only named props. The returned function unsubscribes.
    [<Emit("$0.observeProps($1, ...$2)")>]
    abstract observeProps: callback: (unit -> unit) * propNames: string array -> (unit -> unit)

/// Runs after the initial render, when data-ref elements are available.
type FirstRenderContext =
    inherit SetupContext
    abstract refs: obj

/// Context supplied by Rocket to its render callback.
type RenderContext =
    abstract html: obj
    abstract svg: obj
    abstract props: obj
    abstract host: obj

/// Component options, using the same object-initializer style as the Oxpecker DSL.
type RocketDefinition() =
    member val Mode = RocketMode.Open with get, set
    member val Props: CodecRegistry -> obj = Unchecked.defaultof<_> with get, set
    member val Setup: SetupContext -> unit = Unchecked.defaultof<_> with get, set
    member val OnFirstRender: FirstRenderContext -> unit = Unchecked.defaultof<_> with get, set
    member val Render: RenderContext -> obj = Unchecked.defaultof<_> with get, set
    member val RenderOnPropChange = true with get, set

    /// Produces a JavaScript options object, omitting unset callbacks.
    member this.ToJs() : obj =
        let fields = ResizeArray<string * obj>()
        let addCallback name (value: objnull) =
            match value with
            | null -> ()
            | value -> fields.Add(name, value)
        addCallback "mode" (box this.Mode)
        addCallback "props" (box this.Props)
        addCallback "setup" (box this.Setup)
        addCallback "onFirstRender" (box this.OnFirstRender)
        addCallback "render" (box this.Render)
        if not this.RenderOnPropChange then
            addCallback "renderOnPropChange" (box false)
        createObj fields

[<RequireQualifiedAccess>]
module Rocket =
    // Resolve this bare specifier with the consuming application's import map or bundler.
    [<Import("rocket", "datastar-rocket")>]
    let private registerJs (tag: string) (options: obj) : obj = jsNative

    /// Registers a custom element. Rocket ignores repeated registrations of the same tag.
    let register (tag: string) (definition: RocketDefinition) : unit =
        registerJs tag (definition.ToJs()) |> ignore

    // Construct both cooked and raw arrays, as required by TemplateStringsArray. Each call
    // uses a fresh array: markup can change between renders, unlike a JS template call site.
    [<Emit("((ctx, markup) => { const strings = [markup]; Object.defineProperty(strings, 'raw', { value: Object.freeze([markup]) }); return ctx.html(Object.freeze(strings)); })($0, $1)")>]
    let private markupToTemplate (ctx: RenderContext) (markup: string) : obj = jsNative

    /// Renders the shared DSL through Rocket's template/scoping pass, not as a text node.
    /// Ordinary text and attribute values are HTML-escaped; raw markup and JS expressions are trusted code.
    let view (ctx: RenderContext) (element: #HtmlElement) : obj =
        markupToTemplate ctx (Render.toString element)

    /// A homogeneous array codec.
    [<Emit("$0.array($1)")>]
    let arrayOf (codecs: CodecRegistry) (codec: Codec<'T>) : ValueCodec<'T array> = jsNative

    /// An object codec. Supply a JavaScript object mapping keys to codecs.
    [<Emit("$0.object($1)")>]
    let objectOf (codecs: CodecRegistry) (shape: obj) : ValueCodec<obj> = jsNative

    /// A codec constrained to string literals.
    [<Emit("$0.oneOf(...$1)")>]
    let oneOf (codecs: CodecRegistry) (values: string array) : ValueCodec<string> = jsNative
