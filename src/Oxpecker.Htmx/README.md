---
---
# Oxpecker.Htmx

`Oxpecker.Htmx` extends `Oxpecker.ViewEngine` package with HTMX attributes and headers.

[Nuget package](https://www.nuget.org/packages/Oxpecker.Htmx) `dotnet add package Oxpecker.Htmx`

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

Please refer to the [official HTMX site for the documentation](https://htmx.org).

You can check [ContactApp sample](https://github.com/Lanayx/Oxpecker/tree/develop/examples/ContactApp) in the repository to get a better understanding of how the code will look like in your application.

## API

### Client side

After opening `Oxpecker.Htmx` namespace you'll get access to HTMX attributes:
- hxGet
- hxPost
- hxPushUrl
- hxSelect
- hxSelectOob
- hxSwap
- hxSwapOob
- hxTarget
- hxTrigger
- hxVals
- hxBoost
- hxConfirm
- hxDelete
- hxDisable
- hxDisabledElt
- hxDisinherit
- hxEncoding
- hxExt
- hxHeaders
- hxHistory
- hxHistoryElt
- hxInclude
- hxIndicator
- hxParams
- hxPatch
- hxPreserve
- hxPrompt
- hxPut
- hxReplaceUrl
- hxRequest
- hxSync
- hxValidate

and event handler method:

- hxOn

### Server side

After opening `Oxpecker.Htmx` namespace you'll get access to the header name constants:

- HxRequestHeader.Boosted
- HxRequestHeader.CurrentUrl
- HxRequestHeader.HistoryRestoreRequest
- HxRequestHeader.Prompt
- HxRequestHeader.Request
- HxRequestHeader.Target
- HxRequestHeader.TriggerName
- HxRequestHeader.Trigger

and

- HxResponseHeader.Location
- HxResponseHeader.PushUrl
- HxResponseHeader.Redirect
- HxResponseHeader.Refresh
- HxResponseHeader.ReplaceUrl
- HxResponseHeader.Reswap
- HxResponseHeader.Retarget
- HxResponseHeader.Trigger
- HxResponseHeader.TriggerAfterSettle
- HxResponseHeader.TriggerAfterSwap
