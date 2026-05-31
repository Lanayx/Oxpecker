namespace Oxpecker.Htmx.Extensions

open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices
open Oxpecker.ViewEngine


/// Values for the `hx-head` attribute provided by the `hx-head` (head-support) extension.
/// See https://four.htmx.org/extensions/hx-head
[<RequireQualifiedAccess>]
module HxHeadMode =
    /// `merge` — follow the head merging algorithm (default for boosted requests).
    [<Literal>]
    let merge = "merge"
    /// `append` — append the response head elements to the existing head (default for non-boosted requests).
    [<Literal>]
    let append = "append"
    /// `re-eval` — placed on an individual head element, re-add it (remove and append) on every request.
    [<Literal>]
    let reEval = "re-eval"

/// Modifiers for the `upsert` swap style provided by the `hx-upsert` extension.
/// Append these to `HxSwapMethod.upsert` when calling `hxSwap`, e.g. `HxSwapMethod.upsert + HxUpsertModifier.sort`.
/// See https://four.htmx.org/extensions/hx-upsert
[<RequireQualifiedAccess>]
module HxUpsertModifier =
    /// `sort` — keep elements in ascending order by ID.
    let sort = " sort"
    /// `sort:desc` — keep elements in descending order by ID.
    let sortDesc = " sort:desc"
    /// `prepend` — insert unkeyed (id-less) elements at the beginning instead of appending them.
    let prepend = " prepend"
    /// `key:<attr>` — sort by a different attribute than `id`.
    let key (attr: string) = $" key:%s{attr}"


/// Attributes provided by the built-in htmx 4 extensions.
/// Each extension must be loaded client-side via its own script tag.
/// See https://four.htmx.org/extensions
type HtmxExtensionExtensions =

    // ─── hx-sse (Server-Sent Events) ───

    /// `hx-sse:connect` — open a persistent SSE connection to the URL (auto-connect, reconnect with backoff).
    /// Requires the `hx-sse` extension.
    [<Extension>]
    static member hxSseConnect(this: #HtmlTag, [<StringSyntax("Uri")>] url: string | null) =
        this.attr("hx-sse:connect", url)

    /// `hx-sse:close` — gracefully close the SSE connection when the named server event is received.
    /// Requires the `hx-sse` extension.
    [<Extension>]
    static member hxSseClose(this: #HtmlTag, event: string | null) = this.attr("hx-sse:close", event)

    // ─── hx-ws (WebSockets) ───

    /// `hx-ws:connect` — establish a WebSocket connection to the URL.
    /// Requires the `hx-ws` extension.
    [<Extension>]
    static member hxWsConnect(this: #HtmlTag, [<StringSyntax("Uri")>] url: string | null) =
        this.attr("hx-ws:connect", url)

    /// `hx-ws:send` — send form data / `hx-vals` to the active WebSocket when the element is triggered.
    /// Renders `hx-ws:send` (boolean attribute) when true. Requires the `hx-ws` extension.
    [<Extension>]
    static member hxWsSend(this: #HtmlTag, value: bool) = this.bool("hx-ws:send", value)

    /// `hx-ws:send="<url>"` — like `hxWsSend`, but opens its own WebSocket connection to the URL.
    /// Requires the `hx-ws` extension.
    [<Extension>]
    static member hxWsSend(this: #HtmlTag, [<StringSyntax("Uri")>] url: string | null) = this.attr("hx-ws:send", url)

    // ─── hx-head (head merging) ───

    /// `hx-head` — control how the response `<head>` is merged (`merge`, `append`, or `re-eval`).
    /// See `HxHeadMode`. Requires the `hx-head` extension.
    [<Extension>]
    static member hxHead(this: #HtmlTag, value: string | null) = this.attr("hx-head", value)

    // ─── hx-targets (multi-target swap) ───

    /// `hx-targets` — swap the response into every element matching the CSS selector. Inheritable.
    /// Requires the `hx-targets` extension.
    [<Extension>]
    static member hxTargets(this: #HtmlTag, [<StringSyntax("css")>] value: string | null) =
        this.attr("hx-targets", value)

    /// `hx-targets` — swap the response into every element matching the CSS selector. Inheritable.
    /// Requires the `hx-targets` extension.
    [<Extension>]
    static member hxTargets(this: #HtmlTag, [<StringSyntax("css")>] value: string | null, modifiers: string) =
        this.attr($"hx-targets%s{modifiers}", value)

    // ─── hx-ptag (polling tags) ───

    /// `hx-ptag` — set the initial polling tag sent with the first request. Requires the `hx-ptag` extension.
    [<Extension>]
    static member hxPtag(this: #HtmlTag, value: string | null) = this.attr("hx-ptag", value)

    // ─── hx-browser-indicator ───

    /// `hx-browser-indicator` — show the browser's native loading indicator during requests.
    /// Renders `hx-browser-indicator="true"`/`"false"`. Requires the `hx-browser-indicator` extension.
    [<Extension>]
    static member hxBrowserIndicator(this: #HtmlTag, value: bool) =
        this.attr("hx-browser-indicator", (if value then "true" else "false"))

    // ─── hx-history-cache ───

    /// `hx-history` — when set to `false`, exclude this page from the history cache.
    /// Renders `hx-history="true"`/`"false"`. Requires the `hx-history-cache` extension.
    [<Extension>]
    static member hxHistory(this: #HtmlTag, value: bool) =
        this.attr("hx-history", (if value then "true" else "false"))

    // ─── hx-csp (Content Security Policy) ───

    /// `hx-nonce` — CSP nonce proving the element was rendered by trusted server-side code.
    /// Requires the `hx-csp` extension.
    [<Extension>]
    static member hxNonce(this: #HtmlTag, value: string | null) = this.attr("hx-nonce", value)

    // ─── hx-live (reactive expressions) ───

    /// `hx-live` — a JavaScript expression that re-runs whenever the DOM changes.
    /// Requires the `hx-live` extension.
    [<Extension>]
    static member hxLive(this: #HtmlTag, [<StringSyntax("js")>] expression: string | null) =
        this.attr("hx-live", expression)
