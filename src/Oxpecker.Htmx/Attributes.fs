namespace Oxpecker.Htmx

open System
open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices
open Oxpecker.ViewEngine

[<AutoOpen>]
module ModifierAttributes =
    /// Inheritance modifier for htmx 4 attributes.
    /// Set: emit attribute as `hx-foo:inherited` (applies to all children).
    /// Append:  emit attribute as `hx-foo:inherited:append` (appends to parent's values).
    [<Struct>]
    [<RequireQualifiedAccess>]
    type HxInherited =
        /// Emit attribute as `:inherited` (applies to all children).
        | Set
        /// Emit attribute as `:inherited:append` (appends to parent's values).
        | Append

    let internal getInheritedSuffix (inherited: HxInherited voption) =
        match inherited with
        | ValueNone -> ""
        | ValueSome HxInherited.Set -> ":inherited"
        | ValueSome HxInherited.Append -> ":inherited:append"

    /// Marker interface for typed htmx attribute carriers.
    /// Each concrete type knows how to apply itself to an HtmlTag.
    type HxElement =
        abstract member SetAttribute<'T when 'T :> HtmlTag> : 'T -> unit

    /// Extension that lets callers attach one or more typed htmx attributes to a tag in a single call:
    ///     div().attr(hxGet "/api", hxTarget "#out", hxBoost("true", HxInherited.Set))
    type HtmlTagHtmxExtensions =
        [<Extension>]
        static member attr(this: #HtmlTag, [<ParamArray>] args: HxElement[]) =
            for arg in args do
                arg.SetAttribute(this)
            this

/// Builders for htmx 4 extended selectors.
/// See https://four.htmx.org/docs/features/extended-selectors
/// Each helper returns the htmx selector string ready to pass into selector-typed attributes
/// such as `hxTarget`, `hxSelect`, `hxSelectOob`, `hxIndicator`, `hxInclude`, `hxDisable`, `hxOptimistic`.
[<RequireQualifiedAccess>]
module HxSelector =
    /// `this` — the element itself.
    let this' = "this"
    /// `body` — the document body.
    let body = "body"
    /// `document` — the document object (mainly for event triggers).
    let document = "document"
    /// `window` — the window object (mainly for event triggers).
    let window = "window"
    /// `host` — the shadow DOM host element (only valid inside shadow DOM).
    let host = "host"
    /// `next` — the next sibling element.
    let nextSibling = "next"
    /// `previous` — the previous sibling element.
    let previousSibling = "previous"

    /// `closest <selector>` — nearest ancestor (or self) matching the selector.
    let closest ([<StringSyntax("css")>] selector: string) = $"closest {selector}"
    /// `find <selector>` — first child descendant matching the selector.
    let find ([<StringSyntax("css")>] selector: string) = $"find {selector}"
    /// `findAll <selector>` — all child descendants matching the selector.
    let findAll ([<StringSyntax("css")>] selector: string) = $"findAll {selector}"
    /// `next <selector>` — first following sibling matching the selector.
    let next ([<StringSyntax("css")>] selector: string) = $"next {selector}"
    /// `previous <selector>` — first preceding sibling matching the selector.
    let previous ([<StringSyntax("css")>] selector: string) = $"previous {selector}"
    /// `global <selector>` — search the entire document, crossing shadow DOM boundaries.
    let global' ([<StringSyntax("css")>] selector: string) = $"global {selector}"

    /// Comma-join multiple selectors for multi-target attributes (e.g. `hxTarget`).
    let many (selectors: seq<string>) = String.Join(", ", selectors)


[<AutoOpen>]
module CoreAttributes =
    // ─── Verb attributes (no modifiers) ───

    /// Issues a GET to the specified URL.
    type hxGet([<StringSyntax("uri")>] url: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-get", url) |> ignore

    /// Issues a POST to the specified URL.
    type hxPost([<StringSyntax("uri")>] url: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-post", url) |> ignore

    /// Issues a PUT to the specified URL.
    type hxPut([<StringSyntax("uri")>] url: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-put", url) |> ignore

    /// Issues a PATCH to the specified URL.
    type hxPatch([<StringSyntax("uri")>] url: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-patch", url) |> ignore

    /// Issues a DELETE to the specified URL.
    type hxDelete([<StringSyntax("uri")>] url: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-delete", url) |> ignore

    /// Handle DOM events with inline scripts on elements (renders `hx-on:{event}`).
    type hxOn(event: string, [<StringSyntax("js")>] script: string) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-on:{event}", script) |> ignore

    // ─── Inheritable core attributes ───

    /// Controls how content will swap in (outerHTML, beforeend, afterend, …).
    type hxSwap(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-swap{getInheritedSuffix inherited}", value) |> ignore

    /// Specifies the target element to be swapped (CSS selector).
    type hxTarget([<StringSyntax("css")>] value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-target{getInheritedSuffix inherited}", value) |> ignore

    /// Specifies the event that triggers the request.
    type hxTrigger(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-trigger{getInheritedSuffix inherited}", value) |> ignore

    /// Select content to swap in from a response (CSS selector).
    type hxSelect([<StringSyntax("css")>] value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-select{getInheritedSuffix inherited}", value) |> ignore

    /// Add values to submit with the request (JSON format).
    type hxVals([<StringSyntax("json")>] value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-vals{getInheritedSuffix inherited}", value) |> ignore


[<AutoOpen>]
module AdditionalAttributes =
    // ─── Inheritable additional attributes ───

    /// Mark element to swap in from a response (out of band).
    type hxSwapOob(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-swap-oob{getInheritedSuffix inherited}", value) |> ignore

    /// Push a URL into the browser location bar to create history.
    type hxPushUrl(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-push-url{getInheritedSuffix inherited}", value) |> ignore

    /// Include additional data in requests (CSS selector or extended selector like `closest form`).
    type hxInclude([<StringSyntax("css")>] value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-include{getInheritedSuffix inherited}", value) |> ignore

    /// Select content to swap in from a response, somewhere other than the target (out of band).
    type hxSelectOob([<StringSyntax("css")>] value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-select-oob{getInheritedSuffix inherited}", value) |> ignore

    /// Add progressive enhancement for links and forms.
    type hxBoost(value: string, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-boost{getInheritedSuffix inherited}", value) |> ignore

    /// Replace the URL in the browser location bar.
    type hxReplaceUrl(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-replace-url{getInheritedSuffix inherited}", value) |> ignore

    /// Show a confirm() dialog before issuing a request.
    type hxConfirm(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-confirm{getInheritedSuffix inherited}", value) |> ignore

    /// Adds the `disabled` attribute to specified elements (CSS selector) while a request is in flight.
    /// Optionally `merge` parent values (`hx-disable:merge`) and/or inherit to descendants.
    type hxDisable
        ([<StringSyntax("css")>] selector: string, [<Struct>] ?merge: bool, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                let mergeSuffix =
                    if merge |> ValueOption.defaultValue false then
                        ":merge"
                    else
                        ""
                let inheritedSuffix = getInheritedSuffix inherited
                tag.attr($"hx-disable{mergeSuffix}{inheritedSuffix}", selector) |> ignore

    /// Specifies elements to keep unchanged between requests. Renders `hx-preserve` (boolean attribute) when true.
    type hxPreserve(value: bool, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.bool($"hx-preserve{getInheritedSuffix inherited}", value) |> ignore

    /// Adds to the headers that will be submitted with the request (JSON object).
    type hxHeaders([<StringSyntax("json")>] value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-headers{getInheritedSuffix inherited}", value) |> ignore

    /// The element to put the `htmx-request` class on during the request (CSS selector).
    type hxIndicator([<StringSyntax("css")>] value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-indicator{getInheritedSuffix inherited}", value) |> ignore

    /// Control how requests made by different elements are synchronized.
    type hxSync(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-sync{getInheritedSuffix inherited}", value) |> ignore

    /// Force elements to validate themselves before a request.
    type hxValidate(value: bool, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.bool($"hx-validate{getInheritedSuffix inherited}", value) |> ignore

    /// Changes the request encoding type.
    type hxEncoding(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-encoding{getInheritedSuffix inherited}", value) |> ignore

    /// Preload content before user triggers request.
    type hxPreload(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-preload{getInheritedSuffix inherited}", value) |> ignore

    // ─── Non-inheritable additional attributes ───

    /// Specify URL to receive request (use with `hxMethod`).
    type hxAction([<StringSyntax("uri")>] value: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-action", value) |> ignore

    /// Specify HTTP method for request (use with `hxAction`).
    type hxMethod(value: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-method", value) |> ignore

    /// Configure request behavior with JSON.
    type hxConfig([<StringSyntax("json")>] value: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-config", value) |> ignore

    /// Disable htmx processing for the given node and any children. Renders `hx-ignore` only when true.
    type hxIgnore(value: bool) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.bool("hx-ignore", value) |> ignore

    /// Show optimistic content during request (template id, e.g. `#my-template`).
    type hxOptimistic([<StringSyntax("css")>] value: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr("hx-optimistic", value) |> ignore

    /// Handle responses differently by status code (`hx-status:CODE`).
    /// `code` is an exact code (e.g. "404"), single-digit wildcard (e.g. "50x"),
    /// or range wildcard (e.g. "5xx"). `value` takes space-separated `key:value` pairs.
    type hxStatus(code: string, value: string) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-status:{code}", value) |> ignore
