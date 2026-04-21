---
---
# Oxpecker.Htmx

`Oxpecker.Htmx` extends `Oxpecker.ViewEngine` package with [HTMX 4](https://four.htmx.org) attributes and headers.

> **This is a beta release targeting htmx 4.** It is a clean break from the htmx 2.x API — there are no compatibility shims or aliases for removed features.

[Nuget package](https://www.nuget.org/packages/Oxpecker.Htmx) `dotnet add package Oxpecker.Htmx --prerelease`

Markup example:

```fsharp
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let renderForm q =
    form(action="/contacts", method="get") {
        label(for'="search") { "Search Term" }
        input(id="search", type'="search", name="q", value=q, style="margin: 0 5px", autocomplete="off",
              hxGet="/contacts",
              hxTrigger="search, keyup delay:200ms changed",
              hxTarget="tbody",
              hxPushUrl="true",
              hxIndicator="#spinner")
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

### Client side — Event handler

- hxOn

### Client side — Modifier helpers

htmx 4 introduces attribute modifiers for explicit inheritance. Use these helper methods:

```fsharp
// Explicit inheritance: renders hx-boost:inherited="true"
body().hxInherited("hx-boost", "true") { ... }

// Inherited append: renders hx-include:inherited:append=".extra"
form().hxInheritedAppend("hx-include", ".extra") { ... }

// Merge: renders hx-disable:merge="find button"
main().hxMerge("hx-disable", "find button") { ... }

// Status-code swap: renders hx-status:422="swap:innerHTML target:#errors"
form().hxStatus("422", "swap:innerHTML target:#errors") { ... }

// Wildcard status: renders hx-status:5xx="swap:none"
div().hxStatus("5xx", "swap:none") { ... }
```

### Server side — Request headers

- HxRequestHeader.Request
- HxRequestHeader.RequestType (new in htmx 4: `"full"` or `"partial"`)
- HxRequestHeader.CurrentUrl
- HxRequestHeader.Source (new in htmx 4, replaces old `HX-Trigger` request header)
- HxRequestHeader.Target
- HxRequestHeader.Boosted
- HxRequestHeader.HistoryRestoreRequest

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
