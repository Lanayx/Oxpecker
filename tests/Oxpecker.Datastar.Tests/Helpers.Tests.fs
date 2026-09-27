module Helpers.Tests

open Oxpecker.Datastar
open Xunit

[<Fact>]
let ``case modifiers have the documented spellings`` () =
    Assert.All(
        [
            DsCase.camel, "__case.camel"
            DsCase.kebab, "__case.kebab"
            DsCase.snake, "__case.snake"
            DsCase.pascal, "__case.pascal"
        ],
        fun (actual, expected) -> Assert.Equal(expected, actual)
    )

[<Fact>]
let ``constant modifiers have the documented spellings`` () =
    Assert.All(
        [
            DsModifier.once, "__once"
            DsModifier.passive, "__passive"
            DsModifier.capture, "__capture"
            DsModifier.window, "__window"
            DsModifier.document, "__document"
            DsModifier.outside, "__outside"
            DsModifier.prevent, "__prevent"
            DsModifier.stop, "__stop"
            DsModifier.viewTransition, "__viewtransition"
            DsModifier.ifMissing, "__ifmissing"
            DsModifier.terse, "__terse"
            DsModifier.self, "__self"
            DsModifier.exit, "__exit"
            DsModifier.half, "__half"
            DsModifier.full, "__full"
        ],
        fun (actual, expected) -> Assert.Equal(expected, actual)
    )

[<Fact>]
let ``parameterised modifiers have the documented spellings`` () =
    Assert.All(
        [
            DsModifier.delay "1s", "__delay.1s"
            DsModifier.delayMs 500, "__delay.500ms"
            DsModifier.debounce "1s", "__debounce.1s"
            DsModifier.debounceMs 500, "__debounce.500ms"
            DsModifier.debounceLeading "1s", "__debounce.1s.leading"
            DsModifier.debounceNoTrailing "1s", "__debounce.1s.notrailing"
            DsModifier.throttle "1s", "__throttle.1s"
            DsModifier.throttleMs 500, "__throttle.500ms"
            DsModifier.throttleNoLeading "1s", "__throttle.1s.noleading"
            DsModifier.throttleTrailing "1s", "__throttle.1s.trailing"
            DsModifier.duration "1s", "__duration.1s"
            DsModifier.durationMs 500, "__duration.500ms"
            DsModifier.durationLeading "1s", "__duration.1s.leading"
            DsModifier.threshold "0.5", "__threshold.0.5"
            DsModifier.bindProp "checked", "__prop.checked"
            DsModifier.bindEvent "input.change", "__event.input.change"
        ],
        fun (actual, expected) -> Assert.Equal(expected, actual)
    )
