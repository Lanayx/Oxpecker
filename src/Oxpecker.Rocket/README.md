# Oxpecker.Rocket

This work-in-progress Fable project combines browser rendering, shared Datastar
attributes, and initial bindings for authoring Rocket components in F#.

The binding signatures target the
[Rocket v1.0.4 declarations](https://cdn.jsdelivr.net/gh/starfederation/datastar@v1.0.4/bundles/datastar-rocket.d.ts).
Rocket is beta; pin the matching runtime while developing against these bindings.

The linked sources retain the `Oxpecker.ViewEngine` and `Oxpecker.Datastar`
namespaces and the server-side view syntax. Browser applications reference
`Oxpecker.Rocket`; server applications keep their existing references. Do not
reference the browser and server variants together in the same project.

This project links directly to the original source files. Changes within
those files apply to both targets automatically. When adding a new shared source
file, add it to both project compile lists in the same order. Do not copy tag or
attribute implementations here.

Browser-only `Tools.fs` and `Render.fs` live in this project. The existing .NET
renderer and CI workflow are unchanged. Builders, tags, ARIA, and Datastar
attributes are shared sources rather than copies.

The browser target supports `Render.toString` and `Render.toHtmlDocString`, with
the same DSL and escaping contract. Streams, UTF-8 buffer writers, and the .NET
builder pool remain server-only.

## Component authoring

See the [runnable counter and backend Datastar example](../../examples/Rocket/README.md)
for a complete application with two independent counters and server HTML patching.

```fsharp
open Fable.Core.JsInterop
open Oxpecker.ViewEngine
open Oxpecker.Datastar
open Oxpecker.Rocket

let counter =
    RocketDefinition(
        Mode = RocketMode.Light,
        Props = (fun codecs -> createObj [ "start" ==> codecs.number.min(0.0).withDefault(1.0) ]),
        Setup = (fun ctx -> ctx.``$$``?count <- ctx.props?start),
        Render = (fun ctx ->
            Rocket.view ctx (
                div() {
                    button().dataOn("click", "$$count++") { "+" }
                    output().dataText("$$count") { }
                    template().dataIf("$$count > 5") { span() { "Over five" } }
                }
            ))
    )

Rocket.register "ox-counter" counter
```

Use `<ox-counter start="3"></ox-counter>` after loading the compiled module.
Resolve the bare `datastar-rocket` import using your bundler or a browser import map:

```html
<script type="importmap">
{"imports":{"datastar-rocket":"https://cdn.jsdelivr.net/gh/starfederation/datastar@v1.0.4/bundles/datastar-rocket.js"}}
</script>
<script type="module" src="/your-compiled-component.mjs"></script>
```

The Rocket bundle already includes Datastar: do not load a second Datastar runtime.

`Setup` and `OnFirstRender` run per connection. `OnFirstRender` exposes rendered
refs. `effect` and `observeProps` return functions that stop the subscription early;
Rocket also handles lifecycle cleanup. Pass an array of prop names to watch only
those props: `ctx.observeProps(callback, [| "start" |])`. The example copies `start`
only during setup; observe it explicitly if later prop changes should reset the count.

Codecs distinguish strings, numbers, and typed default values. Their transforms
stay fluent after `withDefault`; factory defaults use `withDefaultFactory`.
`Rocket.arrayOf`, `Rocket.objectOf`, and `Rocket.oneOf` cover basic structured codecs.
Props, signals, refs, and host access remain dynamic in this initial API.

`dataIf`, `dataElseIf`, `dataElse`, and `dataFor` apply only to `template()`.
`RocketModifier.root` provides `__root` for keyed signal-name attributes.

`Rocket.view` HTML-encodes ordinary DSL values, then passes the rendered markup
through Rocket's template function for parsing and scope rewriting. It returns
the resulting fragment, not an HTML string. `raw` markup, attribute names, and
JavaScript expressions are trusted code; HTML encoding is not JavaScript escaping.

This initial API does not yet bind manifests, custom codecs, typed prop-shape
inference, predicate-based `RenderOnPropChange`, or all advanced setup/action
overloads. The action callback currently supports the context without trailing arguments.

## Verification

From the repository root, with the local .NET tools restored and Node 24 installed:

```sh
dotnet build Oxpecker.Rocket.slnx
dotnet test tests/Oxpecker.Rocket.Parity.Tests/Oxpecker.Rocket.Parity.Tests.fsproj
dotnet fable tests/Oxpecker.Rocket.Fable.Tests --outDir tests/Oxpecker.Rocket.Fable.Tests/dist --extension .mjs
node tests/Oxpecker.Rocket.Fable.Tests/run.mjs
```

Both test targets compile `tests/Oxpecker.Rocket.Shared/RenderingCases.fs` and
check the same expected output. These tests run independently of the existing
test projects and CI pipeline. JavaScript execution is required: a .NET build
alone does not verify Fable behavior.

The Node runner also verifies registration, option serialization, codec calls,
callback/unsubscribe forwarding, directives, and the template-array bridge using
a test-only runtime stub. It does not prove real Rocket DOM, scoping, or lifecycle
behavior. Browser integration tests against the pinned bundle and packaged-consumer
verification remain the next steps.
