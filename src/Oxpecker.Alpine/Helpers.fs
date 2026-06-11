namespace Oxpecker.Alpine


/// Frequently used modifiers for Transitions.
/// Each constant includes the leading `.` and is appended verbatim after the attribute name.
[<RequireQualifiedAccess>]
module XTransitionModifier =

    /// Adds `x-transition.duration.XXXms` for a XXX transition duration.
    let inline durationMs value = $".duration.%i{value}ms"

    /// Adds `x-transition.delay.XXXms` for a XXX transition delay.
    let inline delayMs value = $".delay.%i{value}ms"

    /// Adds `x-transition.opacity` for a fade transition.
    [<Literal>]
    let opacity = ".opacity"

    /// Adds `x-transition.scale` for a scale transition.
    let inline scale value = $".scale.%i{value}"

    /// Adds `x-transition.scale.origin.XXX` for a scale transition origin of XXX (e.g. `"top"`).
    let inline scaleOrigin origin = $".scale.origin.%s{origin}"

/// Frequently used modifiers for x-model.
/// Each constant includes the leading `.` and is appended verbatim after the attribute name.
[<RequireQualifiedAccess>]
module XModelModifier =

    /// Adds `x-model.number` for numeric input parsing.
    [<Literal>]
    let number = ".number"

    /// Adds `x-model.boolean` for storing JS boolean values instead of strings.
    [<Literal>]
    let boolean = ".boolean"

    /// Adds `x-model.lazy` to only update the property when user focuses away from the input element.
    [<Literal>]
    let lazy' = ".lazy"

    /// Adds `x-model.change` to sync the data only when the input loses focus and its value has changed.
    [<Literal>]
    let change = ".change"

    /// Adds `x-model.blur` to sync the data only when the input loses focus.
    [<Literal>]
    let blur = ".blur"

    /// Adds `x-model.enter` to sync the data only when the user presses the Enter key.
    [<Literal>]
    let enter = ".enter"

    /// Adds `x-model.fill` to use an input's value attribute to populate the property.
    [<Literal>]
    let fill = ".fill"

    /// Adds `x-model.debounce` to debounce model updates by 250 milliseconds.
    [<Literal>]
    let debounce = ".debounce"

    /// Adds `x-model.debounce.XXXms` to debounce model updates by XXX milliseconds.
    let inline debounceMs value = $".debounce.%i{value}ms"

    /// Adds `x-model.throttle` to throttle model updates by 250 milliseconds.
    [<Literal>]
    let throttle = ".throttle"

    /// Adds `x-model.throttle.XXXms` to throttle model updates by XXX milliseconds.
    let inline throttleMs value = $".throttle.%i{value}ms"

/// Frequently used modifiers for x-show.
/// Each constant includes the leading `.` and is appended verbatim after the attribute name.
[<RequireQualifiedAccess>]
module XShowModifier =

    /// Adds `x-show.important` to apply `!important` to the generated CSS, ensuring it takes precedence over other styles.
    [<Literal>]
    let important = ".important"

/// Frequently used modifiers for x-on (and its `@` shorthand).
/// Each value includes the leading `.` and is concatenated onto the event name passed to `xOn`.
[<RequireQualifiedAccess>]
module XOnModifier =

    /// Adds `.prevent` to call `event.preventDefault()` before running the handler.
    [<Literal>]
    let prevent = ".prevent"

    /// Adds `.stop` to call `event.stopPropagation()` before running the handler.
    [<Literal>]
    let stop = ".stop"

    /// Adds `.outside` to listen for clicks outside of the element it is attached to.
    [<Literal>]
    let outside = ".outside"

    /// Adds `.window` to register the listener on the root `window` object instead of the element.
    [<Literal>]
    let window = ".window"

    /// Adds `.document` to register the listener on the `document` object instead of the element.
    [<Literal>]
    let document = ".document"

    /// Adds `.once` to ensure the handler is only called once.
    [<Literal>]
    let once = ".once"

    /// Adds `.debounce` to debounce the handler by 250 milliseconds.
    [<Literal>]
    let debounce = ".debounce"

    /// Adds `.debounce.XXXms` to debounce the handler by XXX milliseconds.
    let inline debounceMs value = $".debounce.%i{value}ms"

    /// Adds `.throttle` to throttle the handler to once every 250 milliseconds.
    [<Literal>]
    let throttle = ".throttle"

    /// Adds `.throttle.XXXms` to throttle the handler to once every XXX milliseconds.
    let inline throttleMs value = $".throttle.%i{value}ms"

    /// Adds `.self` to only run the handler when the event originated on the element itself.
    [<Literal>]
    let self = ".self"

    /// Adds `.camel` to listen for a camelCased version of the event name (e.g. `custom-event` -> `customEvent`).
    [<Literal>]
    let camel = ".camel"

    /// Adds `.dot` to listen for a dotted version of the event name (e.g. `custom-event` -> `custom.event`).
    [<Literal>]
    let dot = ".dot"

    /// Adds `.passive` to register a passive listener that does not block scroll performance.
    [<Literal>]
    let passive = ".passive"

    /// Adds `.passive.false` to make a (passive-by-default) touch or wheel event cancelable.
    [<Literal>]
    let passiveFalse = ".passive.false"

    /// Adds `.capture` to run the handler during the event's capturing phase.
    [<Literal>]
    let capture = ".capture"

/// Keyboard and mouse key modifiers for x-on (and its `@` shorthand).
/// Each value includes the leading `.` and is concatenated onto the event name passed to `xOn`.
[<RequireQualifiedAccess>]
module XOnKey =

    /// Adds `.shift` to require the Shift key (`shiftKey` for mouse events).
    [<Literal>]
    let shift = ".shift"

    /// Adds `.enter` to require the Enter key.
    [<Literal>]
    let enter = ".enter"

    /// Adds `.space` to require the Space key.
    [<Literal>]
    let space = ".space"

    /// Adds `.ctrl` to require the Ctrl key (`ctrlKey` for mouse events).
    [<Literal>]
    let ctrl = ".ctrl"

    /// Adds `.cmd` to require the Cmd key (`metaKey` for mouse events).
    [<Literal>]
    let cmd = ".cmd"

    /// Adds `.meta` to require the Meta key (Cmd on Mac, Windows key on Windows; `metaKey` for mouse events).
    [<Literal>]
    let meta = ".meta"

    /// Adds `.alt` to require the Alt key (`altKey` for mouse events).
    [<Literal>]
    let alt = ".alt"

    /// Adds `.up` to require the Up arrow key.
    [<Literal>]
    let up = ".up"

    /// Adds `.down` to require the Down arrow key.
    [<Literal>]
    let down = ".down"

    /// Adds `.left` to require the Left arrow key.
    [<Literal>]
    let left = ".left"

    /// Adds `.right` to require the Right arrow key.
    [<Literal>]
    let right = ".right"

    /// Adds `.escape` to require the Escape key.
    [<Literal>]
    let escape = ".escape"

    /// Adds `.tab` to require the Tab key.
    [<Literal>]
    let tab = ".tab"

    /// Adds `.caps-lock` to require the Caps Lock key.
    [<Literal>]
    let capsLock = ".caps-lock"

    /// Adds `.equal` to require the Equal (`=`) key.
    [<Literal>]
    let equal = ".equal"

    /// Adds `.period` to require the Period (`.`) key.
    [<Literal>]
    let period = ".period"

    /// Adds `.comma` to require the Comma (`,`) key.
    [<Literal>]
    let comma = ".comma"

    /// Adds `.slash` to require the forward slash (`/`) key.
    [<Literal>]
    let slash = ".slash"
