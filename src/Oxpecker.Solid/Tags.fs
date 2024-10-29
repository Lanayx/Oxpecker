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
