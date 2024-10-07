module Oxpecker.Solid.Tests.Cases.Children

open System.Runtime.CompilerServices
open Oxpecker.Solid

[<SolidComponent>]
let Component (hello: string) (children: #HtmlElement) =
    h1() {
        hello
        children
    }

// [<SolidComponent>]
// let Test () =
//     div() {
//         Component "xxx" (__() {
//             "World"
//         })
//     }
