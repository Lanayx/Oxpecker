Fable compiler plugin referenced by `Oxpecker.Solid` library. It is used to transform Oxpecker View Engine computation expressions to Solid-compatible JSX.

[Nuget package](https://www.nuget.org/packages/Oxpecker.Solid.FablePlugin) `dotnet add package Oxpecker.Solid.FablePlugin`

**Medium article**: [How to write Fable compiler plugin](https://medium.com/@lanayx/how-to-write-fable-compiler-plugin-79fa0ee6c4c2)

### Attribute Usage

```fsharp
open Oxpecker.Solid // this library namespace

[<SolidComponent>]
let hello =
    h1() {
        "Hello world!"
    }
```
You can pass special flag to inspect component AST for debugging purposes:

```fsharp
[<SolidComponent(SolidComponentFlag.Debug)>]
let hello =
    h1() {
        "Hello world!"
    }
```
