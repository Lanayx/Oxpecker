namespace App

open Fable.Core
open Oxpecker.Solid
open Browser.Dom

[<AutoOpen>]
module App =


    let signal = Solid.createSignal 0

    [<SolidComponent>]
    let Counter2 () =
        printfn "Evaluating function..."
        let count, setCount = signal

        __() {
            p() { $"Count is {count()}" }
            button(class'="button", onClick= fun _ -> count() + 1 |> setCount) { "Click me!" }
        }

    let Counter() =
        printfn "Evaluating function..."
        let count, setCount = signal

        JSX.html $"""
        <>
            <p>Count is {let _ = printfn "Evaluating expression..." in count()}</p>
            <button class="button" onclick={fun _ -> count () + 1 |> setCount}>
                Click me!
            </button>
        </>
        """


    [<SolidComponent>]
    let App() =
        JSX.html $"""
        <>
            <h1>Hello world!!!</h1>
            {Counter2()}
        </>
        """
