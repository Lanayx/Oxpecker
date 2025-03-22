namespace Oxpecker.Solid

open System
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
    /// Set of html extensions that keep original type
    [<Extension>]
    [<Erase>]
    type SvgHtmlElementExtensions =

        /// Add an attribute to the element
        [<Extension; Erase>]
        static member attr(this: #SvgTag, name: string, value: string) = this

        /// Add event handler to the element through the corresponding attribute
        [<Extension; Erase>]
        static member on(this: #SvgTag, eventName: string, eventHandler: Event -> unit) = this

        /// Add data attribute to the element
        [<Extension; Erase>]
        static member data(this: #SvgTag, name: string, value: string) = this

        /// Referenced native HTML element
        [<Extension; Erase>]
        static member ref(this: #SvgTag, el: #Element) = this

        /// Referenced native HTML element (before connecting to DOM)
        [<Extension; Erase>]
        static member ref(this: #SvgTag, el: #Element -> unit) = this

        /// Usage `elem.style(createObj ["color", "green"; "background-color", state.myColor ])`
        [<Extension; Erase>]
        static member style'(this: #SvgTag, styleObj: obj) = this

        /// Usage `elem.classList(createObj ["active", true; "disabled", state.disabled ])`
        [<Extension; Erase>]
        static member classList(this: #SvgTag, classListObj: obj) = this

        /// Adds or removes attribute without value
        [<Extension; Erase>]
        static member bool(this: #SvgTag, name: string, value: bool) = this

    // global attributes
    type HtmlTag with
        [<Erase>]
        member this.id
            with set (value: string) = ()
        [<Erase>]
        member this.class'
            with set (value: string) = ()
        [<LanguageInjection(InjectedLanguage.CSS, Prefix = ".x{", Suffix = ";}")>]
        [<Erase>]
        member this.style
            with set (value: string) = ()
        [<Erase>]
        member this.lang
            with set (value: string) = ()
        [<Erase>]
        member this.dir
            with set (value: string) = ()
        [<Erase>]
        member this.tabindex
            with set (value: int) = ()
        [<Erase>]
        member this.title
            with set (value: string) = ()
        [<Erase>]
        member this.accesskey
            with set (value: char) = ()
        [<Erase>]
        member this.contenteditable
            with set (value: string) = ()
        [<Erase>]
        member this.draggable
            with set (value: string) = ()
        [<Erase>]
        member this.enterkeyhint
            with set (value: string) = ()
        [<Erase>]
        member this.hidden
            with set (value: string) = ()
        [<Erase>]
        member this.inert
            with set (value: bool) = ()
        [<Erase>]
        member this.inputmode
            with set (value: string) = ()
        [<Erase>]
        member this.popover
            with set (value: string) = ()
        [<Erase>]
        member this.spellcheck
            with set (value: bool) = ()
        [<Erase>]
        member this.translate
            with set (value: string) = ()
        [<Erase>]
        member this.autocapitalize
            with set (value: string) = ()
        [<Erase>]
        member this.is
            with set (value: string) = ()
        [<Erase>]
        member this.part
            with set (value: string) = ()
        [<Erase>]
        member this.slot
            with set (value: string) = ()

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
            with set (value: string) = ()
        [<Erase>]
        member this.hreflang
            with set (value: string) = ()
        [<Erase>]
        member this.rel
            with set (value: string) = ()
        [<Erase>]
        member this.target
            with set (value: string) = ()
        [<Erase>]
        member this.download
            with set (value: string) = ()
        [<Erase>]
        member this.ping
            with set (value: string) = ()
        [<Erase>]
        member this.type'
            with set (value: string) = ()
        [<Erase>]
        member this.referrerpolicy
            with set (value: string) = ()

    [<Erase>]
    type base'() =
        inherit VoidNode()
        [<Erase>]
        member this.href
            with set (value: string) = ()
        [<Erase>]
        member this.target
            with set (value: string) = ()

    [<Erase>]
    type img() =
        inherit VoidNode()
        [<Erase>]
        member this.src
            with set (value: string) = ()
        [<Erase>]
        member this.alt
            with set (value: string) = ()
        [<Erase>]
        member this.width
            with set (value: int) = ()
        [<Erase>]
        member this.height
            with set (value: int) = ()
        [<Erase>]
        member this.srcset
            with set (value: string) = ()
        [<Erase>]
        member this.referrerpolicy
            with set (value: string) = ()
        [<Erase>]
        member this.crossorigin
            with set (value: string) = ()
        [<Erase>]
        member this.sizes
            with set (value: string) = ()
        [<Erase>]
        member this.usemap
            with set (value: string) = ()
        [<Erase>]
        member this.ismap
            with set (value: bool) = ()
        [<Erase>]
        member this.decoding
            with set (value: string) = ()
        [<Erase>]
        member this.loading
            with set (value: string) = ()
        [<Erase>]
        member this.fetchpriority
            with set (value: string) = ()
        [<Erase>]
        member this.elementtiming
            with set (value: string) = ()

    [<Erase>]
    type form() =
        inherit RegularNode()
        [<Erase>]
        member this.action
            with set (value: string) = ()
        [<Erase>]
        member this.method
            with set (value: string) = ()
        [<Erase>]
        member this.enctype
            with set (value: string) = ()
        [<Erase>]
        member this.target
            with set (value: string) = ()
        [<Erase>]
        member this.acceptCharset
            with set (value: string) = ()
        [<Erase>]
        member this.autocomplete
            with set (value: string) = ()
        [<Erase>]
        member this.name
            with set (value: string) = ()
        [<Erase>]
        member this.novalidate
            with set (value: bool) = ()
        [<Erase>]
        member this.rel
            with set (value: string) = ()

    [<Erase>]
    type script() =
        inherit RegularNode()
        [<Erase>]
        member this.src
            with set (value: string) = ()
        [<Erase>]
        member this.type'
            with set (value: string) = ()
        [<Erase>]
        member this.async
            with set (value: bool) = ()
        [<Erase>]
        member this.defer
            with set (value: bool) = ()
        [<Erase>]
        member this.integrity
            with set (value: string) = ()
        [<Erase>]
        member this.crossorigin
            with set (value: string) = ()
        [<Erase>]
        member this.nomodule
            with set (value: bool) = ()
        [<Erase>]
        member this.nonce
            with set (value: string) = ()
        [<Erase>]
        member this.referrerpolicy
            with set (value: string) = ()

    [<Erase>]
    type link() =
        inherit VoidNode()
        [<Erase>]
        member this.rel
            with set (value: string) = ()
        [<Erase>]
        member this.href
            with set (value: string) = ()
        [<Erase>]
        member this.type'
            with set (value: string) = ()
        [<Erase>]
        member this.media
            with set (value: string) = ()
        [<Erase>]
        member this.as'
            with set (value: string) = ()
        [<Erase>]
        member this.sizes
            with set (value: string) = ()
        [<Erase>]
        member this.crossorigin
            with set (value: string) = ()
        [<Erase>]
        member this.integrity
            with set (value: string) = ()
        [<Erase>]
        member this.referrerpolicy
            with set (value: string) = ()
        [<Erase>]
        member this.disabled
            with set (value: bool) = ()
        [<Erase>]
        member this.hreflang
            with set (value: string) = ()
        [<Erase>]
        member this.imagesizes
            with set (value: string) = ()
        [<Erase>]
        member this.imagesrcset
            with set (value: string) = ()
        [<Erase>]
        member this.title
            with set (value: string) = ()

    [<Erase>]
    type html() =
        inherit RegularNode()
        [<Erase>]
        member this.xmlns
            with set (value: string) = ()

    [<Erase>]
    type meta() =
        inherit VoidNode()
        [<Erase>]
        member this.name
            with set (value: string) = ()
        [<Erase>]
        member this.content
            with set (value: string) = ()
        [<Erase>]
        member this.charset
            with set (value: string) = ()
        [<Erase>]
        member this.httpEquiv
            with set (value: string) = ()

    [<Erase>]
    type input() =
        inherit VoidNode()
        [<Erase>]
        member this.type'
            with set (value: string) = ()
        [<Erase>]
        member this.name
            with set (value: string) = ()
        [<Erase>]
        member this.value
            with set (value: string) = ()
        [<Erase>]
        member this.placeholder
            with set (value: string) = ()
        [<Erase>]
        member this.required
            with set (value: bool) = ()
        [<Erase>]
        member this.autofocus
            with set (value: bool) = ()
        [<Erase>]
        member this.autocomplete
            with set (value: string) = ()
        [<Erase>]
        member this.min
            with set (value: string) = ()
        [<Erase>]
        member this.max
            with set (value: string) = ()
        [<Erase>]
        member this.step
            with set (value: string) = ()
        [<Erase>]
        member this.pattern
            with set (value: string) = ()
        [<Erase>]
        member this.readonly
            with set (value: bool) = ()
        [<Erase>]
        member this.disabled
            with set (value: bool) = ()
        [<Erase>]
        member this.multiple
            with set (value: bool) = ()
        [<Erase>]
        member this.accept
            with set (value: string) = ()
        [<Erase>]
        member this.list
            with set (value: string) = ()
        [<Erase>]
        member this.maxlength
            with set (value: int) = ()
        [<Erase>]
        member this.minlength
            with set (value: int) = ()
        [<Erase>]
        member this.size
            with set (value: int) = ()
        [<Erase>]
        member this.src
            with set (value: string) = ()
        [<Erase>]
        member this.width
            with set (value: int) = ()
        [<Erase>]
        member this.height
            with set (value: int) = ()
        [<Erase>]
        member this.alt
            with set (value: string) = ()
        [<Erase>]
        member this.checked'
            with set (value: bool) = ()
        [<Erase>]
        member this.dirname
            with set (value: string) = ()
        [<Erase>]
        member this.form
            with set (value: string) = ()
        [<Erase>]
        member this.formaction
            with set (value: string) = ()
        [<Erase>]
        member this.formenctype
            with set (value: string) = ()
        [<Erase>]
        member this.formmethod
            with set (value: string) = ()
        [<Erase>]
        member this.formnovalidate
            with set (value: bool) = ()
        [<Erase>]
        member this.formtarget
            with set (value: string) = ()
        [<Erase>]
        member this.inputmode
            with set (value: string) = ()
        [<Erase>]
        member this.capture
            with set (value: string) = ()

    [<Erase>]
    type output() =
        inherit RegularNode()
        [<Erase>]
        member this.for'
            with set (value: string) = ()
        [<Erase>]
        member this.form
            with set (value: string) = ()
        [<Erase>]
        member this.name
            with set (value: string) = ()

    [<Erase>]
    type textarea() =
        inherit RegularNode()
        [<Erase>]
        member this.name
            with set (value: string) = ()
        [<Erase>]
        member this.placeholder
            with set (value: string) = ()
        [<Erase>]
        member this.required
            with set (value: bool) = ()
        [<Erase>]
        member this.autofocus
            with set (value: bool) = ()
        [<Erase>]
        member this.autocomplete
            with set (value: string) = ()
        [<Erase>]
        member this.readonly
            with set (value: bool) = ()
        [<Erase>]
        member this.disabled
            with set (value: bool) = ()
        [<Erase>]
        member this.rows
            with set (value: int) = ()
        [<Erase>]
        member this.cols
            with set (value: int) = ()
        [<Erase>]
        member this.wrap
            with set (value: string) = ()
        [<Erase>]
        member this.maxlength
            with set (value: int) = ()
        [<Erase>]
        member this.minlength
            with set (value: int) = ()
        [<Erase>]
        member this.dirname
            with set (value: string) = ()
        [<Erase>]
        member this.form
            with set (value: string) = ()


    [<Erase>]
    type button() =
        inherit RegularNode()
        [<Erase>]
        member this.type'
            with set (value: string) = ()
        [<Erase>]
        member this.name
            with set (value: string) = ()
        [<Erase>]
        member this.value
            with set (value: string) = ()
        [<Erase>]
        member this.disabled
            with set (value: bool) = ()
        [<Erase>]
        member this.autofocus
            with set (value: bool) = ()
        [<Erase>]
        member this.form
            with set (value: string) = ()
        [<Erase>]
        member this.formaction
            with set (value: string) = ()
        [<Erase>]
        member this.formenctype
            with set (value: string) = ()
        [<Erase>]
        member this.formmethod
            with set (value: string) = ()
        [<Erase>]
        member this.formnovalidate
            with set (value: bool) = ()
        [<Erase>]
        member this.formtarget
            with set (value: string) = ()
        [<Erase>]
        member this.popovertarget
            with set (value: string) = ()
        [<Erase>]
        member this.popovertargetaction
            with set (value: string) = ()

    [<Erase>]
    type select() =
        inherit RegularNode()
        [<Erase>]
        member this.name
            with set (value: string) = ()
        [<Erase>]
        member this.required
            with set (value: bool) = ()
        [<Erase>]
        member this.autofocus
            with set (value: bool) = ()
        [<Erase>]
        member this.autocomplete
            with set (value: string) = ()
        [<Erase>]
        member this.disabled
            with set (value: bool) = ()
        [<Erase>]
        member this.multiple
            with set (value: bool) = ()
        [<Erase>]
        member this.size
            with set (value: int) = ()
        [<Erase>]
        member this.form
            with set (value: string) = ()

    [<Erase>]
    type option() =
        inherit RegularNode()
        [<Erase>]
        member this.value
            with set (value: string) = ()
        [<Erase>]
        member this.selected
            with set (value: bool) = ()
        [<Erase>]
        member this.disabled
            with set (value: bool) = ()
        [<Erase>]
        member this.label
            with set (value: string) = ()

    [<Erase>]
    type optgroup() =
        inherit RegularNode()
        [<Erase>]
        member this.label
            with set (value: string) = ()
        [<Erase>]
        member this.disabled
            with set (value: bool) = ()

    [<Erase>]
    type label() =
        inherit RegularNode()
        [<Erase>]
        member this.for'
            with set (value: string) = ()

    [<Erase>]
    type style() =
        inherit RegularNode()
        [<Erase>]
        member this.type'
            with set (value: string) = ()
        [<Erase>]
        member this.media
            with set (value: string) = ()

    [<Erase>]
    type iframe() =
        inherit RegularNode()
        [<Erase>]
        member this.src
            with set (value: string) = ()
        [<Erase>]
        member this.name
            with set (value: string) = ()
        [<Erase>]
        member this.sandbox
            with set (value: string) = ()
        [<Erase>]
        member this.width
            with set (value: int) = ()
        [<Erase>]
        member this.height
            with set (value: int) = ()
        [<Erase>]
        member this.allow
            with set (value: string) = ()
        [<Erase>]
        member this.loading
            with set (value: string) = ()
        [<Erase>]
        member this.referrerpolicy
            with set (value: string) = ()
        [<Erase>]
        member this.srcdoc
            with set (value: string) = ()

    [<Erase>]
    type video() =
        inherit RegularNode()
        [<Erase>]
        member this.src
            with set (value: string) = ()
        [<Erase>]
        member this.poster
            with set (value: string) = ()
        [<Erase>]
        member this.autoplay
            with set (value: bool) = ()
        [<Erase>]
        member this.controls
            with set (value: bool) = ()
        [<Erase>]
        member this.playsinline
            with set (value: bool) = ()
        [<Erase>]
        member this.controlsList
            with set (value: string) = ()
        [<Erase>]
        member this.crossorigin
            with set (value: string) = ()
        [<Erase>]
        member this.loop
            with set (value: bool) = ()
        [<Erase>]
        member this.muted
            with set (value: bool) = ()
        [<Erase>]
        member this.width
            with set (value: int) = ()
        [<Erase>]
        member this.height
            with set (value: int) = ()
        [<Erase>]
        member this.preload
            with set (value: string) = ()
        [<Erase>]
        member this.disableremoteplayback
            with set (value: bool) = ()
        [<Erase>]
        member this.disablepictureinpicture
            with set (value: bool) = ()

    [<Erase>]
    type audio() =
        inherit RegularNode()
        [<Erase>]
        member this.src
            with set (value: string) = ()
        [<Erase>]
        member this.autoplay
            with set (value: bool) = ()
        [<Erase>]
        member this.controls
            with set (value: bool) = ()
        [<Erase>]
        member this.controlsList
            with set (value: string) = ()
        [<Erase>]
        member this.crossorigin
            with set (value: string) = ()
        [<Erase>]
        member this.preload
            with set (value: string) = ()
        [<Erase>]
        member this.loop
            with set (value: bool) = ()
        [<Erase>]
        member this.muted
            with set (value: bool) = ()
        [<Erase>]
        member this.disableremoteplayback
            with set (value: bool) = ()

    [<Erase>]
    type source() =
        inherit VoidNode()
        [<Erase>]
        member this.src
            with set (value: string) = ()
        [<Erase>]
        member this.type'
            with set (value: string) = ()
        [<Erase>]
        member this.media
            with set (value: string) = ()
        [<Erase>]
        member this.sizes
            with set (value: string) = ()
        [<Erase>]
        member this.srcset
            with set (value: string) = ()

    [<Erase>]
    type canvas() =
        inherit RegularNode()
        [<Erase>]
        member this.width
            with set (value: int) = ()
        [<Erase>]
        member this.height
            with set (value: int) = ()

    [<Erase>]
    type object'() =
        inherit RegularNode()
        [<Erase>]
        member this.data
            with set (value: string) = ()
        [<Erase>]
        member this.type'
            with set (value: string) = ()
        [<Erase>]
        member this.width
            with set (value: int) = ()
        [<Erase>]
        member this.height
            with set (value: int) = ()

    [<Erase>]
    type param() =
        inherit VoidNode()
        [<Erase>]
        member this.name
            with set (value: string) = ()
        [<Erase>]
        member this.value
            with set (value: string) = ()

    [<Erase>]
    type data() =
        inherit RegularNode()
        [<Erase>]
        member this.value
            with set (value: string) = ()

    [<Erase>]
    type time() =
        inherit RegularNode()
        [<Erase>]
        member this.datetime
            with set (value: string) = ()

    [<Erase>]
    type progress() =
        inherit RegularNode()
        [<Erase>]
        member this.value
            with set (value: string) = ()
        [<Erase>]
        member this.max
            with set (value: string) = ()

    [<Erase>]
    type meter() =
        inherit RegularNode()
        [<Erase>]
        member this.form
            with set (value: string) = ()
        [<Erase>]
        member this.value
            with set (value: string) = ()
        [<Erase>]
        member this.min
            with set (value: string) = ()
        [<Erase>]
        member this.max
            with set (value: string) = ()
        [<Erase>]
        member this.low
            with set (value: string) = ()
        [<Erase>]
        member this.high
            with set (value: string) = ()
        [<Erase>]
        member this.optimum
            with set (value: string) = ()

    [<Erase>]
    type details() =
        inherit RegularNode()
        [<Erase>]
        member this.open'
            with set (value: bool) = ()

    [<Erase>]
    type summary() =
        inherit RegularNode()

    [<Erase>]
    type dialog() =
        inherit RegularNode()
        [<Erase>]
        member this.open'
            with set (value: bool) = ()

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
            with set (value: bool) = ()

        [<Erase>]
        member this.form
            with set (value: string) = ()
        [<Erase>]
        member this.name
            with set (value: string) = ()

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
            with set (value: string) = ()
        [<Erase>]
        member this.colspan
            with set (value: int) = ()
        [<Erase>]
        member this.rowspan
            with set (value: int) = ()
        [<Erase>]
        member this.headers
            with set (value: string) = ()
        [<Erase>]
        member this.scope
            with set (value: string) = ()
    [<Erase>]
    type td() =
        inherit RegularNode()
        [<Erase>]
        member this.colspan
            with set (value: int) = ()
        [<Erase>]
        member this.rowspan
            with set (value: int) = ()
        [<Erase>]
        member this.headers
            with set (value: string) = ()

    [<Erase>]
    type map() =
        inherit RegularNode()
        [<Erase>]
        member this.name
            with set (value: string) = ()
    [<Erase>]
    type area() =
        inherit VoidNode()
        [<Erase>]
        member this.shape
            with set (value: string) = ()
        [<Erase>]
        member this.coords
            with set (value: string) = ()
        [<Erase>]
        member this.href
            with set (value: string) = ()
        [<Erase>]
        member this.alt
            with set (value: string) = ()
        [<Erase>]
        member this.download
            with set (value: string) = ()
        [<Erase>]
        member this.target
            with set (value: string) = ()
        [<Erase>]
        member this.rel
            with set (value: string) = ()
        [<Erase>]
        member this.referrerpolicy
            with set (value: string) = ()
        [<Erase>]
        member this.ping
            with set (value: string) = ()

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
            with set (value: int) = ()
    [<Erase>]
    type colgroup() =
        inherit RegularNode()
        [<Erase>]
        member this.span
            with set (value: int) = ()
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
            with set (value: string) = ()
        [<Erase>]
        member this.type'
            with set (value: string) = ()
        [<Erase>]
        member this.width
            with set (value: int) = ()
        [<Erase>]
        member this.height
            with set (value: int) = ()
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

    type SvgTag with
        [<Erase>]
        member _.id
            with set (value: string) = ()
        member _.class'
            with set (value: string) = ()
        member _.style
            with set (value: string) = ()
        member _.lang
            with set (value: string) = ()
        member _.tabindex
            with set (value: int) = ()
        member _.``xml:lang``
            with set (value: string) = ()
        member _.``xml:space``
            with set (value: string) = ()
        member inline this.xmlLang
            with inline set (value: string) = this.``xml:lang`` <- value
        member inline this.xmlSpace
            with inline set (value: string) = this.``xml:space`` <- value

    [<Erase>]
    module Svg =
        [<Erase>]
        type a() =
            inherit SvgNode()
            member _.download
                with set (value: string) = ()
            member _.href
                with set (value: string) = ()
            member _.hreflang
                with set (value: string) = ()
            member _.ping
                with set (value: string) = ()
            member _.referrerpolicy
                with set (value: string) = ()
            member _.rel
                with set (value: string) = ()
            member _.target
                with set (value: string) = ()
            member _.type'
                with set (value: string) = ()

        [<Erase>]
        type animate() =
            inherit SvgNode()
        [<Erase>]
        type animateMotion() =
            inherit SvgNode()
            member _.keyPoints
                with set (value: string) = ()
            member _.path
                with set (value: string) = ()
            member _.rotate
                with set (value: string) = ()

        [<Erase>]
        type animateTransform() =
            inherit SvgNode()
            member _.by
                with set (value: string) = ()
            member _.from
                with set (value: string) = ()
            member _.to'
                with set (value: string) = ()
            member _.type'
                with set (value: string) = ()

        [<Erase>]
        type circle() =
            inherit SvgNode()
            member _.cx
                with set (value: string) = ()
            member _.cy
                with set (value: string) = ()
            member _.r
                with set (value: string) = ()
            member _.pathLength
                with set (value: string) = ()
        [<Erase>]
        type clipPath() =
            inherit SvgNode()
            member _.clipPathUnits
                with set (value: string) = ()
        [<Erase>]
        type defs() =
            inherit SvgNode()
        [<Erase>]
        type desc() =
            inherit SvgNode()
        [<Erase>]
        type ellipse() =
            inherit SvgNode()
            member _.cx
                with set (value: string) = ()
            member _.cy
                with set (value: string) = ()
            member _.rx
                with set (value: string) = ()
            member _.ry
                with set (value: string) = ()
            member _.pathLength
                with set (value: string) = ()
        [<Erase>]
        type feBlend() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()
            member _.in2
                with set (value: string) = ()
            member _.mode
                with set (value: string) = ()

        type feColorMatrix() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()
            member _.type'
                with set (value: string) = ()
            member _.values
                with set (value: string) = ()
        [<Erase>]
        type feComponentTransfer() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()
        [<Erase>]
        type feComposite() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()
            member _.in2
                with set (value: string) = ()
            member _.operator
                with set (value: string) = ()
            member _.k1
                with set (value: string) = ()
            member _.k2
                with set (value: string) = ()
            member _.k3
                with set (value: string) = ()
            member _.k4
                with set (value: string) = ()
        [<Erase>]
        type feConvolveMatrix() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()
            member _.order
                with set (value: string) = ()
            member _.kernelMatrix
                with set (value: string) = ()
            member _.divisor
                with set (value: string) = ()
            member _.bias
                with set (value: string) = ()
            member _.targetX
                with set (value: string) = ()
            member _.targetY
                with set (value: string) = ()
            member _.edgeMode
                with set (value: string) = ()
            member _.kernelUnitLength
                with set (value: string) = ()
            member _.preserveAlpha
                with set (value: string) = ()
        [<Erase>]
        type feDiffuseLighting() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()
            member _.surfaceScale
                with set (value: string) = ()
            member _.diffuseConstant
                with set (value: string) = ()
            member _.kernelUnitLength
                with set (value: string) = ()
        [<Erase>]
        type feDisplacementMap() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()
            member _.in2
                with set (value: string) = ()
            member _.scale
                with set (value: string) = ()
            member _.xChannelSelector
                with set (value: string) = ()
            member _.yChannelSelector
                with set (value: string) = ()
        [<Erase>]
        type feDistantLight() =
            inherit SvgNode()
            member _.azimuth
                with set (value: string) = ()
            member _.elevation
                with set (value: string) = ()
        [<Erase>]
        type feDropShadow() =
            inherit SvgNode()
            member _.dx
                with set (value: string) = ()
            member _.dy
                with set (value: string) = ()
            member _.stdDeviation
                with set (value: string) = ()

        [<Erase>]
        type feFlood() =
            inherit SvgNode()
            member _.``flood-color``
                with set (value: string) = ()
            member _.``flood-opacity``
                with set (value: string) = ()
            member inline this.floodColor
                with inline set (value: string) = this.``flood-color`` <- value
            member inline this.floodOpacity
                with inline set (value: string) = this.``flood-opacity`` <- value
        [<Erase>]
        type feFuncA() =
            inherit SvgNode()
        [<Erase>]
        type feFuncB() =
            inherit SvgNode()
        [<Erase>]
        type feFuncG() =
            inherit SvgNode()
        [<Erase>]
        type feFuncR() =
            inherit SvgNode()
        [<Erase>]
        type feGaussianBlur() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()
            member _.stdDeviation
                with set (value: string) = ()
            member _.edgeMode
                with set (value: string) = ()

        [<Erase>]
        type feImage() =
            inherit SvgNode()
            member _.crossorigin
                with set (value: string) = ()
            member _.preserveAspectRatio
                with set (value: string) = ()
            member _.``xlink:href``
                with set (value: string) = ()
            member inline this.xlinkHref
                with inline set (value: string) = this.``xlink:href`` <- value
        [<Erase>]
        type feMerge() =
            inherit SvgNode()
        [<Erase>]
        type feMergeNode() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()
        [<Erase>]
        type feMorphology() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()
            member _.operator
                with set (value: string) = ()
            member _.radius
                with set (value: string) = ()

        [<Erase>]
        type feOffset() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()
            member _.dx
                with set (value: string) = ()
            member _.dy
                with set (value: string) = ()

        [<Erase>]
        type fePointLight() =
            inherit SvgNode()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()
            member _.z
                with set (value: string) = ()

        [<Erase>]
        type feSpecularLighting() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()
            member _.surfaceScale
                with set (value: string) = ()
            member _.specularConstant
                with set (value: string) = ()
            member _.specularExponent
                with set (value: string) = ()
            member _.kernelUnitLength
                with set (value: string) = ()
        [<Erase>]
        type feSpotLight() =
            inherit SvgNode()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()
            member _.z
                with set (value: string) = ()
            member _.pointsAtX
                with set (value: string) = ()
            member _.pointsAtY
                with set (value: string) = ()
            member _.pointsAtZ
                with set (value: string) = ()
            member _.specularExponent
                with set (value: string) = ()
            member _.limitingConeAngle
                with set (value: string) = ()
        [<Erase>]
        type feTile() =
            inherit SvgNode()
            member _.in'
                with set (value: string) = ()

        [<Erase>]
        type feTurbulence() =
            inherit SvgNode()
            member _.baseFrequency
                with set (value: string) = ()
            member _.numOctaves
                with set (value: string) = ()
            member _.seed
                with set (value: string) = ()
            member _.stitchTiles
                with set (value: string) = ()
            member _.type'
                with set (value: string) = ()
        [<Erase>]
        type filter() =
            inherit SvgNode()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()
            member _.width
                with set (value: string) = ()
            member _.height
                with set (value: string) = ()
            member _.filterUnits
                with set (value: string) = ()
            member _.primitiveUnits
                with set (value: string) = ()
            member _.``xlink:href``
                with set (value: string) = ()
            member inline this.xlinkHref
                with inline set (value: string) = this.``xlink:href`` <- value
        [<Erase>]
        type foreignObject() =
            inherit SvgNode()
            member _.height
                with set (value: string) = ()
            member _.width
                with set (value: string) = ()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()
        [<Erase>]
        type g() =
            inherit SvgNode()
        [<Erase>]
        type image() =
            inherit SvgNode()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()
            member _.width
                with set (value: string) = ()
            member _.height
                with set (value: string) = ()
            member _.href
                with set (value: string) = ()
            member _.preserveAspectRatio
                with set (value: string) = ()
            member _.crossorigin
                with set (value: string) = ()
            member _.decoding
                with set (value: string) = ()

        [<Erase>]
        type line() =
            inherit SvgNode()
            member _.x1
                with set (value: string) = ()
            member _.x2
                with set (value: string) = ()
            member _.y1
                with set (value: string) = ()
            member _.y2
                with set (value: string) = ()
            member _.pathLength
                with set (value: string) = ()

        [<Erase>]
        type linearGradient() =
            inherit SvgNode()
            member _.gradientUnits
                with set (value: string) = ()
            member _.gradientTransform
                with set (value: string) = ()
            member _.href
                with set (value: string) = ()
            member _.spreadMethod
                with set (value: string) = ()
            member _.x1
                with set (value: string) = ()
            member _.x2
                with set (value: string) = ()
            member _.y1
                with set (value: string) = ()
            member _.y2
                with set (value: string) = ()
        [<Erase>]
        type marker() =
            inherit SvgNode()
            member _.markerHeight
                with set (value: string) = ()
            member _.markerUnits
                with set (value: string) = ()
            member _.markerWidth
                with set (value: string) = ()
            member _.orient
                with set (value: string) = ()
            member _.preserveAspectRatio
                with set (value: string) = ()
            member _.refX
                with set (value: string) = ()
            member _.refY
                with set (value: string) = ()
            member _.viewBox
                with set (value: string) = ()
        [<Erase>]
        type mask() =
            inherit SvgNode()
            member _.height
                with set (value: string) = ()
            member _.maskContentUnits
                with set (value: string) = ()
            member _.maskUnits
                with set (value: string) = ()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()
            member _.width
                with set (value: string) = ()

        [<Erase>]
        type metadata() =
            inherit SvgNode()
        [<Erase>]
        type mpath() =
            inherit SvgNode()
        [<Erase>]
        type path() =
            inherit SvgNode()
            member _.d
                with set (value: string) = ()
            member _.pathLength
                with set (value: string) = ()
        [<Erase>]
        type pattern() =
            inherit SvgNode()
            member _.height
                with set (value: string) = ()
            member _.href
                with set (value: string) = ()
            member _.patternContentUnits
                with set (value: string) = ()
            member _.patternTransform
                with set (value: string) = ()
            member _.patternUnits
                with set (value: string) = ()
            member _.preserveAspectRatio
                with set (value: string) = ()
            member _.viewBox
                with set (value: string) = ()
            member _.width
                with set (value: string) = ()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()

        [<Erase>]
        type polygon() =
            inherit SvgNode()
            member _.points
                with set (value: string) = ()
            member _.pathLength
                with set (value: string) = ()
        [<Erase>]
        type polyline() =
            inherit SvgNode()
            member _.points
                with set (value: string) = ()
            member _.pathLength
                with set (value: string) = ()
        [<Erase>]
        type radialGradient() =
            inherit SvgNode()
            member _.cx
                with set (value: string) = ()
            member _.cy
                with set (value: string) = ()
            member _.fr
                with set (value: string) = ()
            member _.fx
                with set (value: string) = ()
            member _.fy
                with set (value: string) = ()
            member _.gradientUnits
                with set (value: string) = ()
            member _.gradientTransform
                with set (value: string) = ()
            member _.href
                with set (value: string) = ()
            member _.r
                with set (value: string) = ()
            member _.spreadMethod
                with set (value: string) = ()
        [<Erase>]
        type rect() =
            inherit SvgNode()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()
            member _.width
                with set (value: string) = ()
            member _.height
                with set (value: string) = ()
            member _.rx
                with set (value: string) = ()
            member _.ry
                with set (value: string) = ()
            member _.pathLength
                with set (value: string) = ()

        [<Erase>]
        type script() =
            inherit SvgNode()
            member _.crossorigin
                with set (value: string) = ()
            member _.href
                with set (value: string) = ()
            member _.type'
                with set (value: string) = ()
        [<Erase>]
        type set() =
            inherit SvgNode()
            member _.to'
                with set (value: string) = ()
        [<Erase>]
        type stop() =
            inherit SvgNode()
            member _.offset
                with set (value: string) = ()
            member _.``stop-color``
                with set (value: string) = ()
            member _.``stop-opacity``
                with set (value: string) = ()
            member inline this.stopColor
                with inline set (value: string) = this.``stop-color`` <- value
            member inline this.stopOpacity
                with inline set (value: string) = this.``stop-opacity`` <- value
        [<Erase>]
        type style() =
            inherit SvgNode()
            member _.type'
                with set (value: string) = ()
            member _.media
                with set (value: string) = ()
            member _.title
                with set (value: string) = ()

        [<Erase>]
        type svg() =
            inherit SvgNode()
            member _.baseProfile
                with set (value: string) = ()
            member _.height
                with set (value: string) = ()
            member _.preserveAspectRatio
                with set (value: string) = ()
            member _.viewBox
                with set (value: string) = ()
            member _.width
                with set (value: string) = ()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()
        [<Erase>]
        type switch() =
            inherit SvgNode()
        [<Erase>]
        type symbol() =
            inherit SvgNode()
            member _.height
                with set (value: string) = ()
            member _.preserveAspectRatio
                with set (value: string) = ()
            member _.refX
                with set (value: string) = ()
            member _.refY
                with set (value: string) = ()
            member _.viewBox
                with set (value: string) = ()
            member _.width
                with set (value: string) = ()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()
        [<Erase>]
        type text() =
            inherit SvgNode()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()
            member _.dx
                with set (value: string) = ()
            member _.dy
                with set (value: string) = ()
            member _.rotate
                with set (value: string) = ()
            member _.lengthAdjust
                with set (value: string) = ()
            member _.textLength
                with set (value: string) = ()
        [<Erase>]
        type textPath() =
            inherit SvgNode()
            member _.href
                with set (value: string) = ()
            member _.lengthAdjust
                with set (value: string) = ()
            member _.method
                with set (value: string) = ()
            member _.path
                with set (value: string) = ()
            member _.side
                with set (value: string) = ()
            member _.spacing
                with set (value: string) = ()
            member _.startOffset
                with set (value: string) = ()
            member _.textLength
                with set (value: string) = ()
        [<Erase>]
        type title() =
            inherit SvgNode()
        [<Erase>]
        type tspan() =
            inherit SvgNode()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()
            member _.dx
                with set (value: string) = ()
            member _.dy
                with set (value: string) = ()
            member _.rotate
                with set (value: string) = ()
            member _.lengthAdjust
                with set (value: string) = ()
            member _.textLength
                with set (value: string) = ()
        [<Erase>]
        type use'() =
            inherit SvgNode()
            member _.href
                with set (value: string) = ()
            member _.x
                with set (value: string) = ()
            member _.y
                with set (value: string) = ()
            member _.width
                with set (value: string) = ()
            member _.height
                with set (value: string) = ()
        [<Erase>]
        type view() =
            inherit SvgNode()
            member _.preserveAspectRatio
                with set (value: string) = ()
            member _.viewBox
                with set (value: string) = ()
