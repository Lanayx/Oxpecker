---
---
# Oxpecker.Alpine

`Oxpecker.Alpine` extends `Oxpecker.ViewEngine` with typed [Alpine.js](https://alpinejs.dev) directives.

[Nuget package](https://www.nuget.org/packages/Oxpecker.Alpine) `dotnet add package Oxpecker.Alpine`

Each Alpine directive is exposed as a fluent extension method on `HtmlTag`, so attributes chain directly onto a tag with zero allocation overhead. The `{ children }` builder syntax still works at the end of the chain:

```fsharp
open Oxpecker.ViewEngine
open Oxpecker.Alpine

let dropdown =
    div().xData("{ open: false }") {
        button(type' = "button").xOn("click", "open = !open") { "Toggle" }
        div().xShow("open").xOn("click.outside", "open = false") {
            "Dropdown contents"
        }
    }
```

## Documentation

Please refer to the [official Alpine.js site](https://alpinejs.dev) for directive behavior and supported expressions.

## API

After opening the `Oxpecker.Alpine` namespace you'll get access to the Alpine directives:

| Method        | Renders             | Notes                                                                 |
|---------------|---------------------|-----------------------------------------------------------------------|
| `xData`       | `x-data`            | Valueless and value overloads.                                        |
| `xInit`       | `x-init`            |                                                                       |
| `xEffect`     | `x-effect`          |                                                                       |
| `xShow`       | `x-show`            | Optional `modifiers` overload (e.g. `XShowModifier.important`).       |
| `xBind`       | `x-bind:<name>`     | `name` may include modifiers (e.g. `"class.camel"`).                  |
| `xOn`         | `x-on:<event>`      | `event` may include modifiers (e.g. `"submit.prevent.once"`).         |
| `xText`       | `x-text`            |                                                                       |
| `xHtml`       | `x-html`            |                                                                       |
| `xModel`      | `x-model`           | Optional `modifiers` overload (e.g. `XModelModifier.number`).         |
| `xModelable`  | `x-modelable`       |                                                                       |
| `xFor`        | `x-for`             |                                                                       |
| `xIf`         | `x-if`              |                                                                       |
| `xTransition` | `x-transition`      | Valueless; optional `modifiers` overload.                             |
| `xTransitionOn` | `x-transition:<phase>` | Phase-specific transition. Optional `modifiers` overload.        |
| `xRef`        | `x-ref`             |                                                                       |
| `xId`         | `x-id`              |                                                                       |
| `xCloak`      | `x-cloak`           | Boolean — only renders when `true`.                                   |
| `xIgnore`     | `x-ignore`          | Boolean — only renders when `true`.                                   |
| `xTeleport`   | `x-teleport`        |                                                                       |

### Directive Arguments And Modifiers

For `xOn` and `xBind`, modifiers are written directly into the directive name string, exactly as you would in markup:

```fsharp
// Renders x-on:submit.prevent.once="save()"
form().xOn("submit.prevent.once", "save()") { ... }

// Renders x-bind:class="open ? '' : 'hidden'"
div().xBind("class", "open ? '' : 'hidden'") { ... }
```

Other directives expose an optional `modifiers` parameter that is appended verbatim to the attribute name. Pass it with the leading `.` (the bundled helper modules already include it):

```fsharp
// Renders x-show.important="open"
div().xShow("open", XShowModifier.important) { ... }

// Renders x-model.number.debounce.500ms="age"
input().xModel("age", XModelModifier.number + XModelModifier.debounceMs 500)

// Renders x-transition.duration.500ms
div().xTransition(XTransitionModifier.durationMs 500) { ... }

// Renders x-transition:enter.duration.300ms
div().xTransitionOn("enter", XTransitionModifier.durationMs 300) { ... }
```

The following helper modules expose the most common Alpine modifier names as constants and helper functions. Each value already includes the leading `.` and can be concatenated with `+`:

| Module                | Examples                                                                 |
|-----------------------|--------------------------------------------------------------------------|
| `XShowModifier`       | `important`                                                              |
| `XModelModifier`      | `number`, `boolean`, `lazy'`, `change`, `blur`, `enter`, `fill`, `debounce`, `debounceMs n`, `throttle`, `throttleMs n` |
| `XTransitionModifier` | `durationMs n`, `delayMs n`, `opacity`, `scale n`, `scaleOrigin "top"`   |

Custom modifier strings are also accepted; just remember to include the leading `.`:

```fsharp
input().xModel("query", ".debounce.250ms")
```

### Valueless Directives

Use the parameterless overloads for directives Alpine allows without values:

```fsharp
// Renders x-data
div().xData() { ... }

// Renders x-transition
div().xTransition() { ... }
```

Boolean-style directives only render when `true`:

```fsharp
// Renders x-cloak
div().xCloak(true) { ... }

// Does not render x-cloak
div().xCloak(false) { ... }
```
