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
form().xOn("submit.prevent.once", "save()") { ... }

// Renders x-bind:class="open ? '' : 'hidden'"
div().xBind("class", "open ? '' : 'hidden'") { ... }

// Renders x-transition.duration.500ms
div().xTransition("duration.500ms") { ... }

// Renders x-ignore.self
div().xIgnore(true, "self") { ... }
```

### Valueless Directives

Use the parameterless overloads for directives Alpine allows without values:

```fsharp
// Renders x-data
div().xData() { ... }

// Renders x-transition
div().xTransition() { ... }
```

Boolean-style directives only render when true:

```fsharp
// Renders x-cloak
div().xCloak(true) { ... }

// Does not render x-cloak
div().xCloak(false) { ... }
```
