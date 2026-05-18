namespace Oxpecker.Alpine

open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices
open Oxpecker.ViewEngine
open JetBrains.Annotations


/// Alpine.js component and lifecycle methods.
type AlpineComponentExtensions =

    /// Defines an Alpine component scope. Renders valueless `x-data`.
    [<Extension>]
    static member xData(this: #HtmlTag) = this.bool("x-data", true)

    /// Defines an Alpine component scope with data.
    [<Extension>]
    static member xData
        (this: #HtmlTag, [<LanguageInjection(InjectedLanguage.JAVASCRIPT, Prefix = "this=")>] value: string | null)
        =
        this.attr("x-data", value)

    /// Runs an expression when Alpine initializes an element.
    [<Extension>]
    static member xInit(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-init", value)

    /// Runs an expression whenever its reactive dependencies change.
    [<Extension>]
    static member xEffect(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-effect", value)

    /// Hides an element until Alpine is ready.
    [<Extension>]
    static member xCloak(this: #HtmlTag, value: bool) = this.bool("x-cloak", value)


/// Alpine.js binding and event methods.
type AlpineBindingExtensions =

    /// Binds an attribute to an Alpine expression.
    [<Extension>]
    static member xBind(this: #HtmlTag, name: string, [<StringSyntax("js")>] value: string | null) =
        this.attr($"x-bind:%s{name}", value)

    /// Handles DOM events with Alpine expressions. `event` may include modifiers (e.g. `"submit.prevent.once"`).
    [<Extension>]
    static member xOn(this: #HtmlTag, event: string, [<StringSyntax("js")>] value: string | null) =
        this.attr($"x-on:%s{event}", value)

    /// Creates a two-way binding with form inputs.
    [<Extension>]
    static member xModel(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-model", value)

    /// Creates a two-way binding with form inputs.
    /// `modifiers` is appended verbatim and must include the leading `.` (e.g. `".number.debounce.500ms"`).
    /// XModelModifier provides helper values for common modifiers.
    [<Extension>]
    static member xModel(this: #HtmlTag, [<StringSyntax("js")>] value: string | null, modifiers: string) =
        this.attr($"x-model%s{modifiers}", value)

    /// Exposes an inner x-model value to a parent component.
    [<Extension>]
    static member xModelable(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr("x-modelable", value)


/// Alpine.js content and conditional rendering methods.
type AlpineRenderingExtensions =

    /// Toggles element visibility from an Alpine expression.
    [<Extension>]
    static member xShow(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-show", value)

    /// Toggles element visibility from an Alpine expression.
    /// `modifiers` is appended verbatim and must include the leading `.` (e.g. `".important"`).
    /// XShowModifier provides helper values for common modifiers.
    [<Extension>]
    static member xShow(this: #HtmlTag, [<StringSyntax("js")>] value: string | null, modifiers: string) =
        this.attr($"x-show%s{modifiers}", value)

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
    static member xTeleport(this: #HtmlTag, [<StringSyntax("css")>] value: string | null) =
        this.attr("x-teleport", value)


/// Alpine.js transition methods.
type AlpineTransitionExtensions =

    /// Adds a valueless Alpine transition attribute.
    [<Extension>]
    static member xTransition(this: #HtmlTag) = this.bool("x-transition", true)

    /// Adds a valueless Alpine transition attribute.
    /// `modifiers` is the dot-separated suffix (e.g. `".duration.500ms"`).
    /// XTransitionModifier provides helper functions for common modifiers.
    [<Extension>]
    static member xTransition(this: #HtmlTag, modifiers: string) =
        this.bool($"x-transition%s{modifiers}", true)

    /// Adds a valueless Alpine transition attribute.
    /// `phase` is the transition phase (e.g. `"enter"`, `"leave-start"`).
    [<Extension>]
    static member xTransitionOn(this: #HtmlTag, phase: string) =
        this.bool($"x-transition:%s{phase}", true)

    /// Adds a valueless Alpine transition attribute.
    /// `phase` is the transition phase (e.g. `"enter"`, `"leave-start"`).
    /// `modifiers` is the dot-separated suffix (e.g. `".duration.500ms"`).
    /// XTransitionModifier provides helper functions for common modifiers.
    [<Extension>]
    static member xTransitionOn(this: #HtmlTag, phase: string, modifiers: string) =
        this.bool($"x-transition:%s{phase}%s{modifiers}", true)


/// Alpine.js utility methods.
type AlpineUtilityExtensions =

    /// Prevents Alpine from initializing a subtree.
    [<Extension>]
    static member xIgnore(this: #HtmlTag, value: bool) = this.bool("x-ignore", value)

    /// Registers a named reference for the current component.
    [<Extension>]
    static member xRef(this: #HtmlTag, value: string | null) = this.attr("x-ref", value)

    /// Generates scoped ids for an Alpine component.
    [<Extension>]
    static member xId(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr("x-id", value)
