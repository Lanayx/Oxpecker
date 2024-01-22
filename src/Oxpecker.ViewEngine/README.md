# Oxpecker.ViewEngine

[Nuget package](https://www.nuget.org/packages/Oxpecker.ViewEngine)

Markup example:

```fsharp
open Oxpecker.ViewEngine

let staticHtml =
    html() {
        body(style = "width: 800px; margin: 0 auto") {
            h1(style = "text-align: center; color: red") { "Error" }
            p() { "Some long error text" }
            ul() {
                for i in 1..10 do
                    li() { span() { $"Test %d{i}" } }
            }
        }
    }
```

## Documentation:

TBD
