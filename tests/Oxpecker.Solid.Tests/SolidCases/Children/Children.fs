module Oxpecker.Solid.Tests.Cases.Children

open Oxpecker.Solid

[<SolidComponent>]
let Component (hello: string) (children: #HtmlElement) =
    h1() {
        hello
        children
    }

[<SolidComponent>]
let Test () =
    div() {
        Component "Hello" (__() {
            "World"
        })
    }
