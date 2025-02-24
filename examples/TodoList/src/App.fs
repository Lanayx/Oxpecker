module App

open Oxpecker.Solid
open Components
open Oxpecker.Solid.Meta
open Oxpecker.Solid.Router

[<SolidComponent>]
let App() : HtmlElement =
    div(){
        TodoList()
        //TodoListStore()
        br()
        br()
        A(href="/about", class'="block text-right") {
            "About"
        }
    }

[<SolidComponent>]
let About() : HtmlElement =
    Fragment() {
        Title() { "About" }
        h1() {
            "TodoList example made with Oxpecker.Solid!"
        }
        br()
        br()
        A(href="/", class'="block text-right") {
            "Back"
        }
    }
