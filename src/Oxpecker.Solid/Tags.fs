namespace Oxpecker.Solid

open System.Runtime.CompilerServices
open Browser.Types
open JetBrains.Annotations
open Fable.Core

[<AutoOpen>]
module Tags =

    /// Fragment (or template) node, only renders children, not itself
    [<Erase>]
    type Fragment() =
        inherit FragmentNode()

    /// Set of html extensions that keep original type
    [<Extension>]
    [<Erase>]
    type HtmlElementExtensions =

        /// Add an attribute to the element
        [<Extension; Erase>]
        static member attr(this: #HtmlTag, name: string, value: string) = this

        /// Add event handler to the element through the corresponding attribute
        [<Extension; Erase>]
        static member on(this: #HtmlTag, eventName: string, eventHandler: Event -> unit) = this

        /// Add data attribute to the element
        [<Extension; Erase>]
        static member data(this: #HtmlTag, name: string, value: string) = this

        /// Referenced native HTML element
        [<Extension; Erase>]
        static member ref(this: #HtmlTag, el: #Element) = this

        /// Referenced native HTML element (before connecting to DOM)
        [<Extension; Erase>]
        static member ref(this: #HtmlTag, el: #Element -> unit) = this

        /// Usage `elem.style(createObj ["color", "green"; "background-color", state.myColor ])`
        [<Extension; Erase>]
        static member style'(this: #HtmlTag, styleObj: obj) = this

        /// Usage `elem.classList(createObj ["active", true; "disabled", state.disabled ])`
        [<Extension; Erase>]
        static member classList(this: #HtmlTag, classListObj: obj) = this

        /// Adds or removes attribute without value
        [<Extension; Erase>]
        static member bool(this: #HtmlTag, name: string, value: bool) = this

    // global attributes
    type HtmlTag with
        [<Erase>]
        member this.id
            with set (_: string) = ()
        [<Erase>]
        member this.class'
            with set (_: string) = ()
        [<LanguageInjection(InjectedLanguage.CSS, Prefix = ".x{", Suffix = ";}")>]
        [<Erase>]
        member this.style
            with set (_: string) = ()
        [<Erase>]
        member this.lang
            with set (_: string) = ()
        [<Erase>]
        member this.dir
            with set (_: string) = ()
        [<Erase>]
        member this.tabindex
            with set (_: int) = ()
        [<Erase>]
        member this.title
            with set (_: string) = ()
        [<Erase>]
        member this.accesskey
            with set (_: char) = ()
        [<Erase>]
        member this.contenteditable
            with set (_: string) = ()
        [<Erase>]
        member this.draggable
            with set (_: string) = ()
        [<Erase>]
        member this.enterkeyhint
            with set (_: string) = ()
        [<Erase>]
        member this.hidden
            with set (_: string) = ()
        [<Erase>]
        member this.inert
            with set (_: bool) = ()
        [<Erase>]
        member this.inputmode
            with set (_: string) = ()
        [<Erase>]
        member this.popover
            with set (_: string) = ()
        [<Erase>]
        member this.spellcheck
            with set (_: bool) = ()
        [<Erase>]
        member this.translate
            with set (_: string) = ()
        [<Erase>]
        member this.autocapitalize
            with set (_: string) = ()
        [<Erase>]
        member this.is
            with set (_: string) = ()
        [<Erase>]
        member this.part
            with set (_: string) = ()
        [<Erase>]
        member this.slot
            with set (_: string) = ()

    [<Erase>]
    type head() =
        inherit RegularNode()
    [<Erase>]
    type body() =
        inherit RegularNode()
    [<Erase>]
    type title() =
        inherit RegularNode()
    [<Erase>]
    type div() =
        inherit RegularNode()
    [<Erase>]
    type article() =
        inherit RegularNode()
    [<Erase>]
    type section() =
        inherit RegularNode()
    [<Erase>]
    type header() =
        inherit RegularNode()
    [<Erase>]
    type footer() =
        inherit RegularNode()
    [<Erase>]
    type main() =
        inherit RegularNode()
    [<Erase>]
    type h1() =
        inherit RegularNode()
    [<Erase>]
    type h2() =
        inherit RegularNode()
    [<Erase>]
    type h3() =
        inherit RegularNode()
    [<Erase>]
    type h4() =
        inherit RegularNode()
    [<Erase>]
    type h5() =
        inherit RegularNode()
    [<Erase>]
    type h6() =
        inherit RegularNode()
    [<Erase>]
    type ul() =
        inherit RegularNode()
    [<Erase>]
    type ol() =
        inherit RegularNode()
    [<Erase>]
    type li() =
        inherit RegularNode()
    [<Erase>]
    type p() =
        inherit RegularNode()
    [<Erase>]
    type span() =
        inherit RegularNode()
    [<Erase>]
    type small() =
        inherit RegularNode()
    [<Erase>]
    type strong() =
        inherit RegularNode()
    [<Erase>]
    type em() =
        inherit RegularNode()
    [<Erase>]
    type caption() =
        inherit RegularNode()
    [<Erase>]
    type nav() =
        inherit RegularNode()
    [<Erase>]
    type search() =
        inherit RegularNode()
    [<Erase>]
    type i() =
        inherit RegularNode()
    [<Erase>]
    type b() =
        inherit RegularNode()
    [<Erase>]
    type u() =
        inherit RegularNode()
    [<Erase>]
    type s() =
        inherit RegularNode()
    [<Erase>]
    type noscript() =
        inherit RegularNode()
    [<Erase>]
    type code() =
        inherit RegularNode()
    [<Erase>]
    type pre() =
        inherit RegularNode()
    [<Erase>]
    type blockquote() =
        inherit RegularNode()
    [<Erase>]
    type cite() =
        inherit RegularNode()
    [<Erase>]
    type q() =
        inherit RegularNode()
    [<Erase>]
    type address() =
        inherit RegularNode()
    [<Erase>]
    type del() =
        inherit RegularNode()
    [<Erase>]
    type ins() =
        inherit RegularNode()
    [<Erase>]
    type abbr() =
        inherit RegularNode()
    [<Erase>]
    type dfn() =
        inherit RegularNode()
    [<Erase>]
    type sub() =
        inherit RegularNode()
    [<Erase>]
    type sup() =
        inherit RegularNode()
    [<Erase>]
    type template() =
        inherit RegularNode()

    [<Erase>]
    type br() =
        inherit VoidNode()
    [<Erase>]
    type hr() =
        inherit VoidNode()

    [<Erase>]
    type a() =
        inherit RegularNode()
        [<Erase>]
        member this.href
            with set (_: string) = ()
        [<Erase>]
        member this.hreflang
            with set (_: string) = ()
        [<Erase>]
        member this.rel
            with set (_: string) = ()
        [<Erase>]
        member this.target
            with set (_: string) = ()
        [<Erase>]
        member this.download
            with set (_: string) = ()
        [<Erase>]
        member this.ping
            with set (_: string) = ()
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        [<Erase>]
        member this.referrerpolicy
            with set (_: string) = ()

    [<Erase>]
    type base'() =
        inherit VoidNode()
        [<Erase>]
        member this.href
            with set (_: string) = ()
        [<Erase>]
        member this.target
            with set (_: string) = ()

    [<Erase>]
    type img() =
        inherit VoidNode()
        [<Erase>]
        member this.src
            with set (_: string) = ()
        [<Erase>]
        member this.alt
            with set (_: string) = ()
        [<Erase>]
        member this.width
            with set (_: int) = ()
        [<Erase>]
        member this.height
            with set (_: int) = ()
        [<Erase>]
        member this.srcset
            with set (_: string) = ()
        [<Erase>]
        member this.referrerpolicy
            with set (_: string) = ()
        [<Erase>]
        member this.crossorigin
            with set (_: string) = ()
        [<Erase>]
        member this.sizes
            with set (_: string) = ()
        [<Erase>]
        member this.usemap
            with set (_: string) = ()
        [<Erase>]
        member this.ismap
            with set (_: bool) = ()
        [<Erase>]
        member this.decoding
            with set (_: string) = ()
        [<Erase>]
        member this.loading
            with set (_: string) = ()
        [<Erase>]
        member this.fetchpriority
            with set (_: string) = ()
        [<Erase>]
        member this.elementtiming
            with set (_: string) = ()

    [<Erase>]
    type form() =
        inherit RegularNode()
        [<Erase>]
        member this.action
            with set (_: string) = ()
        [<Erase>]
        member this.method
            with set (_: string) = ()
        [<Erase>]
        member this.enctype
            with set (_: string) = ()
        [<Erase>]
        member this.target
            with set (_: string) = ()
        [<Erase>]
        member this.acceptCharset
            with set (_: string) = ()
        [<Erase>]
        member this.autocomplete
            with set (_: string) = ()
        [<Erase>]
        member this.name
            with set (_: string) = ()
        [<Erase>]
        member this.novalidate
            with set (_: bool) = ()
        [<Erase>]
        member this.rel
            with set (_: string) = ()

    [<Erase>]
    type script() =
        inherit RegularNode()
        [<Erase>]
        member this.src
            with set (_: string) = ()
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        [<Erase>]
        member this.async
            with set (_: bool) = ()
        [<Erase>]
        member this.defer
            with set (_: bool) = ()
        [<Erase>]
        member this.integrity
            with set (_: string) = ()
        [<Erase>]
        member this.crossorigin
            with set (_: string) = ()
        [<Erase>]
        member this.nomodule
            with set (_: bool) = ()
        [<Erase>]
        member this.nonce
            with set (_: string) = ()
        [<Erase>]
        member this.referrerpolicy
            with set (_: string) = ()

    [<Erase>]
    type link() =
        inherit VoidNode()
        [<Erase>]
        member this.rel
            with set (_: string) = ()
        [<Erase>]
        member this.href
            with set (_: string) = ()
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        [<Erase>]
        member this.media
            with set (_: string) = ()
        [<Erase>]
        member this.as'
            with set (_: string) = ()
        [<Erase>]
        member this.sizes
            with set (_: string) = ()
        [<Erase>]
        member this.crossorigin
            with set (_: string) = ()
        [<Erase>]
        member this.integrity
            with set (_: string) = ()
        [<Erase>]
        member this.referrerpolicy
            with set (_: string) = ()
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()
        [<Erase>]
        member this.hreflang
            with set (_: string) = ()
        [<Erase>]
        member this.imagesizes
            with set (_: string) = ()
        [<Erase>]
        member this.imagesrcset
            with set (_: string) = ()
        [<Erase>]
        member this.title
            with set (_: string) = ()

    [<Erase>]
    type html() =
        inherit RegularNode()
        [<Erase>]
        member this.xmlns
            with set (_: string) = ()

    [<Erase>]
    type meta() =
        inherit VoidNode()
        [<Erase>]
        member this.name
            with set (_: string) = ()
        [<Erase>]
        member this.content
            with set (_: string) = ()
        [<Erase>]
        member this.charset
            with set (_: string) = ()
        [<Erase>]
        member this.httpEquiv
            with set (_: string) = ()

    [<Erase>]
    type input() =
        inherit VoidNode()
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        [<Erase>]
        member this.name
            with set (_: string) = ()
        [<Erase>]
        member this.value
            with set (_: string) = ()
        [<Erase>]
        member this.placeholder
            with set (_: string) = ()
        [<Erase>]
        member this.required
            with set (_: bool) = ()
        [<Erase>]
        member this.autofocus
            with set (_: bool) = ()
        [<Erase>]
        member this.autocomplete
            with set (_: string) = ()
        [<Erase>]
        member this.min
            with set (_: string) = ()
        [<Erase>]
        member this.max
            with set (_: string) = ()
        [<Erase>]
        member this.step
            with set (_: string) = ()
        [<Erase>]
        member this.pattern
            with set (_: string) = ()
        [<Erase>]
        member this.readonly
            with set (_: bool) = ()
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()
        [<Erase>]
        member this.multiple
            with set (_: bool) = ()
        [<Erase>]
        member this.accept
            with set (_: string) = ()
        [<Erase>]
        member this.list
            with set (_: string) = ()
        [<Erase>]
        member this.maxlength
            with set (_: int) = ()
        [<Erase>]
        member this.minlength
            with set (_: int) = ()
        [<Erase>]
        member this.size
            with set (_: int) = ()
        [<Erase>]
        member this.src
            with set (_: string) = ()
        [<Erase>]
        member this.width
            with set (_: int) = ()
        [<Erase>]
        member this.height
            with set (_: int) = ()
        [<Erase>]
        member this.alt
            with set (_: string) = ()
        [<Erase>]
        member this.checked'
            with set (_: bool) = ()
        [<Erase>]
        member this.dirname
            with set (_: string) = ()
        [<Erase>]
        member this.form
            with set (_: string) = ()
        [<Erase>]
        member this.formaction
            with set (_: string) = ()
        [<Erase>]
        member this.formenctype
            with set (_: string) = ()
        [<Erase>]
        member this.formmethod
            with set (_: string) = ()
        [<Erase>]
        member this.formnovalidate
            with set (_: bool) = ()
        [<Erase>]
        member this.formtarget
            with set (_: string) = ()
        [<Erase>]
        member this.inputmode
            with set (_: string) = ()
        [<Erase>]
        member this.capture
            with set (_: string) = ()

    [<Erase>]
    type output() =
        inherit RegularNode()
        [<Erase>]
        member this.for'
            with set (_: string) = ()
        [<Erase>]
        member this.form
            with set (_: string) = ()
        [<Erase>]
        member this.name
            with set (_: string) = ()

    [<Erase>]
    type textarea() =
        inherit RegularNode()
        [<Erase>]
        member this.name
            with set (_: string) = ()
        [<Erase>]
        member this.placeholder
            with set (_: string) = ()
        [<Erase>]
        member this.required
            with set (_: bool) = ()
        [<Erase>]
        member this.autofocus
            with set (_: bool) = ()
        [<Erase>]
        member this.autocomplete
            with set (_: string) = ()
        [<Erase>]
        member this.readonly
            with set (_: bool) = ()
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()
        [<Erase>]
        member this.rows
            with set (_: int) = ()
        [<Erase>]
        member this.cols
            with set (_: int) = ()
        [<Erase>]
        member this.wrap
            with set (_: string) = ()
        [<Erase>]
        member this.maxlength
            with set (_: int) = ()
        [<Erase>]
        member this.minlength
            with set (_: int) = ()
        [<Erase>]
        member this.dirname
            with set (_: string) = ()
        [<Erase>]
        member this.form
            with set (_: string) = ()


    [<Erase>]
    type button() =
        inherit RegularNode()
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        [<Erase>]
        member this.name
            with set (_: string) = ()
        [<Erase>]
        member this.value
            with set (_: string) = ()
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()
        [<Erase>]
        member this.autofocus
            with set (_: bool) = ()
        [<Erase>]
        member this.form
            with set (_: string) = ()
        [<Erase>]
        member this.formaction
            with set (_: string) = ()
        [<Erase>]
        member this.formenctype
            with set (_: string) = ()
        [<Erase>]
        member this.formmethod
            with set (_: string) = ()
        [<Erase>]
        member this.formnovalidate
            with set (_: bool) = ()
        [<Erase>]
        member this.formtarget
            with set (_: string) = ()
        [<Erase>]
        member this.popovertarget
            with set (_: string) = ()
        [<Erase>]
        member this.popovertargetaction
            with set (_: string) = ()

    [<Erase>]
    type select() =
        inherit RegularNode()
        [<Erase>]
        member this.name
            with set (_: string) = ()
        [<Erase>]
        member this.required
            with set (_: bool) = ()
        [<Erase>]
        member this.autofocus
            with set (_: bool) = ()
        [<Erase>]
        member this.autocomplete
            with set (_: string) = ()
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()
        [<Erase>]
        member this.multiple
            with set (_: bool) = ()
        [<Erase>]
        member this.size
            with set (_: int) = ()
        [<Erase>]
        member this.form
            with set (_: string) = ()

    [<Erase>]
    type option() =
        inherit RegularNode()
        [<Erase>]
        member this.value
            with set (_: string) = ()
        [<Erase>]
        member this.selected
            with set (_: bool) = ()
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()
        [<Erase>]
        member this.label
            with set (_: string) = ()

    [<Erase>]
    type optgroup() =
        inherit RegularNode()
        [<Erase>]
        member this.label
            with set (_: string) = ()
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()

    [<Erase>]
    type label() =
        inherit RegularNode()
        [<Erase>]
        member this.for'
            with set (_: string) = ()

    [<Erase>]
    type style() =
        inherit RegularNode()
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        [<Erase>]
        member this.media
            with set (_: string) = ()

    [<Erase>]
    type iframe() =
        inherit RegularNode()
        [<Erase>]
        member this.src
            with set (_: string) = ()
        [<Erase>]
        member this.name
            with set (_: string) = ()
        [<Erase>]
        member this.sandbox
            with set (_: string) = ()
        [<Erase>]
        member this.width
            with set (_: int) = ()
        [<Erase>]
        member this.height
            with set (_: int) = ()
        [<Erase>]
        member this.allow
            with set (_: string) = ()
        [<Erase>]
        member this.loading
            with set (_: string) = ()
        [<Erase>]
        member this.referrerpolicy
            with set (_: string) = ()
        [<Erase>]
        member this.srcdoc
            with set (_: string) = ()

    [<Erase>]
    type video() =
        inherit RegularNode()
        [<Erase>]
        member this.src
            with set (_: string) = ()
        [<Erase>]
        member this.poster
            with set (_: string) = ()
        [<Erase>]
        member this.autoplay
            with set (_: bool) = ()
        [<Erase>]
        member this.controls
            with set (_: bool) = ()
        [<Erase>]
        member this.playsinline
            with set (_: bool) = ()
        [<Erase>]
        member this.controlsList
            with set (_: string) = ()
        [<Erase>]
        member this.crossorigin
            with set (_: string) = ()
        [<Erase>]
        member this.loop
            with set (_: bool) = ()
        [<Erase>]
        member this.muted
            with set (_: bool) = ()
        [<Erase>]
        member this.width
            with set (_: int) = ()
        [<Erase>]
        member this.height
            with set (_: int) = ()
        [<Erase>]
        member this.preload
            with set (_: string) = ()
        [<Erase>]
        member this.disableremoteplayback
            with set (_: bool) = ()
        [<Erase>]
        member this.disablepictureinpicture
            with set (_: bool) = ()

    [<Erase>]
    type audio() =
        inherit RegularNode()
        [<Erase>]
        member this.src
            with set (_: string) = ()
        [<Erase>]
        member this.autoplay
            with set (_: bool) = ()
        [<Erase>]
        member this.controls
            with set (_: bool) = ()
        [<Erase>]
        member this.controlsList
            with set (_: string) = ()
        [<Erase>]
        member this.crossorigin
            with set (_: string) = ()
        [<Erase>]
        member this.preload
            with set (_: string) = ()
        [<Erase>]
        member this.loop
            with set (_: bool) = ()
        [<Erase>]
        member this.muted
            with set (_: bool) = ()
        [<Erase>]
        member this.disableremoteplayback
            with set (_: bool) = ()

    [<Erase>]
    type source() =
        inherit VoidNode()
        [<Erase>]
        member this.src
            with set (_: string) = ()
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        [<Erase>]
        member this.media
            with set (_: string) = ()
        [<Erase>]
        member this.sizes
            with set (_: string) = ()
        [<Erase>]
        member this.srcset
            with set (_: string) = ()

    [<Erase>]
    type canvas() =
        inherit RegularNode()
        [<Erase>]
        member this.width
            with set (_: int) = ()
        [<Erase>]
        member this.height
            with set (_: int) = ()

    [<Erase>]
    type object'() =
        inherit RegularNode()
        [<Erase>]
        member this.data
            with set (_: string) = ()
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        [<Erase>]
        member this.width
            with set (_: int) = ()
        [<Erase>]
        member this.height
            with set (_: int) = ()

    [<Erase>]
    type param() =
        inherit VoidNode()
        [<Erase>]
        member this.name
            with set (_: string) = ()
        [<Erase>]
        member this.value
            with set (_: string) = ()

    [<Erase>]
    type data() =
        inherit RegularNode()
        [<Erase>]
        member this.value
            with set (_: string) = ()

    [<Erase>]
    type time() =
        inherit RegularNode()
        [<Erase>]
        member this.datetime
            with set (_: string) = ()

    [<Erase>]
    type progress() =
        inherit RegularNode()
        [<Erase>]
        member this.value
            with set (_: string) = ()
        [<Erase>]
        member this.max
            with set (_: string) = ()

    [<Erase>]
    type meter() =
        inherit RegularNode()
        [<Erase>]
        member this.form
            with set (_: string) = ()
        [<Erase>]
        member this.value
            with set (_: string) = ()
        [<Erase>]
        member this.min
            with set (_: string) = ()
        [<Erase>]
        member this.max
            with set (_: string) = ()
        [<Erase>]
        member this.low
            with set (_: string) = ()
        [<Erase>]
        member this.high
            with set (_: string) = ()
        [<Erase>]
        member this.optimum
            with set (_: string) = ()

    [<Erase>]
    type details() =
        inherit RegularNode()
        [<Erase>]
        member this.open'
            with set (_: bool) = ()

    [<Erase>]
    type summary() =
        inherit RegularNode()

    [<Erase>]
    type dialog() =
        inherit RegularNode()
        [<Erase>]
        member this.open'
            with set (_: bool) = ()

    [<Erase>]
    type menu() =
        inherit RegularNode()

    [<Erase>]
    type datalist() =
        inherit RegularNode()

    [<Erase>]
    type fieldset() =
        inherit RegularNode()
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()

        [<Erase>]
        member this.form
            with set (_: string) = ()
        [<Erase>]
        member this.name
            with set (_: string) = ()

    [<Erase>]
    type legend() =
        inherit RegularNode()
    [<Erase>]
    type table() =
        inherit RegularNode()
    [<Erase>]
    type tbody() =
        inherit RegularNode()
    [<Erase>]
    type thead() =
        inherit RegularNode()
    [<Erase>]
    type tfoot() =
        inherit RegularNode()
    [<Erase>]
    type tr() =
        inherit RegularNode()
    [<Erase>]
    type th() =
        inherit RegularNode()
        [<Erase>]
        member this.abbr
            with set (_: string) = ()
        [<Erase>]
        member this.colspan
            with set (_: int) = ()
        [<Erase>]
        member this.rowspan
            with set (_: int) = ()
        [<Erase>]
        member this.headers
            with set (_: string) = ()
        [<Erase>]
        member this.scope
            with set (_: string) = ()
    [<Erase>]
    type td() =
        inherit RegularNode()
        [<Erase>]
        member this.colspan
            with set (_: int) = ()
        [<Erase>]
        member this.rowspan
            with set (_: int) = ()
        [<Erase>]
        member this.headers
            with set (_: string) = ()

    [<Erase>]
    type map() =
        inherit RegularNode()
        [<Erase>]
        member this.name
            with set (_: string) = ()
    [<Erase>]
    type area() =
        inherit VoidNode()
        [<Erase>]
        member this.shape
            with set (_: string) = ()
        [<Erase>]
        member this.coords
            with set (_: string) = ()
        [<Erase>]
        member this.href
            with set (_: string) = ()
        [<Erase>]
        member this.alt
            with set (_: string) = ()
        [<Erase>]
        member this.download
            with set (_: string) = ()
        [<Erase>]
        member this.target
            with set (_: string) = ()
        [<Erase>]
        member this.rel
            with set (_: string) = ()
        [<Erase>]
        member this.referrerpolicy
            with set (_: string) = ()
        [<Erase>]
        member this.ping
            with set (_: string) = ()

    [<Erase>]
    type aside() =
        inherit RegularNode()
    [<Erase>]
    type bdi() =
        inherit RegularNode()
    [<Erase>]
    type bdo() =
        inherit RegularNode()
    [<Erase>]
    type col() =
        inherit VoidNode()
        [<Erase>]
        member this.span
            with set (_: int) = ()
    [<Erase>]
    type colgroup() =
        inherit RegularNode()
        [<Erase>]
        member this.span
            with set (_: int) = ()
    [<Erase>]
    type dd() =
        inherit RegularNode()
    [<Erase>]
    type dl() =
        inherit RegularNode()
    [<Erase>]
    type dt() =
        inherit RegularNode()
    [<Erase>]
    type embed() =
        inherit VoidNode()
        [<Erase>]
        member this.src
            with set (_: string) = ()
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        [<Erase>]
        member this.width
            with set (_: int) = ()
        [<Erase>]
        member this.height
            with set (_: int) = ()
    [<Erase>]
    type figcaption() =
        inherit RegularNode()
    [<Erase>]
    type figure() =
        inherit RegularNode()
    [<Erase>]
    type kbd() =
        inherit RegularNode()
    [<Erase>]
    type mark() =
        inherit RegularNode()
    [<Erase>]
    type picture() =
        inherit RegularNode()
    [<Erase>]
    type samp() =
        inherit RegularNode()
    [<Erase>]
    type var() =
        inherit RegularNode()
    [<Erase>]
    type wbr() =
        inherit RegularNode()
