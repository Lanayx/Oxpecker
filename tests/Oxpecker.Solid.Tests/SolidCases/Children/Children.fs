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
        Component "Hello1" <| br()
        Component "Hello2" <| Fragment() {
            i()
        }
        For(each = [|1..3|]) {
            yield fun i _ ->
                Component "Hello3" (Fragment() {
                    string i
                })
        }
    }
