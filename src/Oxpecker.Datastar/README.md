# Oxpecker.Datastar

`Oxpecker.Datastar` extends `Oxpecker.ViewEngine` with typed [Datastar](https://data-star.dev) `data-*` attributes.

This package renders attributes on the server. Load the Datastar JavaScript bundle in your page separately to make them interactive. For example, after [self-hosting the bundle](https://data-star.dev/guide/getting_started#installation) at `/datastar.js`:

```fsharp
head() {
    script(type' = "module", src = "/datastar.js") { }
}
```

If the page uses Rocket components, load `datastar-rocket.js` instead; that bundle already includes Datastar. This package does not choose a script URL or bundle version for your application.

Each Datastar attribute is exposed as a fluent extension method on `HtmlTag`, so attributes chain directly onto a tag with zero allocation overhead. The `{ children }` builder syntax still works at the end of the chain:

```fsharp
open Oxpecker.ViewEngine
open Oxpecker.Datastar

let counter =
    section().dataSignals("{ count: 0 }") {
        h2() { "Datastar counter" }
        p() {
            "Count: "
            strong().dataText("$count") { "0" }
        }
        button(type' = "button").dataOn("click", "$count--") { "−" }
        button(type' = "button").dataOn("click", "$count++") { "+" }
        button(type' = "button").dataOn("click", "$count = 0") { "Reset" }
    }
```

## Documentation

Please refer to the [official Datastar reference](https://data-star.dev/reference) for attribute behavior and supported expressions.

## API

After opening the `Oxpecker.Datastar` namespace you'll get access to the Datastar attributes:

| Method | Renders | Notes |
|---|---|---|
| `dataSignals` | `data-signals` | Value form (object/JSON) and key form (`data-signals:<name>`) with optional modifiers. |
| `dataComputed` | `data-computed` | Value form and key form (`data-computed:<name>`) with optional modifiers. |
| `dataRef` / `dataRefValue` | `data-ref:<name>` / `data-ref` | Valueless key form with optional modifiers, or value form. |
| `dataIndicator` / `dataIndicatorValue` | `data-indicator:<name>` / `data-indicator` | Valueless key form with optional modifiers, or value form. |
| `dataBind` / `dataBindValue` | `data-bind:<name>` / `data-bind` | Valueless key form with optional modifiers, or value form. |
| `dataAttr` | `data-attr` | Value form (object) and key form (`data-attr:<name>`). |
| `dataStyle` | `data-style` | Value form (object) and key form (`data-style:<property>`). |
| `dataClass` | `data-class` | Value form (object) and key form (`data-class:<name>`) with optional modifiers. |
| `dataText` | `data-text` | Expression only. |
| `dataShow` | `data-show` | Expression only. |
| `dataOn` | `data-on:<event>` | Event listener with optional modifiers. |
| `dataOnIntersect` | `data-on-intersect` | Optional modifiers. |
| `dataOnInterval` | `data-on-interval` | Optional modifiers. |
| `dataOnSignalPatch` | `data-on-signal-patch` | Optional modifiers. |
| `dataOnSignalPatchFilter` | `data-on-signal-patch-filter` | Filter object. |
| `dataInit` | `data-init` | Optional modifiers. |
| `dataEffect` | `data-effect` | Expression only. |
| `dataIgnore` | `data-ignore` | Boolean; optional modifiers (e.g. `__self`). |
| `dataIgnoreMorph` | `data-ignore-morph` | Boolean. |
| `dataPreserveAttr` | `data-preserve-attr` | Space-separated attribute names. |
| `dataJsonSignals` | `data-json-signals` | Valueless, filter value, and modifiers overloads. |

### Key forms and value forms

Attributes that define signals or DOM bindings accept both Datastar's key syntax and its object/value syntax:

```fsharp
// Key form: renders data-signals:foo="1"
div().dataSignals("foo", "1")

// Value form: renders data-signals="{ foo: 1, bar: 2 }"
div().dataSignals("{ foo: 1, bar: 2 }")

// Key form: renders data-class:font-bold="$foo"
div().dataClass("font-bold", "$foo")

// Value form: renders data-class="{ 'font-bold': $foo }"
div().dataClass("{ 'font-bold': $foo }")
```

Attributes whose key form is valueless (`data-bind`, `data-ref`, `data-indicator`) use the key form as their single-argument overload:

```fsharp
// Renders data-bind:query
input().dataBind("query")

// Renders data-indicator:fetching
button().dataIndicator("fetching")

// Renders data-ref:foo
div().dataRef("foo")

// Value forms render the signal name as an attribute value
input().dataBindValue("query")
button().dataIndicatorValue("fetching")
div().dataRefValue("foo")
```

### Modifiers

Datastar modifiers are appended **verbatim** after the attribute key or name and must include the leading `__`. The `DsModifier` and `DsCase` modules provide helpers for the common ones, and they compose with `+`:

```fsharp
// Renders data-on:click__window__debounce.500ms.leading="$foo = ''"
button().dataOn("click", "$foo = ''", DsModifier.window + DsModifier.debounceLeading "500ms")

// Renders data-signals:my-signal__case.kebab="1"
div().dataSignals("my-signal", "1", DsCase.kebab)

// Renders data-bind:is-checked__prop.checked__event.change
input().dataBind("is-checked", DsModifier.bindProp "checked" + DsModifier.bindEvent "change")

// Renders data-init__delay.500ms="$count = 1"
div().dataInit("$count = 1", DsModifier.delayMs 500)

// Renders data-on-intersect__once__full="$seen = true"
div().dataOnIntersect("$seen = true", DsModifier.once + DsModifier.full)
```

### Boolean attributes

Boolean attributes only render when `true`:

```fsharp
// Renders data-ignore
div().dataIgnore(true) { ... }

// Does not render data-ignore
div().dataIgnore(false) { ... }

// Renders data-ignore__self
div().dataIgnore(true, DsModifier.self) { ... }
```

## Pro attributes

[Datastar Pro](https://data-star.dev/pro) attributes live in the `Oxpecker.Datastar.Pro` namespace and require the commercial Datastar Pro bundle:

```fsharp
open Oxpecker.ViewEngine
open Oxpecker.Datastar.Pro

div().dataMatchMedia("is-dark", "'prefers-color-scheme: dark'") { ... }
div().dataPersist() { ... }
div().dataPersistKey("mykey", DsProModifier.session) { ... }
button().dataScrollIntoView(DsProModifier.smooth + DsProModifier.focus) { "Scroll" }
```

| Method | Renders |
|---|---|
| `dataAnimate` | `data-animate` |
| `dataCustomValidity` | `data-custom-validity` |
| `dataMatchMedia` | `data-match-media:<signal>` |
| `dataOnRaf` | `data-on-raf` |
| `dataOnResize` | `data-on-resize` |
| `dataPersist` / `dataPersistKey` | `data-persist` / `data-persist:<name>` |
| `dataQueryString` / `dataQueryStringWith` | `data-query-string` |
| `dataReplaceUrl` | `data-replace-url` |
| `dataScrollIntoView` | `data-scroll-into-view` |
| `dataViewTransition` | `data-view-transition` |
