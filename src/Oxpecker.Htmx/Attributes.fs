namespace Oxpecker.Htmx

open System
open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices
open Oxpecker.ViewEngine

[<AutoOpen>]
module ModifierAttributes =
    /// Inheritance modifier for htmx 4 attributes.
    /// Replace: emit attribute as `hx-foo:inherited` (overrides children's same-named attribute).
    /// Append:  emit attribute as `hx-foo:inherited:append` (appends to children's same-named attribute).
    [<Struct>]
    type HxInherited =
        | Replace
        | Append

    let internal getInheritedSuffix (inherited: HxInherited voption) =
        match inherited with
        | ValueNone -> ""
        | ValueSome HxInherited.Replace -> ":inherited"
        | ValueSome HxInherited.Append -> ":inherited:append"

    /// Marker interface for typed htmx attribute carriers.
    /// Each concrete type knows how to apply itself to an HtmlTag.
    type HxElement =
        abstract member SetAttribute<'T when 'T :> HtmlTag> : 'T -> 'T

    /// Extension that lets callers attach one or more typed htmx attributes to a tag in a single call:
    ///     div().attr(hxGet "/api", hxTarget "#out", hxBoost(true, HxInherited.Replace))
    type HtmlTagHtmxExtensions =
        [<Extension>]
        static member attr(this: #HtmlTag, [<ParamArray>] args: HxElement[]) =
            for arg in args do
                arg.SetAttribute(this) |> ignore
            this


[<AutoOpen>]
module CoreAttributes =
    // ─── Verb attributes (no modifiers) ───

    /// Issues a GET to the specified URL.
    type hxGet(url: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-get", url)

    /// Issues a POST to the specified URL.
    type hxPost(url: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-post", url)

    /// Issues a PUT to the specified URL.
    type hxPut(url: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-put", url)

    /// Issues a PATCH to the specified URL.
    type hxPatch(url: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-patch", url)

    /// Issues a DELETE to the specified URL.
    type hxDelete(url: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-delete", url)

    /// Handle DOM events with inline scripts on elements (renders `hx-on:{event}`).
    type hxOn(event: string, [<StringSyntax("js")>] script: string) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr($"hx-on:{event}", script)

    // ─── Inheritable core attributes ───

    /// Controls how content will swap in (outerHTML, beforeend, afterend, …).
    type hxSwap(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-swap{getInheritedSuffix inherited}", value)

    /// Specifies the target element to be swapped (CSS selector).
    type hxTarget([<StringSyntax("css")>] value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-target{getInheritedSuffix inherited}", value)

    /// Specifies the event that triggers the request.
    type hxTrigger(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-trigger{getInheritedSuffix inherited}", value)

    /// Select content to swap in from a response (CSS selector).
    type hxSelect([<StringSyntax("css")>] value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-select{getInheritedSuffix inherited}", value)

    /// Add values to submit with the request (JSON format).
    type hxVals(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-vals{getInheritedSuffix inherited}", value)


[<AutoOpen>]
module AdditionalAttributes =
    // ─── Inheritable additional attributes ───

    /// Mark element to swap in from a response (out of band).
    type hxSwapOob(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-swap-oob{getInheritedSuffix inherited}", value)

    /// Push a URL into the browser location bar to create history.
    type hxPushUrl(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-push-url{getInheritedSuffix inherited}", value)

    /// Include additional data in requests.
    type hxInclude(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-include{getInheritedSuffix inherited}", value)

    /// Select content to swap in from a response, somewhere other than the target (out of band).
    type hxSelectOob([<StringSyntax("css")>] value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-select-oob{getInheritedSuffix inherited}", value)

    /// Add progressive enhancement for links and forms.
    type hxBoost(value: bool, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr(
                    $"hx-boost{getInheritedSuffix inherited}",
                    (if value then "true" else "false")
                )

    /// Replace the URL in the browser location bar.
    type hxReplaceUrl(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-replace-url{getInheritedSuffix inherited}", value)

    /// Show a confirm() dialog before issuing a request.
    type hxConfirm(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-confirm{getInheritedSuffix inherited}", value)

    /// Adds the `disabled` attribute to specified elements (CSS selector) while a request is in flight.
    /// Optionally `merge` parent values (`hx-disable:merge`) and/or inherit to descendants.
    type hxDisable(selector: string, [<Struct>] ?merge: bool, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                let mergeSuffix =
                    if merge |> ValueOption.defaultValue false then ":merge" else ""
                let inheritedSuffix = getInheritedSuffix inherited
                tag.attr($"hx-disable{mergeSuffix}{inheritedSuffix}", selector)

    /// Specifies elements to keep unchanged between requests. Renders `hx-preserve` (boolean attribute) when true.
    type hxPreserve(value: bool, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                if value then
                    tag.attr($"hx-preserve{getInheritedSuffix inherited}", "")
                else
                    tag

    /// Adds to the headers that will be submitted with the request (JSON object).
    type hxHeaders(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-headers{getInheritedSuffix inherited}", value)

    /// The element to put the `htmx-request` class on during the request (CSS selector).
    type hxIndicator([<StringSyntax("css")>] value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-indicator{getInheritedSuffix inherited}", value)

    /// Control how requests made by different elements are synchronized.
    type hxSync(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-sync{getInheritedSuffix inherited}", value)

    /// Force elements to validate themselves before a request.
    type hxValidate(value: bool, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr(
                    $"hx-validate{getInheritedSuffix inherited}",
                    (if value then "true" else "false")
                )

    /// Changes the request encoding type.
    type hxEncoding(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-encoding{getInheritedSuffix inherited}", value)

    /// Preload content before user triggers request.
    type hxPreload(value: string | null, [<Struct>] ?inherited: HxInherited) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                tag.attr($"hx-preload{getInheritedSuffix inherited}", value)

    // ─── Non-inheritable additional attributes ───

    /// Specify URL to receive request (use with `hxMethod`).
    type hxAction(value: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-action", value)

    /// Specify HTTP method for request (use with `hxAction`).
    type hxMethod(value: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-method", value)

    /// Configure request behavior with JSON.
    type hxConfig(value: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-config", value)

    /// Disable htmx processing for the given node and any children. Renders `hx-ignore` only when true.
    type hxIgnore(value: bool) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) =
                if value then tag.attr("hx-ignore", "") else tag

    /// Show optimistic content during request (template id).
    type hxOptimistic(value: string | null) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr("hx-optimistic", value)

    /// Handle responses differently by status code (`hx-status:CODE`).
    /// `code` is an exact code (e.g. "404"), single-digit wildcard (e.g. "50x"),
    /// or range wildcard (e.g. "5xx"). `value` takes space-separated `key:value` pairs.
    type hxStatus(code: string, value: string) =
        interface HxElement with
            member _.SetAttribute(tag: #HtmlTag) = tag.attr($"hx-status:{code}", value)
