namespace Oxpecker.Htmx

open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices
open Oxpecker.ViewEngine

[<AutoOpen>]
module internal ModifierHelpers =
    let inline getModifierSuffix (modifiers: string voption) =
        modifiers |> ValueOption.defaultValue ""

/// Frequently used modifiers for attributes.
/// Each constant includes the leading `:` and is appended verbatim after the attribute name.
[<RequireQualifiedAccess>]
module HxModifier =
    /// `:inherited` — the attribute is inherited by child elements.
    [<Literal>]
    let inherited = ":inherited"
    /// `:inherited:append` — the attribute value is appended to any existing value instead of replacing it.
    [<Literal>]
    let inheritedAppend = ":inherited:append"
    /// `:merge` — for `hxDisable`, the selector is merged with any existing selector instead of replacing it.
    [<Literal>]
    let merge = ":merge"

/// Builders for extended selectors.
/// See https://four.htmx.org/docs/features/extended-selectors
/// Each helper returns the htmx selector string ready to pass into selector-typed attributes
/// such as `hxTarget`, `hxSelect`, `hxSelectOob`, `hxIndicator`, `hxInclude`, `hxDisable`, `hxOptimistic`.
[<RequireQualifiedAccess>]
module HxSelector =
    /// `this` — the element itself.
    [<Literal>]
    let this' = "this"
    /// `body` — the document body.
    [<Literal>]
    let body = "body"
    /// `document` — the document object (mainly for event triggers).
    [<Literal>]
    let document = "document"
    /// `window` — the window object (mainly for event triggers).
    [<Literal>]
    let window = "window"
    /// `host` — the shadow DOM host element (only valid inside shadow DOM).
    [<Literal>]
    let host = "host"
    /// `next` — the next sibling element.
    [<Literal>]
    let nextSibling = "next"
    /// `previous` — the previous sibling element.
    [<Literal>]
    let previousSibling = "previous"

    /// `closest <selector>` — nearest ancestor (or self) matching the selector.
    let inline closest ([<StringSyntax("css")>] selector: string) = $"closest {selector}"
    /// `find <selector>` — first child descendant matching the selector.
    let inline find ([<StringSyntax("css")>] selector: string) = $"find {selector}"
    /// `findAll <selector>` — all child descendants matching the selector.
    let inline findAll ([<StringSyntax("css")>] selector: string) = $"findAll {selector}"
    /// `next <selector>` — first following sibling matching the selector.
    let inline next ([<StringSyntax("css")>] selector: string) = $"next {selector}"
    /// `previous <selector>` — first preceding sibling matching the selector.
    let inline previous ([<StringSyntax("css")>] selector: string) = $"previous {selector}"
    /// `global <selector>` — search the entire document, crossing shadow DOM boundaries.
    let inline global' ([<StringSyntax("css")>] selector: string) = $"global {selector}"


