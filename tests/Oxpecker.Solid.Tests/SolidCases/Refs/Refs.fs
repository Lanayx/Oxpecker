module Oxpecker.Solid.Tests.Cases.Refs

open Browser.Types
open Oxpecker.Solid
open Fable.Core.JS


[<SolidComponent>]
let Test () =
    let mutable htmlCanvas: HTMLCanvasElement = Unchecked.defaultof<_>
    onMount(fun () -> console.log(htmlCanvas.height + htmlCanvas.width))

    div().ref(fun _ -> console.log("before mounted")) {
        canvas(width=256, height=256).ref(htmlCanvas)
    }
