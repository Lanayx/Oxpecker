---
---
# Oxpecker.Alpine

`Oxpecker.Alpine` extends `Oxpecker.ViewEngine` with typed [Alpine.js](https://alpinejs.dev) directives.

[Nuget package](https://www.nuget.org/packages/Oxpecker.Alpine) `dotnet add package Oxpecker.Alpine`

Each Alpine directive is a typed value applied to a tag through the variadic `.attr(...)` extension. This keeps the `{ children }` builder syntax intact and lets you compose Alpine attributes uniformly:

```fsharp
open Oxpecker.ViewEngine
open Oxpecker.Alpine

let dropdown =
    div().attr(xData "{ open: false }") {
        button(type' = "button").attr(xOn("click", "open = !open")) { "Toggle" }
        div().attr(xShow "open", xOn("click", "open = false", "outside")) {
            "Dropdown contents"
        }
    }
```

## Documentation

Please refer to the [official Alpine.js site](https://alpinejs.dev) for directive behavior and supported expressions.

## API

After opening `Oxpecker.Alpine` namespace you'll get access to Alpine directives:

- xData
- xInit
- xShow
- xBind
- xOn
- xText
- xHtml
- xModel
- xModelable
- xFor
- xTransition
- xEffect
- xIgnore
- xRef
- xCloak
- xTeleport
- xIf
- xId

### Directive Arguments And Modifiers

Directives that support Alpine arguments or modifiers accept them directly:

```fsharp
// Renders x-on:submit.prevent.once="save()"
form().attr(xOn("submit", "save()", "prevent", "once")) { ... }

// Renders x-bind:class="open ? '' : 'hidden'"
div().attr(xBind("class", "open ? '' : 'hidden'")) { ... }

// Renders x-transition:enter.duration.300ms="transition ease-out"
div().attr(xTransition(XTransitionPhase.Enter, "transition ease-out", "duration.300ms")) { ... }

// Renders x-ignore.self
div().attr(xIgnore(true, "self")) { ... }
```

### Valueless Directives

Use the parameterless overloads for directives Alpine allows without values:

```fsharp
// Renders x-data
div().attr(xData()) { ... }

// Renders x-transition
div().attr(xTransition()) { ... }
```

Boolean-style directives only render when true:

```fsharp
// Renders x-cloak
div().attr(xCloak true) { ... }

// Does not render x-cloak
div().attr(xCloak false) { ... }
```
