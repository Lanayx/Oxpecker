namespace Oxpecker.Alpine

open System
open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices
open Oxpecker.ViewEngine

[<AutoOpen>]
module AlpineAttributes =

    let internal appendModifiers (name: string) (modifiers: string array) =
        modifiers
        |> Array.filter (String.IsNullOrWhiteSpace >> not)
        |> Array.fold (fun acc modifier -> $"%s{acc}.%s{modifier.TrimStart('.')}") name

    let internal directiveName (directive: string) (argument: string | null) (modifiers: string array) =
        let name =
            if isNull argument || String.IsNullOrWhiteSpace argument then
                directive
            else
                let argumentName = argument |> Unchecked.nonNull
                $"%s{directive}:%s{argumentName.TrimStart(':')}"

        appendModifiers name modifiers

    /// Alpine transition phase for `x-transition:*` directives.
    [<Struct>]
    [<RequireQualifiedAccess>]
    type XTransitionPhase =
        | Enter
        | EnterStart
        | EnterEnd
        | Leave
        | LeaveStart
        | LeaveEnd

    let internal transitionPhaseName phase =
        match phase with
        | XTransitionPhase.Enter -> "enter"
        | XTransitionPhase.EnterStart -> "enter-start"
        | XTransitionPhase.EnterEnd -> "enter-end"
        | XTransitionPhase.Leave -> "leave"
        | XTransitionPhase.LeaveStart -> "leave-start"
        | XTransitionPhase.LeaveEnd -> "leave-end"

    /// Marker interface for typed Alpine.js attribute carriers.
    /// Each concrete type knows how to apply itself to an HtmlTag.
    type AlpineElement =
        abstract member SetAttribute<'T when 'T :> HtmlTag> : 'T -> unit

    /// Extension that lets callers attach one or more typed Alpine.js attributes to a tag in a single call:
    ///     div().attr(xData "{ open: false }", xShow "open", xOn("click", "open = !open"))
    type HtmlTagAlpineExtensions =
        [<Extension>]
        static member attr(this: #HtmlTag, [<ParamArray>] args: AlpineElement[]) =
            for arg in args do
                arg.SetAttribute(this)

            this

    /// Defines an Alpine component scope. Use xData() to render a valueless x-data attribute.
    type xData =
        val value: string | null
        val hasValue: bool

        new() =
            { value = null
              hasValue = false }

        new([<StringSyntax("js")>] value: string | null) =
            { value = value
              hasValue = true }

        interface AlpineElement with
            member this.SetAttribute(tag: #HtmlTag) =
                if this.hasValue then
                    tag.attr("x-data", this.value) |> ignore
                else
                    tag.bool("x-data", true) |> ignore

    /// Runs an expression when Alpine initializes an element.
    type xInit([<StringSyntax("js")>] value: string | null) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("x-init", value) |> ignore

    /// Toggles element visibility from an Alpine expression.
    type xShow([<StringSyntax("js")>] value: string | null, [<ParamArray>] modifiers: string[]) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr(directiveName "x-show" null modifiers, value) |> ignore

    /// Binds one attribute, or a binding object when no attribute name is supplied.
    type xBind =
        val name: string | null
        val value: string | null
        val modifiers: string array

        new([<StringSyntax("js")>] value: string | null) =
            { name = null
              value = value
              modifiers = [||] }

        new(name: string, [<StringSyntax("js")>] value: string | null, [<ParamArray>] modifiers: string[]) =
            { name = name
              value = value
              modifiers = modifiers }

        interface AlpineElement with
            member this.SetAttribute(tag: #HtmlTag) =
                tag.attr(directiveName "x-bind" this.name this.modifiers, this.value) |> ignore

    /// Handles DOM events with Alpine expressions.
    type xOn(event: string, [<StringSyntax("js")>] value: string | null, [<ParamArray>] modifiers: string[]) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr(directiveName "x-on" event modifiers, value) |> ignore

    /// Sets text content from an Alpine expression.
    type xText([<StringSyntax("js")>] value: string | null) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("x-text", value) |> ignore

    /// Sets HTML content from an Alpine expression.
    type xHtml([<StringSyntax("js")>] value: string | null) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("x-html", value) |> ignore

    /// Creates a two-way binding with form inputs.
    type xModel([<StringSyntax("js")>] value: string | null, [<ParamArray>] modifiers: string[]) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr(directiveName "x-model" null modifiers, value) |> ignore

    /// Exposes an inner x-model value to a parent component.
    type xModelable([<StringSyntax("js")>] value: string | null) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("x-modelable", value) |> ignore

    /// Repeats a template for every item in an Alpine expression.
    type xFor([<StringSyntax("js")>] value: string | null) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("x-for", value) |> ignore

    /// Adds Alpine transition directives. Use xTransition() to render valueless x-transition.
    type xTransition =
        val argument: string | null
        val value: string | null
        val hasValue: bool
        val modifiers: string array

        new([<ParamArray>] transitionModifiers: string[]) =
            { argument = null
              value = null
              hasValue = false
              modifiers = transitionModifiers }

        new(phase: XTransitionPhase, [<StringSyntax("js")>] value: string | null, [<ParamArray>] modifiers: string[]) =
            { argument = transitionPhaseName phase
              value = value
              hasValue = true
              modifiers = modifiers }

        interface AlpineElement with
            member this.SetAttribute(tag: #HtmlTag) =
                let name = directiveName "x-transition" this.argument this.modifiers

                if this.hasValue then
                    tag.attr(name, this.value) |> ignore
                else
                    tag.bool(name, true) |> ignore

    /// Runs an expression whenever its reactive dependencies change.
    type xEffect([<StringSyntax("js")>] value: string | null) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("x-effect", value) |> ignore

    /// Prevents Alpine from initializing a subtree. Use xIgnore(true, "self") for x-ignore.self.
    type xIgnore(value: bool, [<ParamArray>] modifiers: string[]) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.bool(directiveName "x-ignore" null modifiers, value) |> ignore

    /// Registers a named reference for the current component.
    type xRef(value: string | null) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("x-ref", value) |> ignore

    /// Hides an element until Alpine is ready.
    type xCloak(value: bool) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.bool("x-cloak", value) |> ignore

    /// Teleports a template to another part of the DOM.
    type xTeleport([<StringSyntax("css")>] value: string | null) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("x-teleport", value) |> ignore

    /// Conditionally renders a template from an Alpine expression.
    type xIf([<StringSyntax("js")>] value: string | null) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("x-if", value) |> ignore

    /// Generates scoped ids for an Alpine component.
    type xId([<StringSyntax("js")>] value: string | null) =
        interface AlpineElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("x-id", value) |> ignore
