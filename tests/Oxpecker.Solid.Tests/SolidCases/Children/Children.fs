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
        Component "Hello2" <| __() {
            i()
        }
        For(each = [|1..3|]) {
            fun i _ ->
                Component "Hello3" (__() {
                    string i
                })
        }
    }
