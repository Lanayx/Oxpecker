module Rocket.BindingTests

open Fable.Core
open Fable.Core.JsInterop
open Oxpecker.ViewEngine
open Oxpecker.Datastar
open Oxpecker.Rocket

// Test-only exports supplied by the Node contract-test harness.
[<Import("getRegistration", "datastar-rocket")>]
let private registration (tag: string) : obj = jsNative
[<Import("renderContext", "datastar-rocket")>]
let private renderContext () : RenderContext = jsNative
[<Import("setupContext", "datastar-rocket")>]
let private setupContext () : FirstRenderContext = jsNative
[<Import("codecRegistry", "datastar-rocket")>]
let private codecRegistry () : CodecRegistry = jsNative
[<Emit("Object.keys($0).sort().join(',')")>]
let private keys (value: obj) : string = jsNative
[<Emit("JSON.stringify($0)")>]
let private json (value: obj) : string = jsNative

let private equal expected actual =
    if expected <> actual then
        failwithf "Expected %A, got %A" expected actual

let private defaults () =
    for mode, expected in
        [
            RocketMode.Open, "open"
            RocketMode.Closed, "closed"
            RocketMode.Light, "light"
        ] do
        Rocket.register "test-defaults" (RocketDefinition(Mode = mode))
        let options = registration "test-defaults"
        equal "mode" (keys options)
        equal expected (options?mode: string)
    let options = RocketDefinition(RenderOnPropChange = false).ToJs()
    equal false (options?renderOnPropChange: bool)

let private lifecycle () =
    let mutable firstRendered = false
    let definition =
        RocketDefinition(
            Props = (fun codecs -> createObj [ "start" ==> codecs.number.min(0.0).withDefault(1.0) ]),
            Setup = (fun ctx -> ctx.``$$``?count <- ctx.props?start),
            OnFirstRender = (fun _ -> firstRendered <- true),
            Render = (fun ctx -> Rocket.view ctx (output().dataText("$$count") { "0" }))
        )
    Rocket.register "test-counter" definition
    let options = registration "test-counter"
    equal "mode,onFirstRender,props,render,setup" (keys options)
    let ctx = setupContext()
    let setup: SetupContext -> unit = options?setup
    setup ctx
    equal 3 (ctx.``$$``?count: int)
    let first: FirstRenderContext -> unit = options?onFirstRender
    first ctx
    equal true firstRendered
    let props: CodecRegistry -> obj = options?props
    equal "start" (keys(props(codecRegistry())))
    let render: RenderContext -> obj = options?render
    equal "<output data-text=\"$$count\">0</output>" ((render(renderContext()))?markup: string)
    let mutable effects = 0
    let stop = ctx.effect(fun () -> effects <- effects + 1)
    stop()
    let unsubscribe = ctx.observeProps((fun () -> effects <- effects + 1), [| "start"; "label" |])
    equal "[\"start\",\"label\"]" (json ctx?names)
    unsubscribe()
    let unsubscribeAll = ctx.observeProps(fun () -> effects <- effects + 1)
    equal "[]" (json ctx?names)
    unsubscribeAll()
    equal 3 effects
    equal 3 (ctx?stopped: int)
    ctx.cleanup(fun () -> effects <- effects + 1)
    equal 4 effects
    ctx.action("reset", fun action -> action.state?count <- 0)
    equal 0 (ctx.``$$``?count: int)
    equal "reset" (ctx?actionName: string)

let private bridge () =
    let ctx = renderContext()
    for text in [ "<&\"'"; "changed"; "$$literal @action() ${value}" ] do
        let element = div(title = text) { text }
        let result = Rocket.view ctx element
        equal (Render.toString element) (result?markup: string)

let private codecs () =
    let registry = codecRegistry()
    registry.number.min(0.0).max(10.0).step(2.0, 1.0).ceil(2).floor().round.withDefault(3.0)
    |> ignore
    registry.string.trim.upper.prefix("a").suffix("z").maxLength(12).withDefault("x").lower
    |> ignore
    registry.bool.withDefault(false) |> ignore
    registry.json.withDefaultFactory(fun () -> createObj []) |> ignore
    Rocket.oneOf registry [| "small"; "large" |] |> ignore
    equal
        "[[\"number\",\"min\",0],[\"number\",\"max\",10],[\"number\",\"step\",2,1],[\"number\",\"ceil\",2],[\"number\",\"floor\"],[\"number\",\"round\"],[\"number\",\"default\",3],[\"string\",\"trim\"],[\"string\",\"upper\"],[\"string\",\"prefix\",\"a\"],[\"string\",\"suffix\",\"z\"],[\"string\",\"maxLength\",12],[\"string\",\"default\",\"x\"],[\"string\",\"lower\"],[\"bool\",\"default\",false],[\"json\",\"default\",{}],[\"oneOf\",\"small\",\"large\"]]"
        (json registry?calls)
    let arrays = codecRegistry()
    Rocket.arrayOf arrays arrays.string |> ignore
    let calls: obj array array = arrays?calls
    equal "array" (unbox<string>(calls[0][0]))
    Rocket.objectOf arrays (createObj [ "name" ==> arrays.string ]) |> ignore
    equal "object" (unbox<string>(calls[1][0]))

let private directives () =
    let cases = [
        template().dataIf("$$count < 1") { }, "<template data-if=\"$$count &lt; 1\"></template>"
        template().dataElseIf("$$ready") { }, "<template data-else-if=\"$$ready\"></template>"
        template().dataElse() { }, "<template data-else></template>"
        template().dataFor("item, index in $$items") { }, "<template data-for=\"item, index in $$items\"></template>"
        template().dataIf(null) { }, "<template></template>"
    ]
    for view, expected in cases do
        equal expected (Render.toString view)
    equal "<input data-bind:name__root>" (Render.toString(input().dataBind("name", RocketModifier.root)))

let run () =
    let failures = ResizeArray<string>()
    for name, test in
        [
            "defaults", defaults
            "lifecycle", lifecycle
            "bridge", bridge
            "codecs", codecs
            "directives", directives
        ] do
        try
            test()
        with ex ->
            failures.Add($"{name}: {ex.Message}")
    if failures.Count > 0 then
        failwith(String.concat "\n" failures)
    printfn "Passed 5 Rocket interop contract groups (stub runtime)."
