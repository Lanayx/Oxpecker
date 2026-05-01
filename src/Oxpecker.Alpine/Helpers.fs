namespace Oxpecker.Alpine


/// Frequently used modifiers for Transitions.
/// Each constant includes the leading `.` and is appended verbatim after the attribute name.
[<RequireQualifiedAccess>]
module XTransitionModifier =

    /// Adds `x-transition.duration.XXXms` for a XXX transition duration.
    let durationMs value = $".duration.%i{value}ms"

    /// Adds `x-transition.delay.XXXms` for a XXX transition delay.
    let delayMs value = $".delay.%i{value}ms"

    /// Adds `x-transition.opacity` for a fade transition.
    [<Literal>]
    let opacity = ".opacity"

    /// Adds `x-transition.scale` for a scale transition.
    let scale value = $".scale.%i{value}"

    /// Adds `x-transition.scale.origin.XXX` for a scale transition origin of XXX (e.g. `"top"`).
    let scaleOrigin origin = $".scale.origin.%s{origin}"

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
    let fill = ".fill"

    /// Adds `x-model.debounce` to debounce model updates by 250 milliseconds.
    [<Literal>]
    let debounce = ".debounce"

    /// Adds `x-model.debounce.XXXms` to debounce model updates by XXX milliseconds.
    let debounceMs value = $".debounce.%i{value}ms"

    /// Adds `x-model.throttle` to throttle model updates by 250 milliseconds.
    [<Literal>]
    let throttle = ".throttle"

    /// Adds `x-model.throttle.XXXms` to throttle model updates by XXX milliseconds.
    let throttleMs value = $".throttle.%i{value}ms"

/// Frequently used modifiers for x-show.
/// Each constant includes the leading `.` and is appended verbatim after the attribute name.
[<RequireQualifiedAccess>]
module XShowModifier =

    /// Adds `x-show.important` to apply `!important` to the generated CSS, ensuring it takes precedence over other styles.
    [<Literal>]
    let important = ".important"
