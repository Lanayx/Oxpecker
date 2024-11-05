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
        input(type'="submit", value="Search")
    }
```

## Documentation:

Please refer to the official site for the HTMX documentation: https://htmx.org/.

You can check [ContactApp sample](https://github.com/Lanayx/Oxpecker/tree/develop/examples/ContactApp) in the repository to get a better understanding of how the code will look like in your application.
