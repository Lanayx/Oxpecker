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
        member this.href with set value = this.AddAttribute("href", value) |> ignore
        member this.rel with set value = this.AddAttribute("rel", value) |> ignore
        member this.target with set value = this.AddAttribute("target", value) |> ignore

    type img() =
        inherit HtmlElement("img", true)
        member this.src with set value = this.AddAttribute("src", value) |> ignore
        member this.alt with set value = this.AddAttribute("alt", value) |> ignore
        member this.width with set value = this.AddAttribute("width", value) |> ignore
        member this.height with set value = this.AddAttribute("height", value) |> ignore

    type form() =
        inherit HtmlElement("form")
        member this.action with set value = this.AddAttribute("action", value) |> ignore
        member this.method with set value = this.AddAttribute("method", value) |> ignore
        member this.enctype with set value = this.AddAttribute("enctype", value) |> ignore
        member this.target with set value = this.AddAttribute("target", value) |> ignore

    type script() =
        inherit HtmlElement("script")
        member this.src with set value = this.AddAttribute("src", value) |> ignore
        member this.type' with set value = this.AddAttribute("type", value) |> ignore
        member this.async with set value = this.AddAttribute("async", value) |> ignore
        member this.defer with set value = this.AddAttribute("defer", value) |> ignore

    type link() =
        inherit HtmlElement("link", true)
        member this.rel with set value = this.AddAttribute("rel", value) |> ignore
        member this.href with set value = this.AddAttribute("href", value) |> ignore
        member this.type' with set value = this.AddAttribute("type", value) |> ignore
        member this.media with set value = this.AddAttribute("media", value) |> ignore

    type html() =
        inherit HtmlElement("html")
        member this.xmlns with set value = this.AddAttribute("xmlns", value) |> ignore

    type meta() =
        inherit HtmlElement("meta", true)
        member this.name with set value = this.AddAttribute("name", value) |> ignore
        member this.content with set value = this.AddAttribute("content", value) |> ignore
        member this.charset with set value = this.AddAttribute("charset", value) |> ignore
        member this.httpEquiv with set value = this.AddAttribute("http-equiv", value) |> ignore

    type input() =
        inherit HtmlElement("input", true)
        member this.type' with set value = this.AddAttribute("type", value) |> ignore
        member this.name with set value = this.AddAttribute("name", value) |> ignore
        member this.value with set value = this.AddAttribute("value", value) |> ignore
        member this.placeholder with set value = this.AddAttribute("placeholder", value) |> ignore
        member this.required with set value = this.AddAttribute("required", value) |> ignore
        member this.autofocus with set value = this.AddAttribute("autofocus", value) |> ignore
        member this.autocomplete with set value = this.AddAttribute("autocomplete", value) |> ignore
        member this.min with set value = this.AddAttribute("min", value) |> ignore
        member this.max with set value = this.AddAttribute("max", value) |> ignore
        member this.step with set value = this.AddAttribute("step", value) |> ignore
        member this.pattern with set value = this.AddAttribute("pattern", value) |> ignore
        member this.readonly with set value = this.AddAttribute("readonly", value) |> ignore
        member this.disabled with set value = this.AddAttribute("disabled", value) |> ignore
        member this.multiple with set value = this.AddAttribute("multiple", value) |> ignore
        member this.accept with set value = this.AddAttribute("accept", value) |> ignore
        member this.list with set value = this.AddAttribute("list", value) |> ignore
        member this.maxlength with set value = this.AddAttribute("maxlength", value) |> ignore
        member this.minlength with set value = this.AddAttribute("minlength", value) |> ignore
        member this.size with set value = this.AddAttribute("size", value) |> ignore
        member this.src with set value = this.AddAttribute("src", value) |> ignore
        member this.height with set value = this.AddAttribute("height", value) |> ignore
        member this.width with set value = this.AddAttribute("width", value) |> ignore
        member this.alt with set value = this.AddAttribute("alt", value) |> ignore

    type textarea() =
        inherit HtmlElement("textarea")
        member this.name with set value = this.AddAttribute("name", value) |> ignore
        member this.placeholder with set value = this.AddAttribute("placeholder", value) |> ignore
        member this.required with set value = this.AddAttribute("required", value) |> ignore
        member this.autofocus with set value = this.AddAttribute("autofocus", value) |> ignore
        member this.readonly with set value = this.AddAttribute("readonly", value) |> ignore
        member this.disabled with set value = this.AddAttribute("disabled", value) |> ignore
        member this.rows with set value = this.AddAttribute("rows", value) |> ignore
        member this.cols with set value = this.AddAttribute("cols", value) |> ignore
        member this.wrap with set value = this.AddAttribute("wrap", value) |> ignore
        member this.maxlength with set value = this.AddAttribute("maxlength", value) |> ignore

    type button() =
        inherit HtmlElement("button")
        member this.type' with set value = this.AddAttribute("type", value) |> ignore
        member this.name with set value = this.AddAttribute("name", value) |> ignore
        member this.value with set value = this.AddAttribute("value", value) |> ignore
        member this.disabled with set value = this.AddAttribute("disabled", value) |> ignore
        member this.autofocus with set value = this.AddAttribute("autofocus", value) |> ignore

    type select() =
        inherit HtmlElement("select")
        member this.name with set value = this.AddAttribute("name", value) |> ignore
        member this.required with set value = this.AddAttribute("required", value) |> ignore
        member this.autofocus with set value = this.AddAttribute("autofocus", value) |> ignore
        member this.disabled with set value = this.AddAttribute("disabled", value) |> ignore
        member this.multiple with set value = this.AddAttribute("multiple", value) |> ignore
        member this.size with set value = this.AddAttribute("size", value) |> ignore

    type option() =
        inherit HtmlElement("option")
        member this.value with set value = this.AddAttribute("value", value) |> ignore
        member this.selected with set value = this.AddAttribute("selected", value) |> ignore
        member this.disabled with set value = this.AddAttribute("disabled", value) |> ignore

    type label() =
        inherit HtmlElement("label")
        member this.for' with set value = this.AddAttribute("for", value) |> ignore

    type style() =
        inherit HtmlElement("style")
        member this.type' with set value = this.AddAttribute("type", value) |> ignore
        member this.media with set value = this.AddAttribute("media", value) |> ignore

    type iframe() =
        inherit HtmlElement("iframe")
        member this.src with set value = this.AddAttribute("src", value) |> ignore
        member this.name with set value = this.AddAttribute("name", value) |> ignore
        member this.sandbox with set value = this.AddAttribute("sandbox", value) |> ignore
        member this.width with set value = this.AddAttribute("width", value) |> ignore
        member this.height with set value = this.AddAttribute("height", value) |> ignore
        member this.allowfullscreen with set value = this.AddAttribute("allowfullscreen", value) |> ignore
        member this.allowpaymentrequest with set value = this.AddAttribute("allowpaymentrequest", value) |> ignore
        member this.loading with set value = this.AddAttribute("loading", value) |> ignore
        member this.referrerpolicy with set value = this.AddAttribute("referrerpolicy", value) |> ignore
        member this.srcdoc with set value = this.AddAttribute("srcdoc", value) |> ignore

    type video() =
        inherit HtmlElement("video")
        member this.src with set value = this.AddAttribute("src", value) |> ignore
        member this.poster with set value = this.AddAttribute("poster", value) |> ignore
        member this.autoplay with set value = this.AddAttribute("autoplay", value) |> ignore
        member this.controls with set value = this.AddAttribute("controls", value) |> ignore
        member this.loop with set value = this.AddAttribute("loop", value) |> ignore
        member this.muted with set value = this.AddAttribute("muted", value) |> ignore
        member this.width with set value = this.AddAttribute("width", value) |> ignore
        member this.height with set value = this.AddAttribute("height", value) |> ignore

    type audio() =
        inherit HtmlElement("audio")
        member this.src with set value = this.AddAttribute("src", value) |> ignore
        member this.autoplay with set value = this.AddAttribute("autoplay", value) |> ignore
        member this.controls with set value = this.AddAttribute("controls", value) |> ignore
        member this.loop with set value = this.AddAttribute("loop", value) |> ignore
        member this.muted with set value = this.AddAttribute("muted", value) |> ignore

    type source() =
        inherit HtmlElement("source", true)
        member this.src with set value = this.AddAttribute("src", value) |> ignore
        member this.type' with set value = this.AddAttribute("type", value) |> ignore
        member this.media with set value = this.AddAttribute("media", value) |> ignore
        member this.sizes with set value = this.AddAttribute("sizes", value) |> ignore
        member this.srcset with set value = this.AddAttribute("srcset", value) |> ignore

    type canvas() =
        inherit HtmlElement("canvas")
        member this.width with set value = this.AddAttribute("width", value) |> ignore
        member this.height with set value = this.AddAttribute("height", value) |> ignore

    type object'() =
        inherit HtmlElement("object")
        member this.data with set value = this.AddAttribute("data", value) |> ignore
        member this.type' with set value = this.AddAttribute("type", value) |> ignore
        member this.width with set value = this.AddAttribute("width", value) |> ignore
        member this.height with set value = this.AddAttribute("height", value) |> ignore

    type param() =
        inherit HtmlElement("param", true)
        member this.name with set value = this.AddAttribute("name", value) |> ignore
        member this.value with set value = this.AddAttribute("value", value) |> ignore


