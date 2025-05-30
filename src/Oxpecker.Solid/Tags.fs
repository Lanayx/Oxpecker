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
        interface FragmentNode

    /// Set of html extensions that keep original type
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
        interface RegularNode
    [<Erase>]
    type body() =
        interface RegularNode
    [<Erase>]
    type title() =
        interface RegularNode
    [<Erase>]
    type div() =
        interface RegularNode
    [<Erase>]
    type article() =
        interface RegularNode
    [<Erase>]
    type section() =
        interface RegularNode
    [<Erase>]
    type header() =
        interface RegularNode
    [<Erase>]
    type footer() =
        interface RegularNode
    [<Erase>]
    type main() =
        interface RegularNode
    [<Erase>]
    type h1() =
        interface RegularNode
    [<Erase>]
    type h2() =
        interface RegularNode
    [<Erase>]
    type h3() =
        interface RegularNode
    [<Erase>]
    type h4() =
        interface RegularNode
    [<Erase>]
    type h5() =
        interface RegularNode
    [<Erase>]
    type h6() =
        interface RegularNode
    [<Erase>]
    type ul() =
        interface RegularNode
    [<Erase>]
    type ol() =
        interface RegularNode
    [<Erase>]
    type li() =
        interface RegularNode
    [<Erase>]
    type p() =
        interface RegularNode
    [<Erase>]
    type span() =
        interface RegularNode
    [<Erase>]
    type small() =
        interface RegularNode
    [<Erase>]
    type strong() =
        interface RegularNode
    [<Erase>]
    type em() =
        interface RegularNode
    [<Erase>]
    type caption() =
        interface RegularNode
    [<Erase>]
    type nav() =
        interface RegularNode
    [<Erase>]
    type search() =
        interface RegularNode
    [<Erase>]
    type i() =
        interface RegularNode
    [<Erase>]
    type b() =
        interface RegularNode
    [<Erase>]
    type u() =
        interface RegularNode
    [<Erase>]
    type s() =
        interface RegularNode
    [<Erase>]
    type noscript() =
        interface RegularNode
    [<Erase>]
    type code() =
        interface RegularNode
    [<Erase>]
    type pre() =
        interface RegularNode
    [<Erase>]
    type blockquote() =
        interface RegularNode
    [<Erase>]
    type cite() =
        interface RegularNode
    [<Erase>]
    type q() =
        interface RegularNode
    [<Erase>]
    type address() =
        interface RegularNode
    [<Erase>]
    type del() =
        interface RegularNode
    [<Erase>]
    type ins() =
        interface RegularNode
    [<Erase>]
    type abbr() =
        interface RegularNode
    [<Erase>]
    type dfn() =
        interface RegularNode
    [<Erase>]
    type sub() =
        interface RegularNode
    [<Erase>]
    type sup() =
        interface RegularNode
    [<Erase>]
    type template() =
        interface RegularNode

    [<Erase>]
    type br() =
        interface VoidNode
    [<Erase>]
    type hr() =
        interface VoidNode

    [<Erase>]
    type a() =
        interface RegularNode
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
        interface VoidNode
        [<Erase>]
        member this.href
            with set (_: string) = ()
        [<Erase>]
        member this.target
            with set (_: string) = ()

    [<Erase>]
    type img() =
        interface VoidNode
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
        interface RegularNode
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
        interface RegularNode
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
        interface VoidNode
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
        interface RegularNode
        [<Erase>]
        member this.xmlns
            with set (_: string) = ()

    [<Erase>]
    type meta() =
        interface VoidNode
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
        interface VoidNode
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
        interface RegularNode
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
        interface RegularNode
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
        interface RegularNode
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
        interface RegularNode
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
        interface RegularNode
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
        interface RegularNode
        [<Erase>]
        member this.label
            with set (_: string) = ()
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()

    [<Erase>]
    type label() =
        interface RegularNode
        [<Erase>]
        member this.for'
            with set (_: string) = ()

    [<Erase>]
    type style() =
        interface RegularNode
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        [<Erase>]
        member this.media
            with set (_: string) = ()

    [<Erase>]
    type iframe() =
        interface RegularNode
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
        interface RegularNode
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
        interface RegularNode
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
        interface VoidNode
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
        interface RegularNode
        [<Erase>]
        member this.width
            with set (_: int) = ()
        [<Erase>]
        member this.height
            with set (_: int) = ()

    [<Erase>]
    type object'() =
        interface RegularNode
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
        interface VoidNode
        [<Erase>]
        member this.name
            with set (_: string) = ()
        [<Erase>]
        member this.value
            with set (_: string) = ()

    [<Erase>]
    type data() =
        interface RegularNode
        [<Erase>]
        member this.value
            with set (_: string) = ()

    [<Erase>]
    type time() =
        interface RegularNode
        [<Erase>]
        member this.datetime
            with set (_: string) = ()

    [<Erase>]
    type progress() =
        interface RegularNode
        [<Erase>]
        member this.value
            with set (_: string) = ()
        [<Erase>]
        member this.max
            with set (_: string) = ()

    [<Erase>]
    type meter() =
        interface RegularNode
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
        interface RegularNode
        [<Erase>]
        member this.open'
            with set (_: bool) = ()

    [<Erase>]
    type summary() =
        interface RegularNode

    [<Erase>]
    type dialog() =
        interface RegularNode
        [<Erase>]
        member this.open'
            with set (_: bool) = ()

    [<Erase>]
    type menu() =
        interface RegularNode

    [<Erase>]
    type datalist() =
        interface RegularNode

    [<Erase>]
    type fieldset() =
        interface RegularNode
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
        interface RegularNode
    [<Erase>]
    type table() =
        interface RegularNode
    [<Erase>]
    type tbody() =
        interface RegularNode
    [<Erase>]
    type thead() =
        interface RegularNode
    [<Erase>]
    type tfoot() =
        interface RegularNode
    [<Erase>]
    type tr() =
        interface RegularNode
    [<Erase>]
    type th() =
        interface RegularNode
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
        interface RegularNode
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
        interface RegularNode
        [<Erase>]
        member this.name
            with set (_: string) = ()
    [<Erase>]
    type area() =
        interface VoidNode
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
        interface RegularNode
    [<Erase>]
    type bdi() =
        interface RegularNode
    [<Erase>]
    type bdo() =
        interface RegularNode
    [<Erase>]
    type col() =
        interface VoidNode
        [<Erase>]
        member this.span
            with set (_: int) = ()
    [<Erase>]
    type colgroup() =
        interface RegularNode
        [<Erase>]
        member this.span
            with set (_: int) = ()
    [<Erase>]
    type dd() =
        interface RegularNode
    [<Erase>]
    type dl() =
        interface RegularNode
    [<Erase>]
    type dt() =
        interface RegularNode
    [<Erase>]
    type embed() =
        interface VoidNode
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
        interface RegularNode
    [<Erase>]
    type figure() =
        interface RegularNode
    [<Erase>]
    type kbd() =
        interface RegularNode
    [<Erase>]
    type mark() =
        interface RegularNode
    [<Erase>]
    type picture() =
        interface RegularNode
    [<Erase>]
    type samp() =
        interface RegularNode
    [<Erase>]
    type var() =
        interface RegularNode
    [<Erase>]
    type wbr() =
        interface RegularNode
