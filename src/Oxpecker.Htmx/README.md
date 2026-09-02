---
---
# Oxpecker.Htmx

`Oxpecker.Htmx` extends `Oxpecker.ViewEngine` package with [HTMX 4](https://four.htmx.org) attributes and headers.

> **This release targets htmx 4.0 (GA).** It is a clean break from the htmx 2.x API — there are no compatibility shims or aliases for removed features.

[Nuget package](https://www.nuget.org/packages/Oxpecker.Htmx) `dotnet add package Oxpecker.Htmx`

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
- hxQuery
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
- hxStatus (new in htmx 4)
- hxMorphSkip (morph swaps only)
- hxMorphSkipChildren (morph swaps only)

### Client side — Event handler

- hxOn

### Client side — Built-in extensions

htmx 4 ships a set of [opt-in extensions](https://four.htmx.org/extensions) that you load client-side with their own `<script>` tags. Open the `Oxpecker.Htmx.Extensions` namespace to get fluent extension methods for the attributes each one introduces:

```fsharp
open Oxpecker.Htmx
open Oxpecker.Htmx.Extensions
```

| Extension | Method(s) | Renders |
|---|---|---|
| [hx-sse](https://four.htmx.org/extensions/hx-sse) | `hxSseConnect`, `hxSseClose` | `hx-sse:connect`, `hx-sse:close` |
| [hx-ws](https://four.htmx.org/extensions/hx-ws) | `hxWsConnect`, `hxWsSend` | `hx-ws:connect`, `hx-ws:send` |
| [hx-multipart](https://four.htmx.org/extensions/hx-multipart) | `hxMultipartConnect`, `hxMultipartClose` | `hx-multipart:connect`, `hx-multipart:close` |
| [hx-head](https://four.htmx.org/extensions/hx-head) | `hxHead` (see `HxHeadMode`) | `hx-head` |
| [hx-targets](https://four.htmx.org/extensions/hx-targets) | `hxTargets` | `hx-targets` |
| [hx-ptag](https://four.htmx.org/extensions/hx-ptag) | `hxPtag` | `hx-ptag` |
| [hx-browser-indicator](https://four.htmx.org/extensions/hx-browser-indicator) | `hxBrowserIndicator` | `hx-browser-indicator` |
| [hx-history-cache](https://four.htmx.org/extensions/hx-history-cache) | `hxHistory` | `hx-history` |
| [hx-csp](https://four.htmx.org/extensions/hx-csp) | `hxNonce` | `hx-nonce` |
| [hx-live](https://four.htmx.org/extensions/hx-live) | `hxLive`, `hxLiveBind` (see `HxLiveBinding`) | `hx-live`, `hx-live:{name}` |
| [hx-prompt](https://four.htmx.org/extensions/hx-prompt) | `hxPrompt` | `hx-prompt` |
| [hx-pending](https://four.htmx.org/extensions/hx-pending) | `hxPending` | `hx-pending` |
| [hx-preload](https://four.htmx.org/extensions/hx-preload) | `hxPreload` (lives in the core `Oxpecker.Htmx` namespace; the extension script is still required) | `hx-preload` |

The [hx-download](https://four.htmx.org/extensions/hx-download) and [hx-upsert](https://four.htmx.org/extensions/hx-upsert) extensions reuse `hxSwap`. Use `HxSwapMethod.download` for downloads, and `HxSwapMethod.upsert` with the `HxUpsertModifier` helpers (`sort`, `sortDesc`, `prepend`, `key`) for upserts.

```fsharp
// hx-sse: persistent stream, append messages
div().hxSseConnect("/log").hxSwap("beforeend") { h3() { "Log:" } }

// hx-ws: connect + send a form over the socket
div().hxWsConnect("/chatroom") {
    form().hxWsSend(true) {
        input(name="message")
        button(type'="submit") { "Send" }
    }
}

// hx-targets: swap the response into every matching element
button().hxGet("/api/notification").hxTargets(".alert-box") { "Refresh All" }

// hx-download: save the response as a file
button().hxGet("/files/report.pdf").hxSwap(HxSwapMethod.download) { "Download" }

// hx-upsert: update-or-insert keyed list items, sorted descending
div().hxGet("/items").hxSwap(HxSwapMethod.upsert + HxUpsertModifier.sortDesc) { "items" }

// hx-multipart: stream parts from a persistent connection until a part fires the "done" event (HX-Trigger: done)
div().hxMultipartConnect("/events").hxMultipartClose("done") { "Waiting..." }

// hx-prompt: ask before deleting, answer arrives as the HX-Prompt request header
button().hxDelete("/item/1").hxPrompt("Reason?") { "Delete" }

// hx-pending: show a template while the request is in flight
form().hxPost("/message").hxTarget("#messages").hxSwap("beforeend").hxPending("#sending") {
    input(name="body")
    button() { "Send" }
}

// hx-live: bind an attribute to a reactive expression
button().hxLiveBind("disabled", "!q('#name').value") { "Save" }
// hx-live: special bindings — textContent and a single class toggle
span().hxLiveBind(HxLiveBinding.text, "q('#count').value")
div().hxLiveBind(HxLiveBinding.toggleClass "active", "q('#tab').value === 'a'") { "tab" }
```

### Client side — Modifier helpers

Inheritable attributes accept an optional `modifiers` string that is appended **verbatim** after the attribute name — you must include the leading `:`. Pass htmx 4 modifier syntax directly, e.g. `":inherited"`, `":inherited:append"`, or for `hxDisable` also `":merge"` / `":merge:inherited"`. The `HxModifier` module provides constants for the most common ones. `hxStatus` writes status-coded attributes (`hx-status:CODE`) and takes the code as its first argument.

```fsharp
// Explicit inheritance: renders hx-boost:inherited="true"
body().hxBoost("true", HxModifier.inherited) { ... }

// Inherited append: renders hx-include:inherited:append=".extra"
form().hxInclude(".extra", HxModifier.inherited + HxModifier.append) { ... }

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

Build [htmx 4 extended selectors](https://four.htmx.org/docs/features/extended-selectors) in a type-safe way via the `HxSelector` module instead of writing them as plain strings. Pass the result into any selector-typed attribute (`hxTarget`, `hxSelect`, `hxSelectOob`, `hxIndicator`, `hxInclude`, `hxDisable`).

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
- HxRequestHeader.LastEventId (last received SSE event ID for reconnection)
- HxRequestHeader.Preloaded (`hx-preload` extension)
- HxRequestHeader.PTag (`hx-ptag` extension)
- HxRequestHeader.Prompt (`hx-prompt` extension; the value is URI-encoded, decode it with `Uri.UnescapeDataString`)
- HxRequestHeader.LastPartId (`hx-multipart` extension)

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
- HxResponseHeader.Download (`hx-download` extension)
- HxResponseHeader.PTag (`hx-ptag` extension)
- HxResponseHeader.PartId (`hx-multipart` extension)

## Migration from Oxpecker.Htmx 2.x

### Removed attributes (no htmx 4 equivalent)
- `hxDisabledElt` — use `hxDisable` (new semantics)
- `hxDisinherit` — not needed (inheritance is explicit in htmx 4)
- `hxExt` — include extension scripts directly
- `hxHistory` — removed (no localStorage cache)
- `hxHistoryElt` — removed
- `hxParams` — use `htmx:config:request` event instead
- `hxRequest` — use `hxConfig`

### Renamed attributes
- Old `hxDisable` (skip htmx processing) → `hxIgnore`
- Old `hxDisabledElt` (disable form elements during request) → `hxDisable`

### Moved to extensions
- `hxPrompt` and `HxRequestHeader.Prompt` — now provided by the `hx-prompt` extension: open `Oxpecker.Htmx.Extensions` and load the extension script client-side

### Removed request headers
- `HxRequestHeader.Trigger` — use `HxRequestHeader.Source`
- `HxRequestHeader.TriggerName` — use `HxRequestHeader.Source`

### Removed response headers
- `HxResponseHeader.TriggerAfterSettle` — use `HxResponseHeader.Trigger`
- `HxResponseHeader.TriggerAfterSwap` — use `HxResponseHeader.Trigger`

## Migration from Oxpecker.Htmx 4.0.0-beta (htmx 4 beta4 → 4.0.0 GA)

### Removed
- `hxOptimistic` / the `hx-optimistic` extension — replaced by `hxPending` (`hx-pending` extension), which takes the same kind of template selector
- `HxRequestHeader.RequestId` / `HxResponseHeader.RequestId` — the `hx-ws` extension was rewritten in GA and no longer uses `HX-Request-ID`

### Added
- `hxQuery` — the new `QUERY` verb (safe and idempotent like `GET`, body-carrying like `POST`)
- `hxMorphSkip` / `hxMorphSkipChildren` — opt elements out of morph swaps
- `HxSwapModifier.swapEmpty` — force or suppress the main swap when the response is empty after out-of-band content is removed
- `hxPrompt` (`hx-prompt` extension) and `HxRequestHeader.Prompt` — htmx 2's prompt behaviour, restored as an extension
- `hxPending` (`hx-pending` extension)
- `hxMultipartConnect` / `hxMultipartClose` (`hx-multipart` extension), plus `HxRequestHeader.LastPartId` and `HxResponseHeader.PartId`
- `hxLiveBind` — the `hx-live:{name}` reactive attribute binding added to the `hx-live` extension, with `HxLiveBinding` constants for the special `text`, `html`, `style`, `class` and `.{class}` names
- `HxSwapModifier.settle` / `settleMs` now emit a leading space like every other swap modifier, so they compose with `+` directly; drop any manual `+ " " +` you had inserted
