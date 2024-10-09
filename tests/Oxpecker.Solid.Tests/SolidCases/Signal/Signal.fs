module Oxpecker.Solid.Tests.Cases.Signal

open Oxpecker.Solid

[<SolidComponent>]
let Test () =
    let count, setCount = createSignal 0

    Fragment() {
        p() { $"Count1 is {count()}" }
        p() { "Count2 is "; count() }
        button(class'="button", onClick= fun _ -> count() + 1 |> setCount) { "Click me!" }
    }
