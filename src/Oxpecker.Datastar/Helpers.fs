namespace Oxpecker.Datastar


/// Casing values for the Datastar `__case` modifier.
/// Each value includes the full `__case.` prefix and is appended verbatim after the attribute key.
[<RequireQualifiedAccess>]
module DsCase =

    /// `__case.camel` — camelCase (default for signal names).
    [<Literal>]
    let camel = "__case.camel"

    /// `__case.kebab` — kebab-case (default for class names and events).
    [<Literal>]
    let kebab = "__case.kebab"

    /// `__case.snake` — snake_case.
    [<Literal>]
    let snake = "__case.snake"

    /// `__case.pascal` — PascalCase.
    [<Literal>]
    let pascal = "__case.pascal"


/// Datastar attribute modifiers.
/// Each value includes the leading `__` (or the `__name.` prefix for parameterised modifiers)
/// and is appended verbatim after the attribute key/name passed to the fluent attribute methods.
/// Time values are Datastar time expressions such as `"500ms"` or `"1s"`.
[<RequireQualifiedAccess>]
module DsModifier =

    /// `__once` — only trigger the listener once.
    [<Literal>]
    let once = "__once"

    /// `__passive` — do not call `preventDefault` on the listener (built-in events only).
    [<Literal>]
    let passive = "__passive"

    /// `__capture` — use a capture-phase event listener (built-in events only).
    [<Literal>]
    let capture = "__capture"

    /// `__window` — attach the listener to `window`.
    [<Literal>]
    let window = "__window"

    /// `__document` — attach the listener to `document`.
    [<Literal>]
    let document = "__document"

    /// `__outside` — trigger when the event happens outside the element.
    [<Literal>]
    let outside = "__outside"

    /// `__prevent` — call `preventDefault` on the event.
    [<Literal>]
    let prevent = "__prevent"

    /// `__stop` — call `stopPropagation` on the event.
    [<Literal>]
    let stop = "__stop"

    /// `__viewtransition` — wrap the expression in `document.startViewTransition()` when available.
    [<Literal>]
    let viewTransition = "__viewtransition"

    /// `__ifmissing` — only patch signals whose keys do not already exist (for `data-signals`).
    [<Literal>]
    let ifMissing = "__ifmissing"

    /// `__terse` — compact JSON output (for `data-json-signals`).
    [<Literal>]
    let terse = "__terse"

    /// `__self` — only ignore the element itself, not its descendants (for `data-ignore`).
    [<Literal>]
    let self = "__self"

    /// `__exit` — only trigger when the element exits the viewport (for `data-on-intersect`).
    [<Literal>]
    let exit = "__exit"

    /// `__half` — trigger when half the element is visible (for `data-on-intersect`).
    [<Literal>]
    let half = "__half"

    /// `__full` — trigger when the whole element is visible (for `data-on-intersect`).
    [<Literal>]
    let full = "__full"

    /// `__delay.{time}` — delay the listener/expression. `time` is e.g. `"500ms"` or `"1s"`.
    let inline delay time = $"__delay.%s{time}"

    /// `__delay.{ms}ms` — delay the listener/expression by an integer number of milliseconds.
    let inline delayMs ms = $"__delay.%i{ms}ms"

    /// `__debounce.{time}` — debounce the listener. `time` is e.g. `"500ms"` or `"1s"`.
    let inline debounce time = $"__debounce.%s{time}"

    /// `__debounce.{ms}ms` — debounce the listener by an integer number of milliseconds.
    let inline debounceMs ms = $"__debounce.%i{ms}ms"

    /// `__debounce.{time}.leading` — debounce with leading edge.
    let inline debounceLeading time = $"__debounce.%s{time}.leading"

    /// `__debounce.{time}.notrailing` — debounce without trailing edge.
    let inline debounceNoTrailing time = $"__debounce.%s{time}.notrailing"

    /// `__throttle.{time}` — throttle the listener. `time` is e.g. `"500ms"` or `"1s"`.
    let inline throttle time = $"__throttle.%s{time}"

    /// `__throttle.{ms}ms` — throttle the listener by an integer number of milliseconds.
    let inline throttleMs ms = $"__throttle.%i{ms}ms"

    /// `__throttle.{time}.noleading` — throttle without leading edge.
    let inline throttleNoLeading time = $"__throttle.%s{time}.noleading"

    /// `__throttle.{time}.trailing` — throttle with trailing edge.
    let inline throttleTrailing time = $"__throttle.%s{time}.trailing"

    /// `__duration.{time}` — interval duration (for `data-on-interval`). `time` is e.g. `"500ms"` or `"1s"`.
    let inline duration time = $"__duration.%s{time}"

    /// `__duration.{ms}ms` — interval duration in milliseconds (for `data-on-interval`).
    let inline durationMs ms = $"__duration.%i{ms}ms"

    /// `__duration.{time}.leading` — execute the first interval immediately.
    let inline durationLeading time = $"__duration.%s{time}.leading"

    /// `__threshold.{n}` — visibility threshold for `data-on-intersect`, e.g. `"0.5"`.
    let inline threshold value = $"__threshold.%s{value}"

    /// `__prop.{name}` — bind to a specific property instead of the default binding (for `data-bind`).
    let inline bindProp name = $"__prop.%s{name}"

    /// `__event.{events}` — events that sync the element property back to the signal, dot-separated (for `data-bind`).
    let inline bindEvent events = $"__event.%s{events}"
