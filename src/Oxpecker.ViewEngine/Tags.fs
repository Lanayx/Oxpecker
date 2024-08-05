﻿namespace Oxpecker.ViewEngine

open System.Diagnostics.CodeAnalysis
open Oxpecker.ViewEngine.Tools
open System.Runtime.CompilerServices
open JetBrains.Annotations

[<AutoOpen>]
module Tags =

    open Oxpecker.ViewEngine.Builder

    /// Fragment (or template) node, only renders children, not itself
    type __() =
        inherit HtmlElement(null)

    /// Set of html extensions that keep original type
    [<Extension>]
    type HtmlElementExtensions =

        /// Add an attribute to the element
        [<Extension>]
        static member attr<'T when 'T :> HtmlElement>(this: 'T, name: string, value: string) =
            if isNotNull value then
                this.AddAttribute({ Name = name; Value = value })
            this

        /// Add event handler to the element through the corresponding attribute
        [<Extension>]
        static member on<'T when 'T :> HtmlElement>
            (this: 'T, eventName: string, [<StringSyntax("js")>] eventHandler: string)
            =
            this.attr($"on{eventName}", eventHandler)

        /// Add data attribute to the element
        [<Extension>]
        static member data<'T when 'T :> HtmlElement>(this: 'T, name: string, value: string) =
            this.attr($"data-{name}", value)


    // global attributes
    type HtmlElement with
        member this.id
            with set value = this.attr("id", value) |> ignore
        member this.class'
            with set value = this.attr("class", value) |> ignore
        [<LanguageInjection(InjectedLanguage.CSS, Prefix = ".x{", Suffix = ";}")>]
        member this.style
            with set value = this.attr("style", value) |> ignore
        member this.lang
            with set value = this.attr("lang", value) |> ignore
        member this.dir
            with set value = this.attr("dir", value) |> ignore
        member this.tabindex
            with set (value: int) = this.attr("tabindex", string value) |> ignore
        member this.title
            with set value = this.attr("title", value) |> ignore
        member this.accesskey
            with set (value: char) = this.attr("accesskey", string value) |> ignore
        member this.contenteditable
            with set (value: bool) = this.attr("contenteditable", (if value then "true" else "false")) |> ignore
        member this.draggable
            with set value = this.attr("draggable", value) |> ignore
        member this.enterkeyhint
            with set value = this.attr("enterkeyhint", value) |> ignore
        member this.hidden
            with set (value: bool) =
                if value then
                    this.attr("hidden", "") |> ignore
        member this.inert
            with set (value: bool) =
                if value then
                    this.attr("inert", "") |> ignore
        member this.inputmode
            with set value = this.attr("inputmode", value) |> ignore
        member this.popover
            with set (value: bool) =
                if value then
                    this.attr("popover", "") |> ignore
        member this.spellcheck
            with set (value: bool) = this.attr("spellcheck", (if value then "true" else "false")) |> ignore
        member this.translate
            with set (value: bool) = this.attr("translate", (if value then "yes" else "no")) |> ignore

    type head() =
        inherit HtmlElement("head")
    type body() =
        inherit HtmlElement("body")
    type title() =
        inherit HtmlElement("title")
    type div() =
        inherit HtmlElement("div")
    type article() =
        inherit HtmlElement("article")
    type section() =
        inherit HtmlElement("section")
    type header() =
        inherit HtmlElement("header")
    type footer() =
        inherit HtmlElement("footer")
    type main() =
        inherit HtmlElement("main")
    type h1() =
        inherit HtmlElement("h1")
    type h2() =
        inherit HtmlElement("h2")
    type h3() =
        inherit HtmlElement("h3")
    type h4() =
        inherit HtmlElement("h4")
    type h5() =
        inherit HtmlElement("h5")
    type h6() =
        inherit HtmlElement("h6")
    type ul() =
        inherit HtmlElement("ul")
    type ol() =
        inherit HtmlElement("ol")
    type li() =
        inherit HtmlElement("li")
    type p() =
        inherit HtmlElement("p")
    type span() =
        inherit HtmlElement("span")
    type strong() =
        inherit HtmlElement("strong")
    type em() =
        inherit HtmlElement("em")
    type caption() =
        inherit HtmlElement("caption")
    type nav() =
        inherit HtmlElement("nav")
    type i() =
        inherit HtmlElement("i")
    type b() =
        inherit HtmlElement("b")
    type u() =
        inherit HtmlElement("u")
    type s() =
        inherit HtmlElement("s")
    type noscript() =
        inherit HtmlElement("noscript")
    type code() =
        inherit HtmlElement("code")
    type pre() =
        inherit HtmlElement("pre")
    type blockquote() =
        inherit HtmlElement("blockquote")
    type cite() =
        inherit HtmlElement("cite")
    type q() =
        inherit HtmlElement("q")
    type address() =
        inherit HtmlElement("address")
    type del() =
        inherit HtmlElement("del")
    type ins() =
        inherit HtmlElement("ins")
    type abbr() =
        inherit HtmlElement("abbr")
    type dfn() =
        inherit HtmlElement("dfn")
    type sub() =
        inherit HtmlElement("sub")
    type sup() =
        inherit HtmlElement("sup")
    type template() =
        inherit HtmlElement("template")

    type br() =
        inherit
            HtmlElement(
                {
                    NodeType = NodeType.VoidNode
                    Value = "br"
                }
            )
    type hr() =
        inherit
            HtmlElement(
                {
                    NodeType = NodeType.VoidNode
                    Value = "hr"
                }
            )

    type a() =
        inherit HtmlElement("a")
        member this.href
            with set value = this.attr("href", value) |> ignore
        member this.rel
            with set value = this.attr("rel", value) |> ignore
        member this.target
            with set value = this.attr("target", value) |> ignore
        member this.download
            with set value = this.attr("download", value) |> ignore

    type base'() =
        inherit
            HtmlElement(
                {
                    NodeType = NodeType.VoidNode
                    Value = "base"
                }
            )
        member this.href
            with set value = this.attr("href", value) |> ignore
        member this.target
            with set value = this.attr("target", value) |> ignore

    type img() =
        inherit
            HtmlElement(
                {
                    NodeType = NodeType.VoidNode
                    Value = "img"
                }
            )
        member this.src
            with set value = this.attr("src", value) |> ignore
        member this.alt
            with set value = this.attr("alt", value) |> ignore
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore

    type form() =
        inherit HtmlElement("form")
        member this.action
            with set value = this.attr("action", value) |> ignore
        member this.method
            with set value = this.attr("method", value) |> ignore
        member this.enctype
            with set value = this.attr("enctype", value) |> ignore
        member this.target
            with set value = this.attr("target", value) |> ignore

    type script() =
        inherit HtmlElement("script")
        member this.src
            with set value = this.attr("src", value) |> ignore
        member this.type'
            with set value = this.attr("type", value) |> ignore
        member this.async
            with set (value: bool) =
                if value then
                    this.attr("async", "") |> ignore
        member this.defer
            with set (value: bool) =
                if value then
                    this.attr("defer", "") |> ignore
        member this.integrity
            with set value = this.attr("integrity", value) |> ignore
        member this.crossorigin
            with set value = this.attr("crossorigin", value) |> ignore

    type link() =
        inherit
            HtmlElement(
                {
                    NodeType = NodeType.VoidNode
                    Value = "link"
                }
            )
        member this.rel
            with set value = this.attr("rel", value) |> ignore
        member this.href
            with set value = this.attr("href", value) |> ignore
        member this.type'
            with set value = this.attr("type", value) |> ignore
        member this.media
            with set value = this.attr("media", value) |> ignore
        member this.as'
            with set value = this.attr("as", value) |> ignore
        member this.sizes
            with set value = this.attr("sizes", value) |> ignore

    type html() =
        inherit HtmlElement("html")
        member this.xmlns
            with set value = this.attr("xmlns", value) |> ignore

    type meta() =
        inherit
            HtmlElement(
                {
                    NodeType = NodeType.VoidNode
                    Value = "meta"
                }
            )
        member this.name
            with set value = this.attr("name", value) |> ignore
        member this.content
            with set value = this.attr("content", value) |> ignore
        member this.charset
            with set value = this.attr("charset", value) |> ignore
        member this.httpEquiv
            with set value = this.attr("http-equiv", value) |> ignore

    type input() =
        inherit
            HtmlElement(
                {
                    NodeType = NodeType.VoidNode
                    Value = "input"
                }
            )
        member this.type'
            with set value = this.attr("type", value) |> ignore
        member this.name
            with set value = this.attr("name", value) |> ignore
        member this.value
            with set value = this.attr("value", value) |> ignore
        member this.placeholder
            with set value = this.attr("placeholder", value) |> ignore
        member this.required
            with set (value: bool) =
                if value then
                    this.attr("required", "") |> ignore
        member this.autofocus
            with set (value: bool) =
                if value then
                    this.attr("autofocus", "") |> ignore
        member this.autocomplete
            with set value = this.attr("autocomplete", value) |> ignore
        member this.min
            with set value = this.attr("min", value) |> ignore
        member this.max
            with set value = this.attr("max", value) |> ignore
        member this.step
            with set value = this.attr("step", value) |> ignore
        member this.pattern
            with set value = this.attr("pattern", value) |> ignore
        member this.readonly
            with set (value: bool) =
                if value then
                    this.attr("readonly", "") |> ignore
        member this.disabled
            with set (value: bool) =
                if value then
                    this.attr("disabled", "") |> ignore
        member this.multiple
            with set (value: bool) =
                if value then
                    this.attr("multiple", "") |> ignore
        member this.accept
            with set value = this.attr("accept", value) |> ignore
        member this.list
            with set value = this.attr("list", value) |> ignore
        member this.maxlength
            with set (value: int) = this.attr("maxlength", string value) |> ignore
        member this.minlength
            with set (value: int) = this.attr("minlength", string value) |> ignore
        member this.size
            with set (value: int) = this.attr("size", string value) |> ignore
        member this.src
            with set value = this.attr("src", value) |> ignore
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore
        member this.alt
            with set value = this.attr("alt", value) |> ignore

    type output() =
        inherit HtmlElement("output")
        member this.for'
            with set value = this.attr("for", value) |> ignore
        member this.form
            with set value = this.attr("form", value) |> ignore
        member this.name
            with set value = this.attr("name", value) |> ignore

    type textarea() =
        inherit HtmlElement("textarea")
        member this.name
            with set value = this.attr("name", value) |> ignore
        member this.placeholder
            with set value = this.attr("placeholder", value) |> ignore
        member this.required
            with set (value: bool) =
                if value then
                    this.attr("required", "") |> ignore
        member this.autofocus
            with set (value: bool) =
                if value then
                    this.attr("autofocus", "") |> ignore
        member this.readonly
            with set (value: bool) =
                if value then
                    this.attr("readonly", "") |> ignore
        member this.disabled
            with set (value: bool) =
                if value then
                    this.attr("disabled", "") |> ignore
        member this.rows
            with set (value: int) = this.attr("rows", string value) |> ignore
        member this.cols
            with set (value: int) = this.attr("cols", string value) |> ignore
        member this.wrap
            with set value = this.attr("wrap", value) |> ignore
        member this.maxlength
            with set (value: int) = this.attr("maxlength", string value) |> ignore

    type button() =
        inherit HtmlElement("button")
        member this.type'
            with set value = this.attr("type", value) |> ignore
        member this.name
            with set value = this.attr("name", value) |> ignore
        member this.value
            with set value = this.attr("value", value) |> ignore
        member this.disabled
            with set (value: bool) =
                if value then
                    this.attr("disabled", "") |> ignore
        member this.autofocus
            with set (value: bool) =
                if value then
                    this.attr("autofocus", "") |> ignore

    type select() =
        inherit HtmlElement("select")
        member this.name
            with set value = this.attr("name", value) |> ignore
        member this.required
            with set (value: bool) =
                if value then
                    this.attr("required", "") |> ignore
        member this.autofocus
            with set (value: bool) =
                if value then
                    this.attr("autofocus", "") |> ignore
        member this.disabled
            with set (value: bool) =
                if value then
                    this.attr("disabled", "") |> ignore
        member this.multiple
            with set (value: bool) =
                if value then
                    this.attr("multiple", "") |> ignore
        member this.size
            with set (value: int) = this.attr("size", string value) |> ignore

    type option() =
        inherit HtmlElement("option")
        member this.value
            with set value = this.attr("value", value) |> ignore
        member this.selected
            with set (value: bool) =
                if value then
                    this.attr("selected", "") |> ignore
        member this.disabled
            with set (value: bool) =
                if value then
                    this.attr("disabled", "") |> ignore
        member this.label
            with set value = this.attr("label", value) |> ignore

    type optgroup() =
        inherit HtmlElement("optgroup")
        member this.label
            with set value = this.attr("label", value) |> ignore
        member this.disabled
            with set (value: bool) =
                if value then
                    this.attr("disabled", "") |> ignore

    type label() =
        inherit HtmlElement("label")
        member this.for'
            with set value = this.attr("for", value) |> ignore

    type style() =
        inherit HtmlElement("style")
        member this.type'
            with set value = this.attr("type", value) |> ignore
        member this.media
            with set value = this.attr("media", value) |> ignore

    type iframe() =
        inherit HtmlElement("iframe")
        member this.src
            with set value = this.attr("src", value) |> ignore
        member this.name
            with set value = this.attr("name", value) |> ignore
        member this.sandbox
            with set value = this.attr("sandbox", value) |> ignore
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore
        member this.allowfullscreen
            with set (value: bool) = this.attr("allowfullscreen", (if value then "true" else "false")) |> ignore
        member this.allowpaymentrequest
            with set (value: bool) = this.attr("allowpaymentrequest", (if value then "true" else "false")) |> ignore
        member this.loading
            with set value = this.attr("loading", value) |> ignore
        member this.referrerpolicy
            with set value = this.attr("referrerpolicy", value) |> ignore
        member this.srcdoc
            with set value = this.attr("srcdoc", value) |> ignore

    type video() =
        inherit HtmlElement("video")
        member this.src
            with set value = this.attr("src", value) |> ignore
        member this.poster
            with set value = this.attr("poster", value) |> ignore
        member this.autoplay
            with set (value: bool) =
                if value then
                    this.attr("autoplay", "") |> ignore
        member this.controls
            with set (value: bool) =
                if value then
                    this.attr("controls", "") |> ignore
        member this.loop
            with set (value: bool) =
                if value then
                    this.attr("loop", "") |> ignore
        member this.muted
            with set (value: bool) =
                if value then
                    this.attr("muted", "") |> ignore
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore
        member this.preload
            with set value = this.attr("preload", value) |> ignore

    type audio() =
        inherit HtmlElement("audio")
        member this.src
            with set value = this.attr("src", value) |> ignore
        member this.autoplay
            with set (value: bool) =
                if value then
                    this.attr("autoplay", "") |> ignore
        member this.controls
            with set (value: bool) =
                if value then
                    this.attr("controls", "") |> ignore
        member this.loop
            with set (value: bool) =
                if value then
                    this.attr("loop", "") |> ignore
        member this.muted
            with set (value: bool) =
                if value then
                    this.attr("muted", "") |> ignore

    type source() =
        inherit
            HtmlElement(
                {
                    NodeType = NodeType.VoidNode
                    Value = "source"
                }
            )
        member this.src
            with set value = this.attr("src", value) |> ignore
        member this.type'
            with set value = this.attr("type", value) |> ignore
        member this.media
            with set value = this.attr("media", value) |> ignore
        member this.sizes
            with set value = this.attr("sizes", value) |> ignore
        member this.srcset
            with set value = this.attr("srcset", value) |> ignore

    type canvas() =
        inherit HtmlElement("canvas")
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore

    type object'() =
        inherit HtmlElement("object")
        member this.data
            with set value = this.attr("data", value) |> ignore
        member this.type'
            with set value = this.attr("type", value) |> ignore
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore

    type param() =
        inherit
            HtmlElement(
                {
                    NodeType = NodeType.VoidNode
                    Value = "param"
                }
            )
        member this.name
            with set value = this.attr("name", value) |> ignore
        member this.value
            with set value = this.attr("value", value) |> ignore

    type data() =
        inherit HtmlElement("data")
        member this.value
            with set value = this.attr("value", value) |> ignore

    type time() =
        inherit HtmlElement("time")
        member this.datetime
            with set value = this.attr("datetime", value) |> ignore

    type progress() =
        inherit HtmlElement("progress")
        member this.value
            with set value = this.attr("value", value) |> ignore
        member this.max
            with set value = this.attr("max", value) |> ignore

    type meter() =
        inherit HtmlElement("meter")
        member this.form
            with set value = this.attr("form", value) |> ignore
        member this.value
            with set value = this.attr("value", value) |> ignore
        member this.min
            with set value = this.attr("min", value) |> ignore
        member this.max
            with set value = this.attr("max", value) |> ignore
        member this.low
            with set value = this.attr("low", value) |> ignore
        member this.high
            with set value = this.attr("high", value) |> ignore
        member this.optimum
            with set value = this.attr("optimum", value) |> ignore

    type details() =
        inherit HtmlElement("details")
        member this.open'
            with set (value: bool) =
                if value then
                    this.attr("open", "") |> ignore

    type summary() =
        inherit HtmlElement("summary")

    type dialog() =
        inherit HtmlElement("dialog")
        member this.open'
            with set (value: bool) =
                if value then
                    this.attr("open", "") |> ignore

    type menu() =
        inherit HtmlElement("menu")

    type datalist() =
        inherit HtmlElement("datalist")

    type fieldset() =
        inherit HtmlElement("fieldset")
        member this.disabled
            with set (value: bool) =
                if value then
                    this.attr("disabled", "") |> ignore
        member this.form
            with set value = this.attr("form", value) |> ignore
        member this.name
            with set value = this.attr("name", value) |> ignore

    type legend() =
        inherit HtmlElement("legend")


    type table() =
        inherit HtmlElement("table")
    type tbody() =
        inherit HtmlElement("tbody")
    type thead() =
        inherit HtmlElement("thead")
    type tfoot() =
        inherit HtmlElement("tfoot")
    type tr() =
        inherit HtmlElement("tr")
    type th() =
        inherit HtmlElement("th")
        member this.abbr
            with set value = this.attr("abbr", value) |> ignore
        member this.colspan
            with set (value: int) = this.attr("colspan", string value) |> ignore
        member this.rowspan
            with set (value: int) = this.attr("rowspan", string value) |> ignore
        member this.headers
            with set value = this.attr("headers", value) |> ignore
        member this.scope
            with set value = this.attr("scope", value) |> ignore
    type td() =
        inherit HtmlElement("td")
        member this.colspan
            with set (value: int) = this.attr("colspan", string value) |> ignore
        member this.rowspan
            with set (value: int) = this.attr("rowspan", string value) |> ignore
        member this.headers
            with set value = this.attr("headers", value) |> ignore
