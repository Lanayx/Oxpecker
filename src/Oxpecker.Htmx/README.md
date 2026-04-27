---
---
# Oxpecker.Htmx

`Oxpecker.Htmx` extends `Oxpecker.ViewEngine` package with [HTMX 4](https://four.htmx.org) attributes and headers.

> **This is a beta release targeting htmx 4.** It is a clean break from the htmx 2.x API — there are no compatibility shims or aliases for removed features.

[Nuget package](https://www.nuget.org/packages/Oxpecker.Htmx) `dotnet add package Oxpecker.Htmx --prerelease`

Each htmx attribute is exposed as a fluent extension method on `HtmlTag`, so attributes chain directly onto a tag with zero allocation overhead. The `{ children }` builder syntax still works at the end of the chain:

```fsharp
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let renderForm q =
    form(action="/contacts", method="get") {
        label(for'="search") { "Search Term" }
        input(id="search", type'="search", name="q", value=q, style="margin: 0 5px", autocomplete="off")
            .hxGet("/contacts")
            .hxTrigger("search, keyup delay:200ms changed")
            .hxTarget("tbody")
            .hxPushUrl("true")
            .hxIndicator("#spinner")
        img(id="spinner", class'="spinner htmx-indicator", src="/spinning-circles.svg", alt="Request In Flight...")
        input(type'="submit", value="Search").hxOn("click", "alert('clicked')")
    }
```

## Documentation

Please refer to the [official HTMX 4 site for the documentation](https://four.htmx.org).

You can check [ContactApp sample](https://github.com/Lanayx/Oxpecker/tree/develop/examples/ContactApp) in the repository to get a better understanding of how the code will look like in your application.

## API

### Client side — Core attributes

After opening `Oxpecker.Htmx` namespace you'll get access to HTMX attributes:
- hxGet
- hxPost
- hxPut
- hxPatch
- hxDelete
- hxSwap
- hxTarget
- hxTrigger
- hxSelect
- hxVals

### Client side — Additional attributes

- hxSwapOob
- hxPushUrl
- hxInclude
- hxSelectOob
- hxBoost
- hxReplaceUrl
- hxConfirm
- hxDisable (htmx 4: disables elements during request, replaces old `hx-disabled-elt`)
- hxPreserve
- hxHeaders
- hxIndicator
- hxSync
- hxPreload (new in htmx 4)
- hxValidate
- hxEncoding
- hxAction (new in htmx 4)
- hxMethod (new in htmx 4)
- hxConfig (new in htmx 4, replaces old `hx-request`)
- hxIgnore (new in htmx 4, replaces old `hx-disable`)
- hxOptimistic (new in htmx 4)
- hxStatus (new in htmx 4)

### Client side — Event handler

- hxOn

### Client side — Modifier helpers

Inheritable attributes accept an optional `modifiers` string that is appended **verbatim** after the attribute name — you must include the leading `:`. Pass htmx 4 modifier syntax directly, e.g. `":inherited"`, `":inherited:append"`, or for `hxDisable` also `":merge"` / `":merge:inherited"`. The `HxModifier` module provides constants for the most common ones. `hxStatus` writes status-coded attributes (`hx-status:CODE`) and takes the code as its first argument.

```fsharp
// Explicit inheritance: renders hx-boost:inherited="true"
body().hxBoost("true", HxModifier.inherited) { ... }

// Inherited append: renders hx-include:inherited:append=".extra"
form().hxInclude(".extra", HxModifier.inheritedAppend) { ... }

// Merge: renders hx-disable:merge="find button"
main().hxDisable("find button", HxModifier.merge) { ... }

// Merge + inherited: renders hx-disable:merge:inherited="find button"
main().hxDisable("find button", ":merge:inherited") { ... }

// Status-code swap: renders hx-status:422="swap:innerHTML target:#errors"
form().hxPost("/save").hxStatus("422", "swap:innerHTML target:#errors") { ... }

// Wildcard status: renders hx-status:5xx="swap:none"
div().hxGet("/data").hxStatus("5xx", "swap:none") { ... }
```

### Client side — Extended selectors

Build [htmx 4 extended selectors](https://four.htmx.org/docs/features/extended-selectors) in a type-safe way via the `HxSelector` module instead of writing them as plain strings. Pass the result into any selector-typed attribute (`hxTarget`, `hxSelect`, `hxSelectOob`, `hxIndicator`, `hxInclude`, `hxDisable`, `hxOptimistic`).

```fsharp
button().hxDelete("/item/1").hxTarget(HxSelector.closest ".card") { "Delete" }
div().hxGet("/user").hxTarget(HxSelector.find ".username") { "Loading" }
button().hxGet("/data").hxTarget(HxSelector.nextSibling) { "Load" }
button().hxGet("/data").hxTarget("#a, #b") { "Load" }   // multiple targets: just write the comma-separated string
```

Available helpers:
- Values: `this'`, `body`, `document`, `window`, `host`, `nextSibling`, `previousSibling`
- Functions: `closest`, `find`, `findAll`, `next`, `previous`, `global'`

### Server side — Request headers

- HxRequestHeader.Request
- HxRequestHeader.RequestType (new in htmx 4: `"full"` or `"partial"`)
- HxRequestHeader.CurrentUrl
- HxRequestHeader.Source (new in htmx 4, replaces old `HX-Trigger` request header)
- HxRequestHeader.Target
- HxRequestHeader.Boosted
- HxRequestHeader.HistoryRestoreRequest
- HxRequestHeader.Accept (content types htmx accepts from the server)
- HxRequestHeader.LastEventID (last received SSE event ID for reconnection)

### Server side — Response headers

- HxResponseHeader.Trigger
- HxResponseHeader.Location
- HxResponseHeader.Redirect
- HxResponseHeader.Refresh
- HxResponseHeader.PushUrl
- HxResponseHeader.ReplaceUrl
- HxResponseHeader.Reswap
- HxResponseHeader.Retarget
- HxResponseHeader.Reselect

## Migration from Oxpecker.Htmx 2.x

### Removed attributes (no htmx 4 equivalent)
- `hxDisabledElt` — use `hxDisable` (new semantics)
- `hxDisinherit` — not needed (inheritance is explicit in htmx 4)
- `hxExt` — include extension scripts directly
- `hxHistory` — removed (no localStorage cache)
- `hxHistoryElt` — removed
- `hxParams` — use `htmx:config:request` event instead
- `hxPrompt` — use `hxConfirm` with `js:` prefix
- `hxRequest` — use `hxConfig`

### Renamed attributes
- Old `hxDisable` (skip htmx processing) → `hxIgnore`
- Old `hxDisabledElt` (disable form elements during request) → `hxDisable`

### Removed request headers
- `HxRequestHeader.Trigger` — use `HxRequestHeader.Source`
- `HxRequestHeader.TriggerName` — use `HxRequestHeader.Source`
- `HxRequestHeader.Prompt` — removed

### Removed response headers
- `HxResponseHeader.TriggerAfterSettle` — use `HxResponseHeader.Trigger`
- `HxResponseHeader.TriggerAfterSwap` — use `HxResponseHeader.Trigger`
