namespace Oxpecker.ViewEngine

[<AutoOpen>]
module Tags =

    open Oxpecker.ViewEngine.Builder

    type head() = inherit HtmlElement("head")
    type body() = inherit HtmlElement("body")
    type div() = inherit HtmlElement("div")
    type article() = inherit HtmlElement("article")
    type section() = inherit HtmlElement("section")
    type header() = inherit HtmlElement("header")
    type footer() = inherit HtmlElement("footer")
    type h1() = inherit HtmlElement("h1")
    type h2() = inherit HtmlElement("h2")
    type h3() = inherit HtmlElement("h3")
    type h4() = inherit HtmlElement("h4")
    type h5() = inherit HtmlElement("h5")
    type h6() = inherit HtmlElement("h6")
    type ul() = inherit HtmlElement("ul")
    type ol() = inherit HtmlElement("ol")
    type li() = inherit HtmlElement("li")
    type p() = inherit HtmlElement("p")
    type span() = inherit HtmlElement("span")
    type strong() = inherit HtmlElement("strong")
    type em() = inherit HtmlElement("em")
    type table() = inherit HtmlElement("table")
    type tr() = inherit HtmlElement("tr")
    type td() = inherit HtmlElement("td")
    type th() = inherit HtmlElement("th")
    type tbody() = inherit HtmlElement("tbody")
    type thead() = inherit HtmlElement("thead")
    type tfoot() = inherit HtmlElement("tfoot")
    type caption() = inherit HtmlElement("caption")
    type nav() = inherit HtmlElement("nav")
    type i() = inherit HtmlElement("i")
    type b() = inherit HtmlElement("b")
    type u() = inherit HtmlElement("u")
    type s() = inherit HtmlElement("s")

    type br() = inherit HtmlElement("br", true)
    type hr() = inherit HtmlElement("hr", true)

    type a() =
        inherit HtmlElement("a")
        member this.href with set value = this.AddProperty "href" value
        member this.rel with set value = this.AddProperty "rel" value
        member this.target with set value = this.AddProperty "target" value

    type img() =
        inherit HtmlElement("img", true)
        member this.src with set value = this.AddProperty "src" value
        member this.alt with set value = this.AddProperty "alt" value
        member this.width with set value = this.AddProperty "width" value
        member this.height with set value = this.AddProperty "height" value

    type form() =
        inherit HtmlElement("form")
        member this.action with set value = this.AddProperty "action" value
        member this.method with set value = this.AddProperty "method" value
        member this.enctype with set value = this.AddProperty "enctype" value
        member this.target with set value = this.AddProperty "target" value

    type script() =
        inherit HtmlElement("script")
        member this.src with set value = this.AddProperty "src" value
        member this.type' with set value = this.AddProperty "type" value
        member this.async with set value = this.AddProperty "async" value
        member this.defer with set value = this.AddProperty "defer" value

    type link() =
        inherit HtmlElement("link", true)
        member this.rel with set value = this.AddProperty "rel" value
        member this.href with set value = this.AddProperty "href" value
        member this.type' with set value = this.AddProperty "type" value
        member this.media with set value = this.AddProperty "media" value

    type html() =
        inherit HtmlElement("html")
        member this.xmlns with set value = this.AddProperty "xmlns" value

    type meta() =
        inherit HtmlElement("meta", true)
        member this.name with set value = this.AddProperty "name" value
        member this.content with set value = this.AddProperty "content" value
        member this.charset with set value = this.AddProperty "charset" value
        member this.httpEquiv with set value = this.AddProperty "http-equiv" value

    type input() =
        inherit HtmlElement("input", true)
        member this.type' with set value = this.AddProperty "type" value
        member this.name with set value = this.AddProperty "name" value
        member this.value with set value = this.AddProperty "value" value
        member this.placeholder with set value = this.AddProperty "placeholder" value
        member this.required with set value = this.AddProperty "required" value
        member this.autofocus with set value = this.AddProperty "autofocus" value
        member this.autocomplete with set value = this.AddProperty "autocomplete" value
        member this.min with set value = this.AddProperty "min" value
        member this.max with set value = this.AddProperty "max" value
        member this.step with set value = this.AddProperty "step" value
        member this.pattern with set value = this.AddProperty "pattern" value
        member this.readonly with set value = this.AddProperty "readonly" value
        member this.disabled with set value = this.AddProperty "disabled" value
        member this.multiple with set value = this.AddProperty "multiple" value
        member this.accept with set value = this.AddProperty "accept" value
        member this.list with set value = this.AddProperty "list" value
        member this.maxlength with set value = this.AddProperty "maxlength" value
        member this.minlength with set value = this.AddProperty "minlength" value
        member this.size with set value = this.AddProperty "size" value
        member this.src with set value = this.AddProperty "src" value
        member this.height with set value = this.AddProperty "height" value
        member this.width with set value = this.AddProperty "width" value
        member this.alt with set value = this.AddProperty "alt" value

    type textarea() =
        inherit HtmlElement("textarea")
        member this.name with set value = this.AddProperty "name" value
        member this.placeholder with set value = this.AddProperty "placeholder" value
        member this.required with set value = this.AddProperty "required" value
        member this.autofocus with set value = this.AddProperty "autofocus" value
        member this.readonly with set value = this.AddProperty "readonly" value
        member this.disabled with set value = this.AddProperty "disabled" value
        member this.rows with set value = this.AddProperty "rows" value
        member this.cols with set value = this.AddProperty "cols" value
        member this.wrap with set value = this.AddProperty "wrap" value
        member this.maxlength with set value = this.AddProperty "maxlength" value

    type button() =
        inherit HtmlElement("button")
        member this.type' with set value = this.AddProperty "type" value
        member this.name with set value = this.AddProperty "name" value
        member this.value with set value = this.AddProperty "value" value
        member this.disabled with set value = this.AddProperty "disabled" value
        member this.autofocus with set value = this.AddProperty "autofocus" value

    type select() =
        inherit HtmlElement("select")
        member this.name with set value = this.AddProperty "name" value
        member this.required with set value = this.AddProperty "required" value
        member this.autofocus with set value = this.AddProperty "autofocus" value
        member this.disabled with set value = this.AddProperty "disabled" value
        member this.multiple with set value = this.AddProperty "multiple" value
        member this.size with set value = this.AddProperty "size" value

    type option() =
        inherit HtmlElement("option")
        member this.value with set value = this.AddProperty "value" value
        member this.selected with set value = this.AddProperty "selected" value
        member this.disabled with set value = this.AddProperty "disabled" value

    type label() =
        inherit HtmlElement("label")
        member this.for' with set value = this.AddProperty "for" value

    type style() =
        inherit HtmlElement("style")
        member this.type' with set value = this.AddProperty "type" value
        member this.media with set value = this.AddProperty "media" value

    type iframe() =
        inherit HtmlElement("iframe")
        member this.src with set value = this.AddProperty "src" value
        member this.name with set value = this.AddProperty "name" value
        member this.sandbox with set value = this.AddProperty "sandbox" value
        member this.width with set value = this.AddProperty "width" value
        member this.height with set value = this.AddProperty "height" value
        member this.allowfullscreen with set value = this.AddProperty "allowfullscreen" value
        member this.allowpaymentrequest with set value = this.AddProperty "allowpaymentrequest" value
        member this.loading with set value = this.AddProperty "loading" value
        member this.referrerpolicy with set value = this.AddProperty "referrerpolicy" value
        member this.srcdoc with set value = this.AddProperty "srcdoc" value

    type video() =
        inherit HtmlElement("video")
        member this.src with set value = this.AddProperty "src" value
        member this.poster with set value = this.AddProperty "poster" value
        member this.autoplay with set value = this.AddProperty "autoplay" value
        member this.controls with set value = this.AddProperty "controls" value
        member this.loop with set value = this.AddProperty "loop" value
        member this.muted with set value = this.AddProperty "muted" value
        member this.width with set value = this.AddProperty "width" value
        member this.height with set value = this.AddProperty "height" value

    type audio() =
        inherit HtmlElement("audio")
        member this.src with set value = this.AddProperty "src" value
        member this.autoplay with set value = this.AddProperty "autoplay" value
        member this.controls with set value = this.AddProperty "controls" value
        member this.loop with set value = this.AddProperty "loop" value
        member this.muted with set value = this.AddProperty "muted" value

    type source() =
        inherit HtmlElement("source", true)
        member this.src with set value = this.AddProperty "src" value
        member this.type' with set value = this.AddProperty "type" value
        member this.media with set value = this.AddProperty "media" value
        member this.sizes with set value = this.AddProperty "sizes" value
        member this.srcset with set value = this.AddProperty "srcset" value

    type canvas() =
        inherit HtmlElement("canvas")
        member this.width with set value = this.AddProperty "width" value
        member this.height with set value = this.AddProperty "height" value

    type object'() =
        inherit HtmlElement("object")
        member this.data with set value = this.AddProperty "data" value
        member this.type' with set value = this.AddProperty "type" value
        member this.width with set value = this.AddProperty "width" value
        member this.height with set value = this.AddProperty "height" value

    type param() =
        inherit HtmlElement("param", true)
        member this.name with set value = this.AddProperty "name" value
        member this.value with set value = this.AddProperty "value" value


