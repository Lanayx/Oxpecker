namespace App

open Oxpecker.Solid

[<AutoOpen>]
module App =

    [<SolidComponent>]
    let Counter () =
        let count, setCount = createSignal 0
        div() {
            p() { "Count is "; $"{count()}" }
            button(class'="button", onClick= fun _ -> setCount(count() + 1)) { "Click me!" }
        }

    [<SolidComponent>]
    let App() =
        div() {
            h1() { "Hello world!" }
            Counter()
        }
