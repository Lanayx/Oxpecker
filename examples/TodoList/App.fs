namespace App

open Fable.Core
open Oxpecker.Solid

[<AutoOpen>]
module App =

    let signal = createSignal 0

    [<SolidComponent>]
    let Counter () =
        printfn "Evaluating function..."
        let count, setCount = signal

        __() {
            p() { "Count is "; $"{count()}" }
            button(class'="button", onClick= fun _ -> count() + 1 |> setCount) { "Click me!" }
        }

    [<SolidComponent>]
    let App() =
        JSX.html $"""
        <>
            <h1>Hello world!!!</h1>
            {Counter()}
        </>
        """
