module Oxpecker.Solid.Tests.Cases.Meta

open Oxpecker.Solid
open Oxpecker.Solid.Meta

[<SolidComponent>]
let Component () =
    h1() {
        Title() { "MyComponent" }
        Meta(content="https://example.com/image.jpg").attr("property", "og:image")
    }

[<SolidComponent>]
let Test () =
    MetaProvider() {
        Component()
    }
