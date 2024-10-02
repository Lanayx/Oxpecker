namespace App

open Oxpecker.Solid

[<AutoOpen>]
module App =

    let count, setCount = createSignal 0

    [<SolidComponent>]
    let Counter (text: string, currentCount: Accessor<int>) =
        div() {
            h1() { text }
            p() { "Count is "; currentCount() }
            button(class'="button", onClick= fun _ -> setCount(currentCount() + 1)) { "Click me!" }
        }

    [<SolidComponent>]
    let App() =
        div() {
            Counter("Hello!", count)
        }
