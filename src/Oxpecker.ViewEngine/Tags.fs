namespace Oxpecker.ViewEngine

open System.Diagnostics.CodeAnalysis
open Oxpecker.ViewEngine.Tools
open System.Runtime.CompilerServices
open JetBrains.Annotations

[<AutoOpen>]
module Tags =

    open Oxpecker.ViewEngine.Builder

    /// Fragment (or template) node, only renders children, not itself
    type __() =
        inherit FragmentNode()

    /// Set of html extensions that keep original type
    [<Extension>]
    type HtmlElementExtensions =

        /// Add an attribute to the element
        [<Extension>]
        static member attr(this: #HtmlTag, name: string, value: string | null) =
            if isNotNull value then
                this.AddAttribute({ Name = name; Value = value })
            this

        /// Add event handler to the element through the corresponding attribute
        [<Extension>]
        static member on(this: #HtmlTag, eventName: string, [<StringSyntax("js")>] eventHandler: string) =
            this.attr($"on%s{eventName}", eventHandler)

        /// Add data attribute to the element
        [<Extension>]
        static member data(this: #HtmlTag, name: string, value: string) = this.attr($"data-%s{name}", value)

    // global attributes
    type HtmlTag with
        member this.id
            with set (value: string | null) = this.attr("id", value) |> ignore
        member this.class'
            with set (value: string | null) = this.attr("class", value) |> ignore
        [<LanguageInjection(InjectedLanguage.CSS, Prefix = ".x{", Suffix = ";}")>]
        member this.style
            with set (value: string | null) = this.attr("style", value) |> ignore
        member this.lang
            with set (value: string | null) = this.attr("lang", value) |> ignore
        member this.dir
            with set (value: string | null) = this.attr("dir", value) |> ignore
        member this.tabindex
            with set (value: int) = this.attr("tabindex", string value) |> ignore
        member this.title
            with set (value: string | null) = this.attr("title", value) |> ignore
        member this.accesskey
            with set (value: char) = this.attr("accesskey", string value) |> ignore
        member this.contenteditable
            with set (value: bool) = this.attr("contenteditable", (if value then "true" else "false")) |> ignore
        member this.draggable
            with set (value: string | null) = this.attr("draggable", value) |> ignore
        member this.enterkeyhint
            with set (value: string | null) = this.attr("enterkeyhint", value) |> ignore
        member this.hidden
            with set (value: bool) =
                if value then
                    this.attr("hidden", "") |> ignore
        member this.inert
            with set (value: bool) =
                if value then
                    this.attr("inert", "") |> ignore
        member this.inputmode
            with set (value: string | null) = this.attr("inputmode", value) |> ignore
        member this.popover
            with set (value: bool) =
                if value then
                    this.attr("popover", "") |> ignore
        member this.spellcheck
            with set (value: bool) = this.attr("spellcheck", (if value then "true" else "false")) |> ignore
        member this.translate
            with set (value: bool) = this.attr("translate", (if value then "yes" else "no")) |> ignore

    type head() =
        inherit RegularNode("head")
    type body() =
        inherit RegularNode("body")
    type title() =
        inherit RegularNode("title")
    type div() =
        inherit RegularNode("div")
    type article() =
        inherit RegularNode("article")
    type section() =
        inherit RegularNode("section")
    type header() =
        inherit RegularNode("header")
    type footer() =
        inherit RegularNode("footer")
    type main() =
        inherit RegularNode("main")
    type h1() =
        inherit RegularNode("h1")
    type h2() =
        inherit RegularNode("h2")
    type h3() =
        inherit RegularNode("h3")
    type h4() =
        inherit RegularNode("h4")
    type h5() =
        inherit RegularNode("h5")
    type h6() =
        inherit RegularNode("h6")
    type ul() =
        inherit RegularNode("ul")
    type ol() =
        inherit RegularNode("ol")
    type li() =
        inherit RegularNode("li")
    type p() =
        inherit RegularNode("p")
    type span() =
        inherit RegularNode("span")
    type strong() =
        inherit RegularNode("strong")
    type em() =
        inherit RegularNode("em")
    type caption() =
        inherit RegularNode("caption")
    type nav() =
        inherit RegularNode("nav")
    type i() =
        inherit RegularNode("i")
    type b() =
        inherit RegularNode("b")
    type u() =
        inherit RegularNode("u")
    type s() =
        inherit RegularNode("s")
    type noscript() =
        inherit RegularNode("noscript")
    type code() =
        inherit RegularNode("code")
    type pre() =
        inherit RegularNode("pre")
    type blockquote() =
        inherit RegularNode("blockquote")
    type cite() =
        inherit RegularNode("cite")
    type q() =
        inherit RegularNode("q")
    type address() =
        inherit RegularNode("address")
    type del() =
        inherit RegularNode("del")
    type ins() =
        inherit RegularNode("ins")
    type abbr() =
        inherit RegularNode("abbr")
    type dfn() =
        inherit RegularNode("dfn")
    type sub() =
        inherit RegularNode("sub")
    type sup() =
        inherit RegularNode("sup")
    type template() =
        inherit RegularNode("template")

    type br() =
        inherit VoidNode("br")

    type hr() =
        inherit VoidNode("hr")

    type a() =
        inherit RegularNode("a")
        member this.href
            with set (value: string | null) = this.attr("href", value) |> ignore
        member this.rel
            with set (value: string | null) = this.attr("rel", value) |> ignore
        member this.target
            with set (value: string | null) = this.attr("target", value) |> ignore
        member this.download
            with set (value: string | null) = this.attr("download", value) |> ignore

    type base'() =
        inherit VoidNode("base")
        member this.href
            with set (value: string | null) = this.attr("href", value) |> ignore
        member this.target
            with set (value: string | null) = this.attr("target", value) |> ignore

    type img() =
        inherit VoidNode("img")
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        member this.alt
            with set (value: string | null) = this.attr("alt", value) |> ignore
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore
        member this.srcset
            with set (value: string | null) = this.attr("srcset", value) |> ignore
        member this.referrerpolicy
            with set (value: string | null) = this.attr("referrerpolicy", value) |> ignore
        member this.crossorigin
            with set (value: string | null) = this.attr("crossorigin", value) |> ignore
        member this.sizes
            with set (value: string | null) = this.attr("sizes", value) |> ignore

    type form() =
        inherit RegularNode("form")
        member this.action
            with set (value: string | null) = this.attr("action", value) |> ignore
        member this.method
            with set (value: string | null) = this.attr("method", value) |> ignore
        member this.enctype
            with set (value: string | null) = this.attr("enctype", value) |> ignore
        member this.target
            with set (value: string | null) = this.attr("target", value) |> ignore

    type script() =
        inherit RegularNode("script")
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        member this.async
            with set (value: bool) =
                if value then
                    this.attr("async", "") |> ignore
        member this.defer
            with set (value: bool) =
                if value then
                    this.attr("defer", "") |> ignore
        member this.integrity
            with set (value: string | null) = this.attr("integrity", value) |> ignore
        member this.crossorigin
            with set (value: string | null) = this.attr("crossorigin", value) |> ignore

    type link() =
        inherit VoidNode("link")
        member this.rel
            with set (value: string | null) = this.attr("rel", value) |> ignore
        member this.href
            with set (value: string | null) = this.attr("href", value) |> ignore
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        member this.media
            with set (value: string | null) = this.attr("media", value) |> ignore
        member this.as'
            with set (value: string | null) = this.attr("as", value) |> ignore
        member this.sizes
            with set (value: string | null) = this.attr("sizes", value) |> ignore

    type html() =
        inherit RegularNode("html")
        member this.xmlns
            with set (value: string | null) = this.attr("xmlns", value) |> ignore

    type meta() =
        inherit VoidNode("meta")
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        member this.content
            with set (value: string | null) = this.attr("content", value) |> ignore
        member this.charset
            with set (value: string | null) = this.attr("charset", value) |> ignore
        member this.httpEquiv
            with set (value: string | null) = this.attr("http-equiv", value) |> ignore

    type input() =
        inherit VoidNode("input")
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore
        member this.placeholder
            with set (value: string | null) = this.attr("placeholder", value) |> ignore
        member this.required
            with set (value: bool) =
                if value then
                    this.attr("required", "") |> ignore
        member this.autofocus
            with set (value: bool) =
                if value then
                    this.attr("autofocus", "") |> ignore
        member this.autocomplete
            with set (value: string | null) = this.attr("autocomplete", value) |> ignore
        member this.min
            with set (value: string | null) = this.attr("min", value) |> ignore
        member this.max
            with set (value: string | null) = this.attr("max", value) |> ignore
        member this.step
            with set (value: string | null) = this.attr("step", value) |> ignore
        member this.pattern
            with set (value: string | null) = this.attr("pattern", value) |> ignore
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
            with set (value: string | null) = this.attr("accept", value) |> ignore
        member this.list
            with set (value: string | null) = this.attr("list", value) |> ignore
        member this.maxlength
            with set (value: int) = this.attr("maxlength", string value) |> ignore
        member this.minlength
            with set (value: int) = this.attr("minlength", string value) |> ignore
        member this.size
            with set (value: int) = this.attr("size", string value) |> ignore
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore
        member this.alt
            with set (value: string | null) = this.attr("alt", value) |> ignore

    type output() =
        inherit RegularNode("output")
        member this.for'
            with set (value: string | null) = this.attr("for", value) |> ignore
        member this.form
            with set (value: string | null) = this.attr("form", value) |> ignore
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore

    type textarea() =
        inherit RegularNode("textarea")
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        member this.placeholder
            with set (value: string | null) = this.attr("placeholder", value) |> ignore
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
            with set (value: string | null) = this.attr("wrap", value) |> ignore
        member this.maxlength
            with set (value: int) = this.attr("maxlength", string value) |> ignore

    type button() =
        inherit RegularNode("button")
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore
        member this.disabled
            with set (value: bool) =
                if value then
                    this.attr("disabled", "") |> ignore
        member this.autofocus
            with set (value: bool) =
                if value then
                    this.attr("autofocus", "") |> ignore

    type select() =
        inherit RegularNode("select")
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
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
        inherit RegularNode("option")
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore
        member this.selected
            with set (value: bool) =
                if value then
                    this.attr("selected", "") |> ignore
        member this.disabled
            with set (value: bool) =
                if value then
                    this.attr("disabled", "") |> ignore
        member this.label
            with set (value: string | null) = this.attr("label", value) |> ignore

    type optgroup() =
        inherit RegularNode("optgroup")
        member this.label
            with set (value: string | null) = this.attr("label", value) |> ignore
        member this.disabled
            with set (value: bool) =
                if value then
                    this.attr("disabled", "") |> ignore

    type label() =
        inherit RegularNode("label")
        member this.for'
            with set (value: string | null) = this.attr("for", value) |> ignore

    type style() =
        inherit RegularNode("style")
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        member this.media
            with set (value: string | null) = this.attr("media", value) |> ignore

    type iframe() =
        inherit RegularNode("iframe")
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        member this.sandbox
            with set (value: string | null) = this.attr("sandbox", value) |> ignore
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore
        member this.allowfullscreen
            with set (value: bool) = this.attr("allowfullscreen", (if value then "true" else "false")) |> ignore
        member this.allowpaymentrequest
            with set (value: bool) = this.attr("allowpaymentrequest", (if value then "true" else "false")) |> ignore
        member this.loading
            with set (value: string | null) = this.attr("loading", value) |> ignore
        member this.referrerpolicy
            with set (value: string | null) = this.attr("referrerpolicy", value) |> ignore
        member this.srcdoc
            with set (value: string | null) = this.attr("srcdoc", value) |> ignore

    type video() =
        inherit RegularNode("video")
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        member this.poster
            with set (value: string | null) = this.attr("poster", value) |> ignore
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
            with set (value: string | null) = this.attr("preload", value) |> ignore

    type audio() =
        inherit RegularNode("audio")
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
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
        inherit VoidNode("source")
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        member this.media
            with set (value: string | null) = this.attr("media", value) |> ignore
        member this.sizes
            with set (value: string | null) = this.attr("sizes", value) |> ignore
        member this.srcset
            with set (value: string | null) = this.attr("srcset", value) |> ignore

    type canvas() =
        inherit RegularNode("canvas")
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore

    type object'() =
        inherit RegularNode("object")
        member this.data
            with set (value: string | null) = this.attr("data", value) |> ignore
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore

    type param() =
        inherit VoidNode("param")
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore

    type data() =
        inherit RegularNode("data")
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore

    type time() =
        inherit RegularNode("time")
        member this.datetime
            with set (value: string | null) = this.attr("datetime", value) |> ignore

    type progress() =
        inherit RegularNode("progress")
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore
        member this.max
            with set (value: string | null) = this.attr("max", value) |> ignore

    type meter() =
        inherit RegularNode("meter")
        member this.form
            with set (value: string | null) = this.attr("form", value) |> ignore
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore
        member this.min
            with set (value: string | null) = this.attr("min", value) |> ignore
        member this.max
            with set (value: string | null) = this.attr("max", value) |> ignore
        member this.low
            with set (value: string | null) = this.attr("low", value) |> ignore
        member this.high
            with set (value: string | null) = this.attr("high", value) |> ignore
        member this.optimum
            with set (value: string | null) = this.attr("optimum", value) |> ignore

    type details() =
        inherit RegularNode("details")
        member this.open'
            with set (value: bool) =
                if value then
                    this.attr("open", "") |> ignore

    type summary() =
        inherit RegularNode("summary")

    type dialog() =
        inherit RegularNode("dialog")
        member this.open'
            with set (value: bool) =
                if value then
                    this.attr("open", "") |> ignore

    type menu() =
        inherit RegularNode("menu")

    type datalist() =
        inherit RegularNode("datalist")

    type fieldset() =
        inherit RegularNode("fieldset")
        member this.disabled
            with set (value: bool) =
                if value then
                    this.attr("disabled", "") |> ignore
        member this.form
            with set (value: string | null) = this.attr("form", value) |> ignore
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore

    type legend() =
        inherit RegularNode("legend")


    type table() =
        inherit RegularNode("table")
    type tbody() =
        inherit RegularNode("tbody")
    type thead() =
        inherit RegularNode("thead")
    type tfoot() =
        inherit RegularNode("tfoot")
    type tr() =
        inherit RegularNode("tr")
    type th() =
        inherit RegularNode("th")
        member this.abbr
            with set (value: string | null) = this.attr("abbr", value) |> ignore
        member this.colspan
            with set (value: int) = this.attr("colspan", string value) |> ignore
        member this.rowspan
            with set (value: int) = this.attr("rowspan", string value) |> ignore
        member this.headers
            with set (value: string | null) = this.attr("headers", value) |> ignore
        member this.scope
            with set (value: string | null) = this.attr("scope", value) |> ignore
    type td() =
        inherit RegularNode("td")
        member this.colspan
            with set (value: int) = this.attr("colspan", string value) |> ignore
        member this.rowspan
            with set (value: int) = this.attr("rowspan", string value) |> ignore
        member this.headers
            with set (value: string | null) = this.attr("headers", value) |> ignore

    type map() =
        inherit RegularNode("map")
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
    type area() =
        inherit VoidNode("area")
        member this.shape
            with set (value: string | null) = this.attr("shape", value) |> ignore
        member this.coords
            with set (value: string | null) = this.attr("coords", value) |> ignore
        member this.href
            with set (value: string | null) = this.attr("href", value) |> ignore
        member this.alt
            with set (value: string | null) = this.attr("alt", value) |> ignore
        member this.download
            with set (value: string | null) = this.attr("download", value) |> ignore
        member this.target
            with set (value: string | null) = this.attr("target", value) |> ignore
        member this.rel
            with set (value: string | null) = this.attr("rel", value) |> ignore
        member this.referrerpolicy
            with set (value: string | null) = this.attr("referrerpolicy", value) |> ignore
