namespace Oxpecker.Solid

open System.Runtime.CompilerServices
open Browser.Types
open JetBrains.Annotations

[<AutoOpen>]
module Tags =

    /// Fragment (or template) node, only renders children, not itself
    type __() =
        inherit FragmentNode()

    /// Set of html extensions that keep original type
    [<Extension>]
    type HtmlElementExtensions =

        /// Add an attribute to the element
        [<Extension>]
        static member attr(this: #HtmlTag, name: string, value: string) = this

        /// Add event handler to the element through the corresponding attribute
        [<Extension>]
        static member on(this: #HtmlTag, eventName: string, eventHandler: Event -> unit) = this

        /// Add data attribute to the element
        [<Extension>]
        static member data(this: #HtmlTag, name: string, value: string) = this

    // global attributes
    type HtmlTag with
        member this.id with set (value: string) = ()
        member this.class' with set (value: string) = ()
        [<LanguageInjection(InjectedLanguage.CSS, Prefix = ".x{", Suffix = ";}")>]
        member this.style with set (value: string) = ()
        member this.lang with set (value: string) = ()
        member this.dir with set (value: string) = ()
        member this.tabindex with set (value: int) = ()
        member this.title with set (value: string) = ()
        member this.accesskey with set (value: char) = ()
        member this.contenteditable with set (value: bool) = () //TODO
        member this.draggable with set (value: string) = ()
        member this.enterkeyhint set (value: string) = ()
        member this.hidden with set (value: bool) = ()
        member this.inert with set (value: bool) = ()
        member this.inputmode with set (value: string) = ()
        member this.popover with set (value: bool) = ()
        member this.spellcheck with set (value: bool) = () //TODO
        member this.translate with set (value: bool) = () //TODO

    type head() = inherit RegularNode()
    type body() = inherit RegularNode()
    type title() = inherit RegularNode()
    type div() = inherit RegularNode()
    type article() = inherit RegularNode()
    type section() = inherit RegularNode()
    type header() = inherit RegularNode()
    type footer() = inherit RegularNode()
    type main() = inherit RegularNode()
    type h1() = inherit RegularNode()
    type h2() = inherit RegularNode()
    type h3() = inherit RegularNode()
    type h4() = inherit RegularNode()
    type h5() = inherit RegularNode()
    type h6() = inherit RegularNode()
    type ul() = inherit RegularNode()
    type ol() = inherit RegularNode()
    type li() = inherit RegularNode()
    type p() = inherit RegularNode()
    type span() = inherit RegularNode()
    type strong() = inherit RegularNode()
    type em() = inherit RegularNode()
    type caption() = inherit RegularNode()
    type nav() = inherit RegularNode()
    type i() = inherit RegularNode()
    type b() = inherit RegularNode()
    type u() = inherit RegularNode()
    type s() = inherit RegularNode()
    type noscript() = inherit RegularNode()
    type code() = inherit RegularNode()
    type pre() = inherit RegularNode()
    type blockquote() = inherit RegularNode()
    type cite() = inherit RegularNode()
    type q() = inherit RegularNode()
    type address() = inherit RegularNode()
    type del() = inherit RegularNode()
    type ins() = inherit RegularNode()
    type abbr() = inherit RegularNode()
    type dfn() = inherit RegularNode()
    type sub() = inherit RegularNode()
    type sup() = inherit RegularNode()
    type template() = inherit RegularNode()

    type br() = inherit VoidNode()
    type hr() = inherit VoidNode()

    type a() =
        inherit RegularNode()
        member this.href with set (value: string) = ()
        member this.rel with set (value: string) = ()
        member this.target with set (value: string) = ()
        member this.download with set (value: string) = ()

    type base'() =
        inherit VoidNode()
        member this.href with set (value: string) = ()
        member this.target with set (value: string) = ()

    type img() =
        inherit VoidNode()
        member this.src with set (value: string) = ()
        member this.alt with set (value: string) = ()
        member this.width with set (value: int) = ()
        member this.height with set (value: int) = ()
        member this.srcset with set (value: string) = ()
        member this.referrerpolicy with set (value: string) = ()
        member this.crossorigin with set (value: string) = ()
        member this.sizes with set (value: string) = ()

    type form() =
        inherit RegularNode()
        member this.action with set (value: string) = ()
        member this.method with set (value: string) = ()
        member this.enctype with set (value: string) = ()
        member this.target with set (value: string) = ()

    type script() =
        inherit RegularNode()
        member this.src with set (value: string) = ()
        member this.type' with set (value: string) = ()
        member this.async with set (value: bool) = ()
        member this.defer with set (value: bool) = ()
        member this.integrity with set (value: string) = ()
        member this.crossorigin with set (value: string) = ()

    type link() =
        inherit VoidNode()
        member this.rel with set (value: string) = ()
        member this.href with set (value: string) = ()
        member this.type' with set (value: string) = ()
        member this.media with set (value: string) = ()
        member this.as' with set (value: string) = ()
        member this.sizes with set (value: string) = ()

    type html() =
        inherit RegularNode()
        member this.xmlns with set (value: string) = ()

    type meta() =
        inherit VoidNode()
        member this.name with set (value: string) = ()
        member this.content with set (value: string) = ()
        member this.charset with set (value: string) = ()
        member this.httpEquiv with set (value: string) = () //TODO

    type input() =
        inherit VoidNode()
        member this.type' with set (value: string) = ()
        member this.name with set (value: string) = ()
        member this.value with set (value: string) = ()
        member this.placeholder with set (value: string) = ()
        member this.required with set (value: bool) = ()
        member this.autofocus with set (value: bool) = ()
        member this.autocomplete with set (value: string) = ()
        member this.min with set (value: string) = ()
        member this.max with set (value: string) = ()
        member this.step with set (value: string) = ()
        member this.pattern with set (value: string) = ()
        member this.readonly with set (value: bool) = ()
        member this.disabled with set (value: bool) = ()
        member this.multiple with set (value: bool) = ()
        member this.accept with set (value: string) = ()
        member this.list with set (value: string) = ()
        member this.maxlength with set (value: int) = ()
        member this.minlength with set (value: int) = ()
        member this.size with set (value: int) = ()
        member this.src with set (value: string) = ()
        member this.width with set (value: int) = ()
        member this.height with set (value: int) = ()
        member this.alt with set (value: string) = ()

    type output() =
        inherit RegularNode()
        member this.for' with set (value: string) = ()
        member this.form with set (value: string) = ()
        member this.name with set (value: string) = ()

    type textarea() =
        inherit RegularNode()
        member this.name with set (value: string) = ()
        member this.placeholder with set (value: string) = ()
        member this.required with set (value: bool) = ()
        member this.autofocus with set (value: bool) = ()
        member this.readonly with set (value: bool) = ()
        member this.disabled with set (value: bool) = ()
        member this.rows with set (value: int) = ()
        member this.cols with set (value: int) = ()
        member this.wrap with set (value: string) = ()
        member this.maxlength with set (value: int) = ()

    type button() =
        inherit RegularNode()
        member this.type' with set (value: string) = ()
        member this.name with set (value: string) = ()
        member this.value with set (value: string) = ()
        member this.disabled with set (value: bool) = ()
        member this.autofocus with set (value: bool) = ()

    type select() =
        inherit RegularNode()
        member this.name with set (value: string) = ()
        member this.required with set (value: bool) = ()
        member this.autofocus with set (value: bool) = ()
        member this.disabled with set (value: bool) = ()
        member this.multiple with set (value: bool) = ()
        member this.size with set (value: int) = ()

    type option() =
        inherit RegularNode()
        member this.value with set (value: string) = ()
        member this.selected with set (value: bool) = ()
        member this.disabled with set (value: bool) = ()
        member this.label with set (value: string) = ()

    type optgroup() =
        inherit RegularNode()
        member this.label with set (value: string) = ()
        member this.disabled with set (value: bool) = ()

    type label() =
        inherit RegularNode()
        member this.for' with set (value: string) = ()

    type style() =
        inherit RegularNode()
        member this.type' with set (value: string) = ()
        member this.media with set (value: string) = ()

    type iframe() =
        inherit RegularNode()
        member this.src with set (value: string) = ()
        member this.name with set (value: string) = ()
        member this.sandbox with set (value: string) = ()
        member this.width with set (value: int) = ()
        member this.height with set (value: int) = ()
        member this.allowfullscreen with set (value: bool) = () //TODO
        member this.allowpaymentrequest with set (value: bool) = () //TODO
        member this.loading with set (value: string) = ()
        member this.referrerpolicy with set (value: string) = ()
        member this.srcdoc with set (value: string) = ()

    type video() =
        inherit RegularNode()
        member this.src with set (value: string) = ()
        member this.poster with set (value: string) = ()
        member this.autoplay with set (value: bool) = ()
        member this.controls with set (value: bool) = ()
        member this.loop with set (value: bool) = ()
        member this.muted with set (value: bool) = ()
        member this.width with set (value: int) = ()
        member this.height with set (value: int) = ()
        member this.preload with set (value: string) = ()

    type audio() =
        inherit RegularNode()
        member this.src with set (value: string) = ()
        member this.autoplay with set (value: bool) = ()
        member this.controls with set (value: bool) = ()
        member this.loop with set (value: bool) = ()
        member this.muted with set (value: bool) = ()

    type source() =
        inherit VoidNode()
        member this.src with set (value: string) = ()
        member this.type' with set (value: string) = ()
        member this.media with set (value: string) = ()
        member this.sizes with set (value: string) = ()
        member this.srcset with set (value: string) = ()

    type canvas() =
        inherit RegularNode()
        member this.width with set (value: int) = ()
        member this.height with set (value: int) = ()

    type object'() =
        inherit RegularNode()
        member this.data with set (value: string) = ()
        member this.type' with set (value: string) = ()
        member this.width with set (value: int) = ()
        member this.height with set (value: int) = ()

    type param() =
        inherit VoidNode()
        member this.name with set (value: string) = ()
        member this.value with set (value: string) = ()

    type data() =
        inherit RegularNode()
        member this.value with set (value: string) = ()

    type time() =
        inherit RegularNode()
        member this.datetime with set (value: string) = ()

    type progress() =
        inherit RegularNode()
        member this.value with set (value: string) = ()
        member this.max with set (value: string) = ()

    type meter() =
        inherit RegularNode()
        member this.form with set (value: string) = ()
        member this.value with set (value: string) = ()
        member this.min with set (value: string) = ()
        member this.max with set (value: string) = ()
        member this.low with set (value: string) = ()
        member this.high with set (value: string) = ()
        member this.optimum with set (value: string) = ()

    type details() =
        inherit RegularNode()
        member this.open' with set (value: bool) = ()

    type summary() = inherit RegularNode()

    type dialog() =
        inherit RegularNode()
        member this.open' with set (value: bool) = ()

    type menu() = inherit RegularNode()

    type datalist() = inherit RegularNode()

    type fieldset() =
        inherit RegularNode()
        member this.disabled with set (value: bool) = ()

        member this.form with set (value: string) = ()
        member this.name with set (value: string) = ()

    type legend() = inherit RegularNode()
    type table() = inherit RegularNode()
    type tbody() = inherit RegularNode()
    type thead() = inherit RegularNode()
    type tfoot() = inherit RegularNode()
    type tr() = inherit RegularNode()
    type th() =
        inherit RegularNode()
        member this.abbr with set (value: string) = ()
        member this.colspan with set (value: int) = ()
        member this.rowspan with set (value: int) = ()
        member this.headers with set (value: string) = ()
        member this.scope with set (value: string) = ()
    type td() =
        inherit RegularNode()
        member this.colspan with set (value: int) = ()
        member this.rowspan with set (value: int) = ()
        member this.headers with set (value: string) = ()

    type map() =
        inherit RegularNode()
        member this.name with set (value: string) = ()
    type area() =
        inherit VoidNode()
        member this.shape with set (value: string) = ()
        member this.coords with set (value: string) = ()
        member this.href with set (value: string) = ()
        member this.alt with set (value: string) = ()
        member this.download with set (value: string) = ()
        member this.target with set (value: string) = ()
        member this.rel with set (value: string) = ()
        member this.referrerpolicy with set (value: string) = ()
