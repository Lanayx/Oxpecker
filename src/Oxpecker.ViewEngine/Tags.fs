namespace Oxpecker.ViewEngine

[<AutoOpen>]
module Tags =

    open Oxpecker.ViewEngine.Builder

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
    type table() =
        inherit HtmlElement("table")
    type tr() =
        inherit HtmlElement("tr")
    type td() =
        inherit HtmlElement("td")
    type th() =
        inherit HtmlElement("th")
    type tbody() =
        inherit HtmlElement("tbody")
    type thead() =
        inherit HtmlElement("thead")
    type tfoot() =
        inherit HtmlElement("tfoot")
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

    type br() =
        inherit HtmlElement(HtmlElementType.VoidNode "br")
    type hr() =
        inherit HtmlElement(HtmlElementType.VoidNode "hr")

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
        inherit HtmlElement(HtmlElementType.VoidNode "base")
        member this.href
            with set value = this.attr("href", value) |> ignore
        member this.target
            with set value = this.attr("target", value) |> ignore

    type img() =
        inherit HtmlElement(HtmlElementType.VoidNode "img")
        member this.src
            with set value = this.attr("src", value) |> ignore
        member this.alt
            with set value = this.attr("alt", value) |> ignore
        member this.width
            with set value = this.attr("width", value) |> ignore
        member this.height
            with set value = this.attr("height", value) |> ignore

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
            with set value = this.attr("async", value) |> ignore
        member this.defer
            with set value = this.attr("defer", value) |> ignore

    type link() =
        inherit HtmlElement(HtmlElementType.VoidNode "link")
        member this.rel
            with set value = this.attr("rel", value) |> ignore
        member this.href
            with set value = this.attr("href", value) |> ignore
        member this.type'
            with set value = this.attr("type", value) |> ignore
        member this.media
            with set value = this.attr("media", value) |> ignore

    type html() =
        inherit HtmlElement("html")
        member this.xmlns
            with set value = this.attr("xmlns", value) |> ignore

    type meta() =
        inherit HtmlElement(HtmlElementType.VoidNode "meta")
        member this.name
            with set value = this.attr("name", value) |> ignore
        member this.content
            with set value = this.attr("content", value) |> ignore
        member this.charset
            with set value = this.attr("charset", value) |> ignore
        member this.httpEquiv
            with set value = this.attr("http-equiv", value) |> ignore

    type input() =
        inherit HtmlElement(HtmlElementType.VoidNode "input")
        member this.type'
            with set value = this.attr("type", value) |> ignore
        member this.name
            with set value = this.attr("name", value) |> ignore
        member this.value
            with set value = this.attr("value", value) |> ignore
        member this.placeholder
            with set value = this.attr("placeholder", value) |> ignore
        member this.required
            with set value = this.attr("required", value) |> ignore
        member this.autofocus
            with set value = this.attr("autofocus", value) |> ignore
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
            with set value = this.attr("readonly", value) |> ignore
        member this.disabled
            with set value = this.attr("disabled", value) |> ignore
        member this.multiple
            with set value = this.attr("multiple", value) |> ignore
        member this.accept
            with set value = this.attr("accept", value) |> ignore
        member this.list
            with set value = this.attr("list", value) |> ignore
        member this.maxlength
            with set value = this.attr("maxlength", value) |> ignore
        member this.minlength
            with set value = this.attr("minlength", value) |> ignore
        member this.size
            with set value = this.attr("size", value) |> ignore
        member this.src
            with set value = this.attr("src", value) |> ignore
        member this.height
            with set value = this.attr("height", value) |> ignore
        member this.width
            with set value = this.attr("width", value) |> ignore
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
            with set value = this.attr("required", value) |> ignore
        member this.autofocus
            with set value = this.attr("autofocus", value) |> ignore
        member this.readonly
            with set value = this.attr("readonly", value) |> ignore
        member this.disabled
            with set value = this.attr("disabled", value) |> ignore
        member this.rows
            with set value = this.attr("rows", value) |> ignore
        member this.cols
            with set value = this.attr("cols", value) |> ignore
        member this.wrap
            with set value = this.attr("wrap", value) |> ignore
        member this.maxlength
            with set value = this.attr("maxlength", value) |> ignore

    type button() =
        inherit HtmlElement("button")
        member this.type'
            with set value = this.attr("type", value) |> ignore
        member this.name
            with set value = this.attr("name", value) |> ignore
        member this.value
            with set value = this.attr("value", value) |> ignore
        member this.disabled
            with set value = this.attr("disabled", value) |> ignore
        member this.autofocus
            with set value = this.attr("autofocus", value) |> ignore

    type select() =
        inherit HtmlElement("select")
        member this.name
            with set value = this.attr("name", value) |> ignore
        member this.required
            with set value = this.attr("required", value) |> ignore
        member this.autofocus
            with set value = this.attr("autofocus", value) |> ignore
        member this.disabled
            with set value = this.attr("disabled", value) |> ignore
        member this.multiple
            with set value = this.attr("multiple", value) |> ignore
        member this.size
            with set value = this.attr("size", value) |> ignore

    type option() =
        inherit HtmlElement("option")
        member this.value
            with set value = this.attr("value", value) |> ignore
        member this.selected
            with set value = this.attr("selected", value) |> ignore
        member this.disabled
            with set value = this.attr("disabled", value) |> ignore

    type optgroup() =
        inherit HtmlElement("optgroup")
        member this.label
            with set value = this.attr("label", value) |> ignore
        member this.disabled
            with set value = this.attr("disabled", value) |> ignore

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
            with set value = this.attr("width", value) |> ignore
        member this.height
            with set value = this.attr("height", value) |> ignore
        member this.allowfullscreen
            with set value = this.attr("allowfullscreen", value) |> ignore
        member this.allowpaymentrequest
            with set value = this.attr("allowpaymentrequest", value) |> ignore
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
            with set value = this.attr("autoplay", value) |> ignore
        member this.controls
            with set value = this.attr("controls", value) |> ignore
        member this.loop
            with set value = this.attr("loop", value) |> ignore
        member this.muted
            with set value = this.attr("muted", value) |> ignore
        member this.width
            with set value = this.attr("width", value) |> ignore
        member this.height
            with set value = this.attr("height", value) |> ignore

    type audio() =
        inherit HtmlElement("audio")
        member this.src
            with set value = this.attr("src", value) |> ignore
        member this.autoplay
            with set value = this.attr("autoplay", value) |> ignore
        member this.controls
            with set value = this.attr("controls", value) |> ignore
        member this.loop
            with set value = this.attr("loop", value) |> ignore
        member this.muted
            with set value = this.attr("muted", value) |> ignore

    type source() =
        inherit HtmlElement(HtmlElementType.VoidNode "source")
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
            with set value = this.attr("width", value) |> ignore
        member this.height
            with set value = this.attr("height", value) |> ignore

    type object'() =
        inherit HtmlElement("object")
        member this.data
            with set value = this.attr("data", value) |> ignore
        member this.type'
            with set value = this.attr("type", value) |> ignore
        member this.width
            with set value = this.attr("width", value) |> ignore
        member this.height
            with set value = this.attr("height", value) |> ignore

    type param() =
        inherit HtmlElement(HtmlElementType.VoidNode "param")
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
            with set value = this.attr("open", value) |> ignore

    type summary() =
        inherit HtmlElement("summary")

    type dialog() =
        inherit HtmlElement("dialog")
        member this.open'
            with set value = this.attr("open", value) |> ignore

    type menu() =
        inherit HtmlElement("menu")

    type datalist() =
        inherit HtmlElement("datalist")
