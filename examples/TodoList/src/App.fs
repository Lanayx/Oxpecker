module App

open Oxpecker.Solid
open Components

[<SolidComponent>]
let App() : HtmlElement =
    TodoList()
    //TodoListStore()

[<SolidComponent>]
let About() : HtmlElement =
    h1() {
        "TodoList example made with Oxpecker.Solid!"
    }