/// Verb attributes (no modifiers).
type HtmxVerbExtensions =

    /// Issues a GET to the specified URL.
    [<Extension>]
    static member hxGet(this: #HtmlTag, [<StringSyntax("Uri")>] url: string | null) = this.attr("hx-get", url)

    /// Issues a POST to the specified URL.
    [<Extension>]
    static member hxPost(this: #HtmlTag, [<StringSyntax("Uri")>] url: string | null) = this.attr("hx-post", url)

    /// Issues a PUT to the specified URL.
    [<Extension>]
    static member hxPut(this: #HtmlTag, [<StringSyntax("Uri")>] url: string | null) = this.attr("hx-put", url)

    /// Issues a PATCH to the specified URL.
    [<Extension>]
    static member hxPatch(this: #HtmlTag, [<StringSyntax("Uri")>] url: string | null) = this.attr("hx-patch", url)

    /// Issues a DELETE to the specified URL.
    [<Extension>]
    static member hxDelete(this: #HtmlTag, [<StringSyntax("Uri")>] url: string | null) = this.attr("hx-delete", url)


/// Event handler attribute.
type HtmxEventExtensions =

    /// Handle DOM events with inline scripts on elements (renders `hx-on:{event}`).
    [<Extension>]
    static member hxOn(this: #HtmlTag, event: string, [<StringSyntax("js")>] script: string) =
        this.attr($"hx-on:{event}", script)


/// Inheritable core attributes.
/// `modifiers` (optional) is appended verbatim after the attribute name and must include any leading `:`,
/// e.g. `":inherited"` → `hx-foo:inherited`, `":inherited:append"` → `hx-foo:inherited:append`.
/// Use the `HxModifier` module for the common cases.
type HtmxCoreExtensions =

    /// Controls how content will swap in (outerHTML, beforeend, afterend, …).
    [<Extension>]
    static member hxSwap(this: #HtmlTag, value: string | null, [<Struct>] ?modifiers: string) =
        this.attr($"hx-swap%s{getModifierSuffix modifiers}", value)

    /// Specifies the target element to be swapped (CSS selector).
    [<Extension>]
    static member hxTarget
        (this: #HtmlTag, [<StringSyntax("css")>] value: string | null, [<Struct>] ?modifiers: string)
        =
        this.attr($"hx-target%s{getModifierSuffix modifiers}", value)

    /// Specifies the event that triggers the request.
    [<Extension>]
    static member hxTrigger(this: #HtmlTag, value: string | null, [<Struct>] ?modifiers: string) =
        this.attr($"hx-trigger%s{getModifierSuffix modifiers}", value)

    /// Select content to swap in from a response (CSS selector).
    [<Extension>]
    static member hxSelect
        (this: #HtmlTag, [<StringSyntax("css")>] value: string | null, [<Struct>] ?modifiers: string)
        =
        this.attr($"hx-select%s{getModifierSuffix modifiers}", value)

    /// Add values to submit with the request (JSON format).
    [<Extension>]
    static member hxVals(this: #HtmlTag, [<StringSyntax("Json")>] value: string | null, [<Struct>] ?modifiers: string) =
        this.attr($"hx-vals%s{getModifierSuffix modifiers}", value)


/// Additional inheritable + non-inheritable attributes.
/// For inheritable attributes, `modifiers` (optional) is appended verbatim after the attribute name
/// and must include any leading `:`. Use the `HxModifier` module for the common cases.
type HtmxAdditionalExtensions =

    // ─── Inheritable additional attributes ───

    /// Mark element to swap in from a response (out of band).
    [<Extension>]
    static member hxSwapOob(this: #HtmlTag, value: string | null, [<Struct>] ?modifiers: string) =
        this.attr($"hx-swap-oob%s{getModifierSuffix modifiers}", value)

    /// Push a URL into the browser location bar to create history.
    [<Extension>]
    static member hxPushUrl(this: #HtmlTag, value: string | null, [<Struct>] ?modifiers: string) =
        this.attr($"hx-push-url%s{getModifierSuffix modifiers}", value)

    /// Include additional data in requests (CSS selector or extended selector like `closest form`).
    [<Extension>]
    static member hxInclude
        (this: #HtmlTag, [<StringSyntax("css")>] value: string | null, [<Struct>] ?modifiers: string)
        =
        this.attr($"hx-include%s{getModifierSuffix modifiers}", value)

    /// Select content to swap in from a response, somewhere other than the target (out of band).
    [<Extension>]
    static member hxSelectOob
        (this: #HtmlTag, [<StringSyntax("css")>] value: string | null, [<Struct>] ?modifiers: string)
        =
        this.attr($"hx-select-oob%s{getModifierSuffix modifiers}", value)

    /// Add progressive enhancement for links and forms.
    [<Extension>]
    static member hxBoost(this: #HtmlTag, value: string, [<Struct>] ?modifiers: string) =
        this.attr($"hx-boost%s{getModifierSuffix modifiers}", value)

    /// Replace the URL in the browser location bar.
    [<Extension>]
    static member hxReplaceUrl(this: #HtmlTag, value: string | null, [<Struct>] ?modifiers: string) =
        this.attr($"hx-replace-url%s{getModifierSuffix modifiers}", value)

    /// Show a confirm() dialog before issuing a request.
    [<Extension>]
    static member hxConfirm(this: #HtmlTag, value: string | null, [<Struct>] ?modifiers: string) =
        this.attr($"hx-confirm%s{getModifierSuffix modifiers}", value)

    /// Adds the `disabled` attribute to specified elements (CSS selector) while a request is in flight.
    /// `modifiers` may include `:merge` and/or `:inherited[:append]`, e.g. `":merge"`, `":inherited"`, `":merge:inherited"`.
    [<Extension>]
    static member hxDisable(this: #HtmlTag, [<StringSyntax("css")>] selector: string, [<Struct>] ?modifiers: string) =
        this.attr($"hx-disable%s{getModifierSuffix modifiers}", selector)

    /// Specifies elements to keep unchanged between requests. Renders `hx-preserve` (boolean attribute) when true.
    [<Extension>]
    static member hxPreserve(this: #HtmlTag, value: bool, [<Struct>] ?modifiers: string) =
        this.bool($"hx-preserve%s{getModifierSuffix modifiers}", value)

    /// Adds to the headers that will be submitted with the request (JSON object).
    [<Extension>]
    static member hxHeaders
        (this: #HtmlTag, [<StringSyntax("Json")>] value: string | null, [<Struct>] ?modifiers: string)
        =
        this.attr($"hx-headers%s{getModifierSuffix modifiers}", value)

    /// The element to put the `htmx-request` class on during the request (CSS selector).
    [<Extension>]
    static member hxIndicator
        (this: #HtmlTag, [<StringSyntax("css")>] value: string | null, [<Struct>] ?modifiers: string)
        =
        this.attr($"hx-indicator%s{getModifierSuffix modifiers}", value)

    /// Control how requests made by different elements are synchronized.
    [<Extension>]
    static member hxSync(this: #HtmlTag, value: string | null, [<Struct>] ?modifiers: string) =
        this.attr($"hx-sync%s{getModifierSuffix modifiers}", value)

    /// Force elements to validate themselves before a request.
    [<Extension>]
    static member hxValidate(this: #HtmlTag, value: bool, [<Struct>] ?modifiers: string) =
        this.bool($"hx-validate%s{getModifierSuffix modifiers}", value)

    /// Changes the request encoding type.
    [<Extension>]
    static member hxEncoding(this: #HtmlTag, value: string | null, [<Struct>] ?modifiers: string) =
        this.attr($"hx-encoding%s{getModifierSuffix modifiers}", value)

    /// Preload content before user triggers request.
    [<Extension>]
    static member hxPreload(this: #HtmlTag, value: string | null, [<Struct>] ?modifiers: string) =
        this.attr($"hx-preload%s{getModifierSuffix modifiers}", value)

    // ─── Non-inheritable additional attributes ───

    /// Specify URL to receive request (use with `hxMethod`).
    [<Extension>]
    static member hxAction(this: #HtmlTag, [<StringSyntax("Uri")>] value: string | null) = this.attr("hx-action", value)

    /// Specify HTTP method for request (use with `hxAction`).
    [<Extension>]
    static member hxMethod(this: #HtmlTag, value: string | null) = this.attr("hx-method", value)

    /// Configure request behavior with JSON.
    [<Extension>]
    static member hxConfig(this: #HtmlTag, [<StringSyntax("Json")>] value: string | null) =
        this.attr("hx-config", value)

    /// Disable htmx processing for the given node and any children. Renders `hx-ignore` only when true.
    [<Extension>]
    static member hxIgnore(this: #HtmlTag, value: bool) = this.bool("hx-ignore", value)

    /// Show optimistic content during request (template id, e.g. `#my-template`).
    [<Extension>]
    static member hxOptimistic(this: #HtmlTag, [<StringSyntax("css")>] value: string | null) =
        this.attr("hx-optimistic", value)

    /// Handle responses differently by status code (`hx-status:CODE`).
    /// `code` is an exact code (e.g. "404"), single-digit wildcard (e.g. "50x"),
    /// or range wildcard (e.g. "5xx"). `value` takes space-separated `key:value` pairs.
    [<Extension>]
    static member hxStatus(this: #HtmlTag, code: string, value: string) = this.attr($"hx-status:{code}", value)
