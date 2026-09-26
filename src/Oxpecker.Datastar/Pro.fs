namespace Oxpecker.Datastar.Pro

open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices
open Oxpecker.ViewEngine


/// Modifiers for Datastar Pro attributes.
/// Each value includes the leading `__` and is appended verbatim after the attribute key/name.
[<RequireQualifiedAccess>]
module DsProModifier =

    /// `__session` — persist signals in session storage instead of local storage (for `data-persist`).
    [<Literal>]
    let session = "__session"

    /// `__filter` — filter out empty values when syncing signal values to query string params.
    [<Literal>]
    let filter = "__filter"

    /// `__history` — enable history support for query string params (for `data-query-string`).
    [<Literal>]
    let history = "__history"

    /// `__smooth` — animate scrolling smoothly (for `data-scroll-into-view`).
    [<Literal>]
    let smooth = "__smooth"

    /// `__instant` — scroll instantly (for `data-scroll-into-view`).
    [<Literal>]
    let instant = "__instant"

    /// `__auto` — scroll according to the computed `scroll-behavior` CSS property (for `data-scroll-into-view`).
    [<Literal>]
    let auto = "__auto"

    /// `__hstart` — scroll to the left of the element.
    [<Literal>]
    let hstart = "__hstart"

    /// `__hcenter` — scroll to the horizontal center of the element.
    [<Literal>]
    let hcenter = "__hcenter"

    /// `__hend` — scroll to the right of the element.
    [<Literal>]
    let hend = "__hend"

    /// `__hnearest` — scroll to the nearest horizontal edge of the element.
    [<Literal>]
    let hnearest = "__hnearest"

    /// `__vstart` — scroll to the top of the element.
    [<Literal>]
    let vstart = "__vstart"

    /// `__vcenter` — scroll to the vertical center of the element.
    [<Literal>]
    let vcenter = "__vcenter"

    /// `__vend` — scroll to the bottom of the element.
    [<Literal>]
    let vend = "__vend"

    /// `__vnearest` — scroll to the nearest vertical edge of the element.
    [<Literal>]
    let vnearest = "__vnearest"

    /// `__focus` — focus the element after scrolling.
    [<Literal>]
    let focus = "__focus"


/// [Datastar Pro](https://data-star.dev/pro) attributes.
/// These attributes require the commercial Datastar Pro bundle loaded client-side.
type DatastarProExtensions =

    /// Animates element attributes over time. Renders `data-animate="…"`.
    [<Extension>]
    static member dataAnimate(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-animate", value)

    /// Adds custom validity to an element. Renders `data-custom-validity="…"`.
    [<Extension>]
    static member dataCustomValidity(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-custom-validity", value)

    /// Sets a signal to whether a media query matches. Renders `data-match-media:{signal}="…"`.
    [<Extension>]
    static member dataMatchMedia(this: #HtmlTag, signal: string, [<StringSyntax("js")>] query: string | null) =
        this.attr ($"data-match-media:%s{signal}", query)

    /// Sets a media-query signal with modifiers. Renders `data-match-media:{signal}{modifiers}="…"`.
    [<Extension>]
    static member dataMatchMedia
        (this: #HtmlTag, signal: string, [<StringSyntax("js")>] query: string | null, modifiers: string)
        =
        this.attr ($"data-match-media:%s{signal}%s{modifiers}", query)

    /// Runs an expression on every `requestAnimationFrame`. Renders `data-on-raf="…"`.
    [<Extension>]
    static member dataOnRaf(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-on-raf", value)

    /// Runs a `requestAnimationFrame` expression with modifiers. Renders `data-on-raf{modifiers}="…"`.
    [<Extension>]
    static member dataOnRaf(this: #HtmlTag, [<StringSyntax("js")>] value: string | null, modifiers: string) =
        this.attr ($"data-on-raf%s{modifiers}", value)

    /// Runs an expression whenever the element is resized. Renders `data-on-resize="…"`.
    [<Extension>]
    static member dataOnResize(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-on-resize", value)

    /// Runs a resize expression with modifiers. Renders `data-on-resize{modifiers}="…"`.
    [<Extension>]
    static member dataOnResize(this: #HtmlTag, [<StringSyntax("js")>] value: string | null, modifiers: string) =
        this.attr ($"data-on-resize%s{modifiers}", value)

    /// Persists signals in local storage. Valueless form renders `data-persist`.
    [<Extension>]
    static member dataPersist(this: #HtmlTag) = this.bool ("data-persist", true)

    /// Persists filtered signals in local storage. Renders `data-persist="…"`.
    [<Extension>]
    static member dataPersist(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-persist", value)

    /// Persists signals under a custom storage key. Renders `data-persist:{name}`.
    [<Extension>]
    static member dataPersistKey(this: #HtmlTag, name: string) =
        this.bool ($"data-persist:%s{name}", true)

    /// Persists signals under a custom storage key with modifiers (e.g. `DsProModifier.session`).
    /// Renders `data-persist:{name}{modifiers}`.
    [<Extension>]
    static member dataPersistKey(this: #HtmlTag, name: string, modifiers: string) =
        this.bool ($"data-persist:%s{name}%s{modifiers}", true)

    /// Syncs query string params to signals. Valueless form renders `data-query-string`.
    [<Extension>]
    static member dataQueryString(this: #HtmlTag) = this.bool ("data-query-string", true)

    /// Syncs filtered query string params to signals. Renders `data-query-string="…"`.
    [<Extension>]
    static member dataQueryString(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-query-string", value)

    /// Syncs query string params with modifiers (e.g. `DsProModifier.filter`, `DsProModifier.history`).
    /// Renders `data-query-string{modifiers}`.
    [<Extension>]
    static member dataQueryStringWith(this: #HtmlTag, modifiers: string) =
        this.bool ($"data-query-string%s{modifiers}", true)

    /// Replaces the browser URL without reloading. Renders `data-replace-url="…"`.
    [<Extension>]
    static member dataReplaceUrl(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-replace-url", value)

    /// Scrolls the element into view. Valueless form renders `data-scroll-into-view`.
    [<Extension>]
    static member dataScrollIntoView(this: #HtmlTag) =
        this.bool ("data-scroll-into-view", true)

    /// Scrolls the element into view with modifiers. Renders `data-scroll-into-view{modifiers}`.
    [<Extension>]
    static member dataScrollIntoView(this: #HtmlTag, modifiers: string) =
        this.bool ($"data-scroll-into-view%s{modifiers}", true)

    /// Sets the `view-transition-name` style attribute explicitly. Renders `data-view-transition="…"`.
    [<Extension>]
    static member dataViewTransition(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-view-transition", value)
