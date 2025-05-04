module App

open Frontend.Components.Orders
open Oxpecker.Solid

[<SolidComponent>]
let App() =
    main(class'="container mx-auto") {
        h1(class'="text-4xl font-bold text-center mt-10") {
            "CRUD App"
        }
        Orders()
    }
