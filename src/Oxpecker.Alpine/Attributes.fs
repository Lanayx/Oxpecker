namespace Oxpecker.Alpine

open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices
open Oxpecker.ViewEngine

[<AutoOpen>]
module ModifierAttributes =

    let internal modifierSuffix (modifiers: string option) =
        match modifiers with
        | Some m when m.Length > 0 -> $".{m.TrimStart('.')}"
        | _ -> ""


/// Alpine.js component and lifecycle directives.
[<Extension>]
type AlpineComponentExtensions =

    /// Defines an Alpine component scope. Renders valueless `x-data`.
    [<Extension>]
    static member xData(this: #HtmlTag) = this.bool("x-data", true)

    /// Defines an Alpine component scope with data.
    [<Extension>]
    static member xData(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-data", value)

    /// Runs an expression when Alpine initializes an element.
    [<Extension>]
    static member xInit(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-init", value)

    /// Runs an expression whenever its reactive dependencies change.
    [<Extension>]
    static member xEffect(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-effect", value)

    /// Hides an element until Alpine is ready.
    [<Extension>]
    static member xCloak(this: #HtmlTag, value: bool) = this.bool("x-cloak", value)


/// Alpine.js binding and event directives.
[<Extension>]
type AlpineBindingExtensions =

    /// Binds an object of directives or attributes to the element.
    [<Extension>]
    static member xBind(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-bind", value)

    /// Binds an attribute to an Alpine expression. `name` may include modifiers (e.g. `"class.camel"`).
    [<Extension>]
    static member xBind(this: #HtmlTag, name: string, [<StringSyntax("js")>] value: string | null) =
        this.attr($"x-bind:{name}", value)

    /// Handles DOM events with Alpine expressions. `event` may include modifiers (e.g. `"submit.prevent.once"`).
    [<Extension>]
    static member xOn(this: #HtmlTag, event: string, [<StringSyntax("js")>] value: string | null) =
        this.attr($"x-on:{event}", value)

    /// Creates a two-way binding with form inputs. `modifiers` is the optional dot-separated suffix (e.g. `"number.debounce.500ms"`).
    [<Extension>]
    static member xModel(this: #HtmlTag, [<StringSyntax("js")>] value: string | null, ?modifiers: string) =
        this.attr($"x-model{modifierSuffix modifiers}", value)

    /// Exposes an inner x-model value to a parent component.
    [<Extension>]
    static member xModelable(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-modelable", value)


/// Alpine.js content and conditional rendering directives.
[<Extension>]
type AlpineRenderingExtensions =

    /// Toggles element visibility from an Alpine expression. `modifiers` is the optional dot-separated suffix (e.g. `"important"`).
    [<Extension>]
    static member xShow(this: #HtmlTag, [<StringSyntax("js")>] value: string | null, ?modifiers: string) =
        this.attr($"x-show{modifierSuffix modifiers}", value)

    /// Sets text content from an Alpine expression.
    [<Extension>]
    static member xText(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-text", value)

    /// Sets HTML content from an Alpine expression.
    [<Extension>]
    static member xHtml(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-html", value)

    /// Repeats a template for every item in an Alpine expression.
    [<Extension>]
    static member xFor(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-for", value)

    /// Conditionally renders a template from an Alpine expression.
    [<Extension>]
    static member xIf(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-if", value)

    /// Teleports a template to another part of the DOM.
    [<Extension>]
    static member xTeleport(this: #HtmlTag, [<StringSyntax("css")>] value: string | null) = this.attr("x-teleport", value)


/// Alpine.js transition directives.
[<Extension>]
type AlpineTransitionExtensions =

    /// Adds a valueless Alpine transition directive. `modifiers` is the optional dot-separated suffix (e.g. `"duration.500ms"`).
    [<Extension>]
    static member xTransition(this: #HtmlTag, ?modifiers: string) =
        this.bool($"x-transition{modifierSuffix modifiers}", true)


/// Alpine.js utility directives.
[<Extension>]
type AlpineUtilityExtensions =

    /// Prevents Alpine from initializing a subtree. `modifiers` is the optional dot-separated suffix (e.g. `"self"`).
    [<Extension>]
    static member xIgnore(this: #HtmlTag, value: bool, ?modifiers: string) =
        this.bool($"x-ignore{modifierSuffix modifiers}", value)

    /// Registers a named reference for the current component.
    [<Extension>]
    static member xRef(this: #HtmlTag, value: string | null) = this.attr("x-ref", value)

    /// Generates scoped ids for an Alpine component.
    [<Extension>]
    static member xId(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-id", value)
