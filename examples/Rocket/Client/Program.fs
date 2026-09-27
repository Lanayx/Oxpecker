module RocketExample.Client

open Fable.Core.JsInterop
open Oxpecker.ViewEngine
open Oxpecker.Datastar
open Oxpecker.Rocket

let counter =
    RocketDefinition(
        Mode = RocketMode.Light,
        Props = (fun codecs -> createObj [ "start" ==> codecs.number.min(0.0).withDefault(0.0) ]),
        Setup = (fun ctx -> ctx.``$$``?count <- ctx.props?start),
        Render =
            (fun ctx ->
                Rocket.view
                    ctx
                    (div(class' = "counter") {
                        button(type' = "button").dataOn("click", "$$count = Math.max(0, $$count - 1)") { "Decrease" }
                        output().attr("aria-live", "polite").dataText("$$count") { }
                        button(type' = "button").dataOn("click", "$$count++") { "Increase" }
                        template().dataIf("$$count >= 5") { p() { "Five or more!" } }
                    }))
    )

Rocket.register "ox-counter" counter
