namespace Oxpecker.Htmx

open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices
open Oxpecker.ViewEngine


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
type HtmxCoreExtensions =

    /// Controls how content will swap in (outerHTML, beforeend, afterend, …).
    [<Extension>]
    static member hxSwap(this: #HtmlTag, value: string | null) = this.attr($"hx-swap", value)

    /// Controls how content will swap in (outerHTML, beforeend, afterend, …).
    [<Extension>]
    static member hxSwap(this: #HtmlTag, value: string | null, modifiers: string) =
        this.attr($"hx-swap%s{modifiers}", value)

    /// Specifies the target element to be swapped (CSS selector).
    [<Extension>]
    static member hxTarget(this: #HtmlTag, [<StringSyntax("css")>] value: string | null) = this.attr("hx-target", value)

    /// Specifies the target element to be swapped (CSS selector).
    [<Extension>]
    static member hxTarget(this: #HtmlTag, [<StringSyntax("css")>] value: string | null, modifiers: string) =
        this.attr($"hx-target%s{modifiers}", value)

    /// Specifies the event that triggers the request.
    [<Extension>]
    static member hxTrigger(this: #HtmlTag, value: string | null) = this.attr("hx-trigger", value)

    /// Specifies the event that triggers the request.
    [<Extension>]
    static member hxTrigger(this: #HtmlTag, value: string | null, modifiers: string) =
        this.attr($"hx-trigger%s{modifiers}", value)

    /// Select content to swap in from a response (CSS selector).
    [<Extension>]
    static member hxSelect(this: #HtmlTag, [<StringSyntax("css")>] value: string | null) = this.attr("hx-select", value)

    /// Select content to swap in from a response (CSS selector).
    [<Extension>]
    static member hxSelect(this: #HtmlTag, [<StringSyntax("css")>] value: string | null, modifiers: string) =
        this.attr($"hx-select%s{modifiers}", value)

    /// Add values to submit with the request (JSON format).
    [<Extension>]
    static member hxVals(this: #HtmlTag, [<StringSyntax("Json")>] value: string | null) = this.attr("hx-vals", value)

    /// Add values to submit with the request (JSON format).
    [<Extension>]
    static member hxVals(this: #HtmlTag, [<StringSyntax("Json")>] value: string | null, modifiers: string) =
        this.attr($"hx-vals%s{modifiers}", value)


/// Additional inheritable + non-inheritable attributes.
type HtmxAdditionalExtensions =

    // ─── Inheritable additional attributes ───

    /// Mark element to swap in from a response (out of band).
    [<Extension>]
    static member hxSwapOob(this: #HtmlTag, value: string | null) = this.attr("hx-swap-oob", value)

    /// Mark element to swap in from a response (out of band).
    [<Extension>]
    static member hxSwapOob(this: #HtmlTag, value: string | null, modifiers: string) =
        this.attr($"hx-swap-oob%s{modifiers}", value)

    /// Push a URL into the browser location bar to create history.
    [<Extension>]
    static member hxPushUrl(this: #HtmlTag, value: string | null) = this.attr("hx-push-url", value)

    /// Push a URL into the browser location bar to create history.
    [<Extension>]
    static member hxPushUrl(this: #HtmlTag, value: string | null, modifiers: string) =
        this.attr($"hx-push-url%s{modifiers}", value)

    /// Include additional data in requests (CSS selector or extended selector like `closest form`).
    [<Extension>]
    static member hxInclude(this: #HtmlTag, [<StringSyntax("css")>] value: string | null) =
        this.attr("hx-include", value)

    /// Include additional data in requests (CSS selector or extended selector like `closest form`).
    [<Extension>]
    static member hxInclude(this: #HtmlTag, [<StringSyntax("css")>] value: string | null, modifiers: string) =
        this.attr($"hx-include%s{modifiers}", value)

    /// Select content to swap in from a response, somewhere other than the target (out of band).
    [<Extension>]
    static member hxSelectOob(this: #HtmlTag, [<StringSyntax("css")>] value: string | null) =
        this.attr("hx-select-oob", value)

    /// Select content to swap in from a response, somewhere other than the target (out of band).
    [<Extension>]
    static member hxSelectOob(this: #HtmlTag, [<StringSyntax("css")>] value: string | null, modifiers: string) =
        this.attr($"hx-select-oob%s{modifiers}", value)

    /// Add progressive enhancement for links and forms.
    [<Extension>]
    static member hxBoost(this: #HtmlTag, value: string) = this.attr("hx-boost", value)

    /// Add progressive enhancement for links and forms.
    [<Extension>]
    static member hxBoost(this: #HtmlTag, value: string, modifiers: string) =
        this.attr($"hx-boost%s{modifiers}", value)

    /// Replace the URL in the browser location bar.
    [<Extension>]
    static member hxReplaceUrl(this: #HtmlTag, value: string | null) = this.attr("hx-replace-url", value)

    /// Replace the URL in the browser location bar.
    [<Extension>]
    static member hxReplaceUrl(this: #HtmlTag, value: string | null, modifiers: string) =
        this.attr($"hx-replace-url%s{modifiers}", value)

    /// Show a confirm() dialog before issuing a request.
    [<Extension>]
    static member hxConfirm(this: #HtmlTag, value: string | null) = this.attr("hx-confirm", value)

    /// Show a confirm() dialog before issuing a request.
    [<Extension>]
    static member hxConfirm(this: #HtmlTag, value: string | null, modifiers: string) =
        this.attr($"hx-confirm%s{modifiers}", value)

    /// Adds the `disabled` attribute to specified elements (CSS selector) while a request is in flight.
    [<Extension>]
    static member hxDisable(this: #HtmlTag, [<StringSyntax("css")>] selector: string) =
        this.attr("hx-disable", selector)

    /// Adds the `disabled` attribute to specified elements (CSS selector) while a request is in flight.
    /// `modifiers` may include `:merge` and/or `:inherited[:append]`, e.g. `":merge"`, `":inherited"`, `":merge:inherited"`.
    [<Extension>]
    static member hxDisable(this: #HtmlTag, [<StringSyntax("css")>] selector: string, modifiers: string) =
        this.attr($"hx-disable%s{modifiers}", selector)

    /// Specifies elements to keep unchanged between requests. Renders `hx-preserve` (boolean attribute) when true.
    [<Extension>]
    static member hxPreserve(this: #HtmlTag, value: bool) = this.bool("hx-preserve", value)

    /// Specifies elements to keep unchanged between requests. Renders `hx-preserve` (boolean attribute) when true.
    [<Extension>]
    static member hxPreserve(this: #HtmlTag, value: bool, modifiers: string) =
        this.bool($"hx-preserve%s{modifiers}", value)

    /// Adds to the headers that will be submitted with the request (JSON object).
    [<Extension>]
    static member hxHeaders(this: #HtmlTag, [<StringSyntax("Json")>] value: string | null) =
        this.attr("hx-headers", value)

    /// Adds to the headers that will be submitted with the request (JSON object).
    [<Extension>]
    static member hxHeaders(this: #HtmlTag, [<StringSyntax("Json")>] value: string | null, modifiers: string) =
        this.attr($"hx-headers%s{modifiers}", value)

    /// The element to put the `htmx-request` class on during the request (CSS selector).
    [<Extension>]
    static member hxIndicator(this: #HtmlTag, [<StringSyntax("css")>] value: string | null) =
        this.attr("hx-indicator", value)

    /// The element to put the `htmx-request` class on during the request (CSS selector).
    [<Extension>]
    static member hxIndicator(this: #HtmlTag, [<StringSyntax("css")>] value: string | null, modifiers: string) =
        this.attr($"hx-indicator%s{modifiers}", value)

    /// Control how requests made by different elements are synchronized.
    [<Extension>]
    static member hxSync(this: #HtmlTag, value: string | null) = this.attr("hx-sync", value)

    /// Control how requests made by different elements are synchronized.
    [<Extension>]
    static member hxSync(this: #HtmlTag, value: string | null, modifiers: string) =
        this.attr($"hx-sync%s{modifiers}", value)

    /// Force elements to validate themselves before a request.
    [<Extension>]
    static member hxValidate(this: #HtmlTag, value: bool) = this.bool("hx-validate", value)

    /// Force elements to validate themselves before a request.
    [<Extension>]
    static member hxValidate(this: #HtmlTag, value: bool, modifiers: string) =
        this.bool($"hx-validate%s{modifiers}", value)

    /// Changes the request encoding type.
    [<Extension>]
    static member hxEncoding(this: #HtmlTag, value: string | null) = this.attr("hx-encoding", value)

    /// Changes the request encoding type.
    [<Extension>]
    static member hxEncoding(this: #HtmlTag, value: string | null, modifiers: string) =
        this.attr($"hx-encoding%s{modifiers}", value)

    /// Preload content before user triggers request.
    [<Extension>]
    static member hxPreload(this: #HtmlTag, value: string | null) = this.attr("hx-preload", value)

    /// Preload content before user triggers request.
    [<Extension>]
    static member hxPreload(this: #HtmlTag, value: string | null, modifiers: string) =
        this.attr($"hx-preload%s{modifiers}", value)

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
