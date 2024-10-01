module Oxpecker.Solid.Tests.Cases.HelloWorld

open Oxpecker.Solid

[<SolidComponent>]
let Signal () =
    let count, _ = createSignal 0

    __() {
        p() { $"Count is {count()}" }
        //button(class'="button", onClick= fun _ -> count() + 1 |> setCount) { "Click me!" }
    }
