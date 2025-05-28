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


    [<AllowNullLiteral>]
    [<Interface>]
    type CoreSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type ExternalResourceSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type ConditionalProcessingSVGAttributes = interface end
    [<AllowNullLiteral>]
    [<Interface>]
    type AnimationElementSVGAttributes =
        inherit CoreSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit ConditionalProcessingSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type ShapeElementSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type ContainerElementSVGAttributes =
        inherit CoreSVGAttributes
        inherit ShapeElementSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FilterPrimitiveElementSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type StylableSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type TransformableSVGAttributes = interface end



    [<AllowNullLiteral>]
    [<Interface>]
    type AnimationTimingSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type AnimationValueSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type AnimationAdditionSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type AnimationAttributeTargetSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type PresentationSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type SingleInputFilterSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type DoubleInputFilterSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type FitToViewBoxSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type GradientElementSVGAttributes =
        inherit CoreSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type GraphicsElementSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type LightSourceElementSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type NewViewportSVGAttributes =
        inherit CoreSVGAttributes



    [<AllowNullLiteral>]
    [<Interface>]
    type TextContentElementSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type ZoomAndPanSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type AnimateSVGAttributes =
        inherit AnimationElementSVGAttributes
        inherit AnimationAttributeTargetSVGAttributes
        inherit AnimationTimingSVGAttributes
        inherit AnimationValueSVGAttributes
        inherit AnimationAdditionSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type AnimateMotionSVGAttributes =
        inherit AnimationElementSVGAttributes
        inherit AnimationTimingSVGAttributes
        inherit AnimationValueSVGAttributes
        inherit AnimationAdditionSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type AnimateTransformSVGAttributes =
        inherit AnimationElementSVGAttributes
        inherit AnimationAttributeTargetSVGAttributes
        inherit AnimationTimingSVGAttributes
        inherit AnimationValueSVGAttributes
        inherit AnimationAdditionSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type CircleSVGAttributes =
        inherit GraphicsElementSVGAttributes
        inherit ShapeElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type ClipPathSVGAttributes =
        inherit CoreSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type DefsSVGAttributes =
        inherit ContainerElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type DescSVGAttributes =
        inherit CoreSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type EllipseSVGAttributes =
        inherit GraphicsElementSVGAttributes
        inherit ShapeElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeBlendSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit DoubleInputFilterSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeColorMatrixSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit SingleInputFilterSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeComponentTransferSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit SingleInputFilterSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeCompositeSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit DoubleInputFilterSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeConvolveMatrixSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit SingleInputFilterSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeDiffuseLightingSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit SingleInputFilterSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeDisplacementMapSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit DoubleInputFilterSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeDistantLightSVGAttributes =
        inherit LightSourceElementSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeDropShadowSVGAttributes =
        inherit CoreSVGAttributes
        inherit FilterPrimitiveElementSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeFloodSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeFuncSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeGaussianBlurSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit SingleInputFilterSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeImageSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeMergeSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeMergeNodeSVGAttributes =
        inherit CoreSVGAttributes
        inherit SingleInputFilterSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeMorphologySVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit SingleInputFilterSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeOffsetSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit SingleInputFilterSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FePointLightSVGAttributes =
        inherit LightSourceElementSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeSpecularLightingSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit SingleInputFilterSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeSpotLightSVGAttributes =
        inherit LightSourceElementSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeTileSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit SingleInputFilterSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FeTurbulanceSVGAttributes =
        inherit FilterPrimitiveElementSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FilterSVGAttributes =
        inherit CoreSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type ForeignObjectSVGAttributes =
        inherit NewViewportSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type GSVGAttributes =
        inherit ContainerElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type ImageSVGAttributes =
        inherit NewViewportSVGAttributes
        inherit GraphicsElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type LineSVGAttributes =
        inherit GraphicsElementSVGAttributes
        inherit ShapeElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type LinearGradientSVGAttributes =
        inherit GradientElementSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type MarkerSVGAttributes =
        inherit ContainerElementSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit FitToViewBoxSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type MaskSVGAttributes =
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type MetadataSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type MPathSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type PathSVGAttributes =
        inherit GraphicsElementSVGAttributes
        inherit ShapeElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type PatternSVGAttributes =
        inherit ContainerElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit FitToViewBoxSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type PolygonSVGAttributes =
        inherit GraphicsElementSVGAttributes
        inherit ShapeElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type PolylineSVGAttributes =
        inherit GraphicsElementSVGAttributes
        inherit ShapeElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type RadialGradientSVGAttributes =
        inherit GradientElementSVGAttributes
    [<AllowNullLiteral>]
    [<Interface>]
    type RectSVGAttributes =
        inherit GraphicsElementSVGAttributes
        inherit ShapeElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type SetSVGAttributes =
        inherit CoreSVGAttributes
        inherit StylableSVGAttributes
        inherit AnimationTimingSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type StopSVGAttributes =
        inherit CoreSVGAttributes
        inherit StylableSVGAttributes
    [<AllowNullLiteral>]
    [<Interface>]
    type SvgSVGAttributes =
        inherit ContainerElementSVGAttributes
        inherit NewViewportSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit FitToViewBoxSVGAttributes
        inherit ZoomAndPanSVGAttributes
        inherit PresentationSVGAttributes
    [<AllowNullLiteral>]
    [<Interface>]
    type SwitchSVGAttributes =
        inherit ContainerElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes
    [<AllowNullLiteral>]
    [<Interface>]
    type SymbolSVGAttributes =
        inherit ContainerElementSVGAttributes
        inherit NewViewportSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit FitToViewBoxSVGAttributes
    [<AllowNullLiteral>]
    [<Interface>]
    type TextSVGAttributes =
        inherit TextContentElementSVGAttributes
        inherit GraphicsElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
        inherit TransformableSVGAttributes
    [<AllowNullLiteral>]
    [<Interface>]
    type TextPathSVGAttributes =
        inherit TextContentElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
    [<AllowNullLiteral>]
    [<Interface>]
    type TSpanSVGAttributes =
        inherit TextContentElementSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes
    [<AllowNullLiteral>]
    [<Interface>]
    type UseSVGAttributes =
        inherit CoreSVGAttributes
        inherit StylableSVGAttributes
        inherit ConditionalProcessingSVGAttributes
        inherit GraphicsElementSVGAttributes
        inherit PresentationSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit TransformableSVGAttributes
    [<AllowNullLiteral>]
    [<Interface>]
    type ViewSVGAttributes =
        inherit CoreSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit FitToViewBoxSVGAttributes
        inherit ZoomAndPanSVGAttributes
    open Fable.Core.JsInterop
    type CoreSVGAttributes with
        [<Erase>]
        member _.id
            with set(_: string) = ()
        [<Erase>]
        member _.lang
            with set(_: string) = ()
        [<Erase>]
        member _.tabIndex
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.tabindex
            with set(_: U2<float, string>) = ()
    type StylableSVGAttributes with
        [<Erase>]
        member _.class'
            with set(_: string) = ()
        [<Erase>]
        member _.style
            with set(_: string) = ()
    type TransformableSVGAttributes with
        [<Erase>]
        member _.transform
            with set(_: string) = ()
    type ConditionalProcessingSVGAttributes with
        [<Erase>]
        member _.requiredExtensions
            with set(_: string) = ()
        [<Erase>]
        member _.requiredFeatures
            with set(_: string) = ()
        [<Erase>]
        member _.systemLanguage
            with set(_: string) = ()
    type ExternalResourceSVGAttributes with
        [<Erase>]
        member _.externalResourcesRequired
            with set(_: string) = ()
    type AnimationTimingSVGAttributes with
        [<Erase>]
        member _.begin'
            with set(_: string) = ()
        [<Erase>]
        member _.dur
            with set(_: string) = ()
        [<Erase>]
        member _.end'
            with set(_: string) = ()
        [<Erase>]
        member _.min
            with set(_: string) = ()
        [<Erase>]
        member _.max
            with set(_: string) = ()
        [<Erase>]
        member _.restart
            with set(_: string) = ()
        [<Erase>]
        member _.repeatCount
            with set(_: string) = ()
        [<Erase>]
        member _.repeatDur
            with set(_: string) = ()
        [<Erase>]
        member _.fill
            with set(_: string) = ()
    type AnimationValueSVGAttributes with
        [<Erase>]
        member _.calcMode
            with set(_: string) = ()
        [<Erase>]
        member _.values
            with set(_: string) = ()
        [<Erase>]
        member _.keyTimes
            with set(_: string) = ()
        [<Erase>]
        member _.keySplines
            with set(_: string) = ()
        [<Erase>]
        member _.from
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.to'
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.by
            with set(_: U2<float, string>) = ()
    type AnimationAdditionSVGAttributes with
        [<Erase>]
        member _.attributeName
            with set(_: string) = ()
        [<Erase>]
        member _.additive
            with set(_: string) = ()
        [<Erase>]
        member _.accumulate
            with set(_: string) = ()
    type AnimationAttributeTargetSVGAttributes with
        [<Erase>]
        member _.attributeName
            with set(_: string) = ()
        [<Erase>]
        member _.attributeType
            with set(_: string) = ()

    type ContainerElementSVGAttributes with
        [<Erase>]
        member _.``color-rendering``
            with set(_: string) = ()
        member this.colorRendering
            with inline set(value: string) = this.``color-rendering`` <- value
        [<Erase>]
        member _.``clip-path``
            with set(_: string) = ()
        member this.clipPath
            with inline set(value: string) = this.``clip-path`` <- value
        [<Erase>]
        member _.cursor
            with set(_: string) = ()
        [<Erase>]
        member _.``color-interpolation``
            with set(_: string) = ()
        member this.colorInterpolation
            with inline set(value: string) = this.``color-interpolation`` <- value
        [<Erase>]
        member _.``enable-background``
            with set(_: string) = ()
        member this.enableBackground
            with inline set(value: string) = this.``enable-background`` <- value
        [<Erase>]
        member _.filter
            with set(_: string) = ()
        [<Erase>]
        member _.mask
            with set(_: string) = ()
        [<Erase>]
        member _.opacity
            with set(_: string) = ()

    type GraphicsElementSVGAttributes with
        [<Erase>]
        member _.``clip-rule``
            with set(_: string) = ()
        member this.clipRule
            with inline set(value: string) = this.``clip-rule`` <- value
        [<Erase>]
        member _.mask
            with set(_: string) = ()
        [<Erase>]
        member _.``pointer-events``
            with set(_: string) = ()
        member this.pointerEvents
            with inline set(value: string) = this.``pointer-events`` <- value
        [<Erase>]
        member _.cursor
            with set(_: string) = ()
        [<Erase>]
        member _.opacity
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.filter
            with set(_: string) = ()
        [<Erase>]
        member _.display
            with set(_: string) = ()
        [<Erase>]
        member _.visibility
            with set(_: string) = ()
        [<Erase>]
        member _.``color-interpolation``
            with set(_: string) = ()
        member this.colorInterpolation
            with inline set(value: string) = this.``color-interpolation`` <- value
        [<Erase>]
        member _.``color-rendering``
            with set(_: string) = ()
        member this.colorRendering
            with inline set(value: string) = this.``color-rendering`` <- value

        [<Erase>]
        member _.stroke
            with set(_: string) = ()
        [<Erase>]
        member _.``stroke-dasharray``
            with set(_: string) = ()
        member this.strokeDasharray
            with inline set(value: string) = this.``stroke-dasharray`` <- value
        [<Erase>]
        member _.``stroke-dashoffset``
            with set(_: U2<float, string>) = ()
        member this.strokeDashoffset
            with inline set(value: string) = this.``stroke-dashoffset`` <- !^value
        [<Erase>]
        member _.``stroke-linecap``
            with set(_: string) = ()
        member this.strokeLinecap
            with inline set(value: string) = this.``stroke-linecap`` <- value
        [<Erase>]
        member _.``stroke-linejoin``
            with set(_: string) = ()
        member this.strokeLinejoin
            with inline set(value: string) = this.``stroke-linejoin`` <- value
        [<Erase>]
        member _.``stroke-miterlimit``
            with set(_: string) = ()
        member this.strokeMiterlimit
            with inline set(value: string) = this.``stroke-miterlimit`` <- value
        [<Erase>]
        member _.``stroke-opacity``
            with set(_: string) = ()
        member this.strokeOpacity
            with inline set(value: string) = this.``stroke-opacity`` <- value
        [<Erase>]
        member _.``stroke-width``
            with set(_: U2<float, string>) = ()
        member this.strokeWidth
            with inline set(value: string) = this.``stroke-width`` <- !^value
        [<Erase>]
        member _.color
            with set(_: string) = ()
        [<Erase>]
        member _.fill
            with set(_: string) = ()
        [<Erase>]
        member _.``fill-opacity``
            with set(_: string) = ()
        member this.fillOpacity
            with inline set(value: string) = this.``fill-opacity`` <- value
        [<Erase>]
        member _.``fill-rule``
            with set(_: string) = ()
        member this.fillRule
            with inline set(value: string) = this.``fill-rule`` <- value
        [<Erase>]
        member _.``shape-rendering``
            with set(_: string) = ()
        member this.shapeRendering
            with inline set(value: string) = this.``shape-rendering`` <- value
        [<Erase>]
        member _.pathLength
            with set(_: U2<string, float>) = ()

    type TextContentElementSVGAttributes with
        [<Erase>]
        member _.``font-family``
            with set(_: string) = ()
        member this.fontFamily
            with inline set(value: string) = this.``font-family`` <- value
        [<Erase>]
        member _.``font-size``
            with set(_: string) = ()
        member this.fontSize
            with inline set(value: string) = this.``font-size`` <- value
        [<Erase>]
        member _.``font-size-adjust``
            with set(_: U2<float, string>) = ()
        member this.fontSizeAdjust
            with inline set(value: string) = this.``font-size-adjust`` <- !^value
        [<Erase>]
        member _.``font-stretch``
            with set(_: string) = ()
        member this.fontStretch
            with inline set(value: string) = this.``font-stretch`` <- value
        [<Erase>]
        member _.``font-style``
            with set(_: string) = ()
        member this.fontStyle
            with inline set(value: string) = this.``font-style`` <- value
        [<Erase>]
        member _.``font-variant``
            with set(_: string) = ()
        member this.fontVariant
            with inline set(value: string) = this.``font-variant`` <- value
        [<Erase>]
        member _.``font-weight``
            with set(_: U2<float, string>) = ()
        member this.fontWeight
            with inline set(value: string) = this.``font-weight`` <- !^value
        [<Erase>]
        member _.kerning
            with set(_: string) = ()
        [<Erase>]
        member _.``letter-spacing``
            with set(_: U2<float, string>) = ()
        member this.letterSpacing
            with inline set(value: string) = this.``letter-spacing`` <- !^value
        [<Erase>]
        member _.``word-spacing``
            with set(_: U2<float, string>) = ()
        member this.wordSpacing
            with inline set(value: string) = this.``word-spacing`` <- !^value
        [<Erase>]
        member _.``text-decoration``
            with set(_: string) = ()
        member this.textDecoration
            with inline set(value: string) = this.``text-decoration`` <- value
        [<Erase>]
        member _.``glyph-orientation-horizontal``
            with set(_: string) = ()
        member this.glyphOrientationHorizontal
            with inline set(value: string) = this.``glyph-orientation-horizontal`` <- value
        [<Erase>]
        member _.``glyph-orientation-vertical``
            with set(_: string) = ()
        member this.glyphOrientationVertical
            with inline set(value: string) = this.``glyph-orientation-vertical`` <- value
        [<Erase>]
        member _.direction
            with set(_: string) = ()
        [<Erase>]
        member _.``unicode-bidi``
            with set(_: string) = ()
        member this.unicodeBidi
            with inline set(value: string) = this.``unicode-bidi`` <- value
        [<Erase>]
        member _.``text-anchor``
            with set(_: string) = ()
        member this.textAnchor
            with inline set(value: string) = this.``text-anchor`` <- value
        [<Erase>]
        member _.``dominant-baseline``
            with set(_: string) = ()
        member this.dominantBaseline
            with inline set(value: string) = this.``dominant-baseline`` <- value
        [<Erase>]
        member _.color
            with set(_: string) = ()
        [<Erase>]
        member _.fill
            with set(_: string) = ()
        [<Erase>]
        member _.``fill-opacity``
            with set(_: string) = ()
        member this.fillOpacity
            with inline set(value: string) = this.``fill-opacity`` <- value
        [<Erase>]
        member _.``fill-rule``
            with set(_: string) = ()
        member this.fillRule
            with inline set(value: string) = this.``fill-rule`` <- value
        [<Erase>]
        member _.stroke
            with set(_: string) = ()
        [<Erase>]
        member _.``stroke-dasharray``
            with set(_: string) = ()
        member this.strokeDasharray
            with inline set(value: string) = this.``stroke-dasharray`` <- value
        [<Erase>]
        member _.``stroke-dashoffset``
            with set(_: U2<float, string>) = ()
        member this.strokeDashoffset
            with inline set(value: string) = this.``stroke-dashoffset`` <- !^value
        [<Erase>]
        member _.``stroke-linecap``
            with set(_: string) = ()
        member this.strokeLinecap
            with inline set(value: string) = this.``stroke-linecap`` <- value
        [<Erase>]
        member _.``stroke-linejoin``
            with set(_: string) = ()
        member this.strokeLinejoin
            with inline set(value: string) = this.``stroke-linejoin`` <- value
        [<Erase>]
        member _.``stroke-miterlimit``
            with set(_: string) = ()
        member this.strokeMiterlimit
            with inline set(value: string) = this.``stroke-miterlimit`` <- value
        [<Erase>]
        member _.``stroke-opacity``
            with set(_: string) = ()
        member this.strokeOpacity
            with inline set(value: string) = this.``stroke-opacity`` <- value
        [<Erase>]
        member _.``stroke-width``
            with set(_: U2<float, string>) = ()
        member this.strokeWidth
            with inline set(value: string) = this.``stroke-width`` <- !^value

    type AnimateSVGAttributes with
        [<Erase>]
        member _.``color-interpolation``
            with set(_: string) = ()
        member this.colorInterpolation
            with inline set(value: string) = this.``color-interpolation`` <- value
        [<Erase>]
        member _.``color-rendering``
            with set(_: string) = ()
        member this.colorRendering
            with inline set(value: string) = this.``color-rendering`` <- value

    type SwitchSVGAttributes with
        [<Erase>]
        member _.display
            with set(_: string) = ()
        [<Erase>]
        member _.visibility
            with set(_: string) = ()


    type PresentationSVGAttributes with
        [<Erase>]
        member _.``alignment-baseline``
            with set(_: string) = ()
        member this.alignmentBaseline
            with inline set(value: string) = this.``alignment-baseline`` <- value
        [<Erase>]
        member _.``baseline-shift``
            with set(_: U2<float, string>) = ()
        member this.baselineShift
            with inline set(value: string) = this.``baseline-shift`` <- !^value
        [<Erase>]
        member _.clip
            with set(_: string) = ()
        [<Erase>]
        member _.``clip-path``
            with set(_: string) = ()
        member this.clipPath
            with inline set(value: string) = this.``clip-path`` <- value
        [<Erase>]
        member _.``clip-rule``
            with set(_: string) = ()
        member this.clipRule
            with inline set(value: string) = this.``clip-rule`` <- value
        [<Erase>]
        member _.color
            with set(_: string) = ()
        [<Erase>]
        member _.``color-interpolation``
            with set(_: string) = ()
        member this.colorInterpolation
            with inline set(value: string) = this.``color-interpolation`` <- value
        [<Erase>]
        member _.``color-interpolation-filters``
            with set(_: string) = ()
        member this.colorInterpolationFilters
            with inline set(value: string) = this.``color-interpolation-filters`` <- value
        [<Erase>]
        member _.``color-profile``
            with set(_: string) = ()
        member this.colorProfile
            with inline set(value: string) = this.``color-profile`` <- value
        [<Erase>]
        member _.``color-rendering``
            with set(_: string) = ()
        member this.colorRendering
            with inline set(value: string) = this.``color-rendering`` <- value
        [<Erase>]
        member _.cursor
            with set(_: string) = ()
        [<Erase>]
        member _.direction
            with set(_: string) = ()
        [<Erase>]
        member _.display
            with set(_: string) = ()
        [<Erase>]
        member _.``dominant-baseline``
            with set(_: string) = ()
        member this.dominantBaseline
            with inline set(value: string) = this.``dominant-baseline`` <- value
        [<Erase>]
        member _.``enable-background``
            with set(_: string) = ()
        member this.enableBackground
            with inline set(value: string) = this.``enable-background`` <- value
        [<Erase>]
        member _.fill
            with set(_: string) = ()
        [<Erase>]
        member _.``fill-opacity``
            with set(_: string) = ()
        member this.fillOpacity
            with inline set(value: string) = this.``fill-opacity`` <- value
        [<Erase>]
        member _.``fill-rule``
            with set(_: string) = ()
        member this.fillRule
            with inline set(value: string) = this.``fill-rule`` <- value
        [<Erase>]
        member _.filter
            with set(_: string) = ()
        [<Erase>]
        member _.``flood-color``
            with set(_: string) = ()
        member this.floodColor
            with inline set(value: string) = this.``flood-color`` <- value
        [<Erase>]
        member _.``flood-opacity``
            with set(_: string) = ()
        member this.floodOpacity
            with inline set(value: string) = this.``flood-opacity`` <- value
        [<Erase>]
        member _.``font-family``
            with set(_: string) = ()
        member this.fontFamily
            with inline set(value: string) = this.``font-family`` <- value
        [<Erase>]
        member _.``font-size``
            with set(_: string) = ()
        member this.fontSize
            with inline set(value: string) = this.``font-size`` <- value
        [<Erase>]
        member _.``font-size-adjust``
            with set(_: U2<float, string>) = ()
        member this.fontSizeAdjust
            with inline set(value: string) = this.``font-size-adjust`` <- !^value
        [<Erase>]
        member _.``font-stretch``
            with set(_: string) = ()
        member this.fontStretch
            with inline set(value: string) = this.``font-stretch`` <- value
        [<Erase>]
        member _.``font-style``
            with set(_: string) = ()
        member this.fontStyle
            with inline set(value: string) = this.``font-style`` <- value
        [<Erase>]
        member _.``font-variant``
            with set(_: string) = ()
        member this.fontVariant
            with inline set(value: string) = this.``font-variant`` <- value
        [<Erase>]
        member _.``font-weight``
            with set(_: U2<float, string>) = ()
        member this.fontWeight
            with inline set(value: string) = this.``font-weight`` <- !^value
        [<Erase>]
        member _.``glyph-orientation-horizontal``
            with set(_: string) = ()
        member this.glyphOrientationHorizontal
            with inline set(value: string) = this.``glyph-orientation-horizontal`` <- value
        [<Erase>]
        member _.``glyph-orientation-vertical``
            with set(_: string) = ()
        member this.glyphOrientationVertical
            with inline set(value: string) = this.``glyph-orientation-vertical`` <- value
        [<Erase>]
        member _.``image-rendering``
            with set(_: string) = ()
        member this.imageRendering
            with inline set(value: string) = this.``image-rendering`` <- value
        [<Erase>]
        member _.kerning
            with set(_: string) = ()
        [<Erase>]
        member _.``letter-spacing``
            with set(_: U2<float, string>) = ()
        member this.letterSpacing
            with inline set(value: string) = this.``letter-spacing`` <- !^value
        [<Erase>]
        member _.``lighting-color``
            with set(_: string) = ()
        member this.lightingColor
            with inline set(value: string) = this.``lighting-color`` <- value
        [<Erase>]
        member _.``marker-end``
            with set(_: string) = ()
        member this.markerEnd
            with inline set(value: string) = this.``marker-end`` <- value
        [<Erase>]
        member _.``marker-mid``
            with set(_: string) = ()
        member this.markerMid
            with inline set(value: string) = this.``marker-mid`` <- value
        [<Erase>]
        member _.``marker-start``
            with set(_: string) = ()
        member this.markerStart
            with inline set(value: string) = this.``marker-start`` <- value
        [<Erase>]
        member _.mask
            with set(_: string) = ()
        [<Erase>]
        member _.opacity
            with set(_: string) = ()
        [<Erase>]
        member _.overflow
            with set(_: string) = ()
        [<Erase>]
        member _.pathLength
            with set(_: U2<string, float>) = ()
        [<Erase>]
        member _.``pointer-events``
            with set(_: string) = ()
        member this.pointerEvents
            with inline set(value: string) = this.``pointer-events`` <- value
        [<Erase>]
        member _.``shape-rendering``
            with set(_: string) = ()
        member this.shapeRendering
            with inline set(value: string) = this.``shape-rendering`` <- value
        [<Erase>]
        member _.``stop-color``
            with set(_: string) = ()
        member this.stopColor
            with inline set(value: string) = this.``stop-color`` <- value
        [<Erase>]
        member _.``stop-opacity``
            with set(_: string) = ()
        member this.stopOpacity
            with inline set(value: string) = this.``stop-opacity`` <- value
        [<Erase>]
        member _.stroke
            with set(_: string) = ()
        [<Erase>]
        member _.``stroke-dasharray``
            with set(_: string) = ()
        member this.strokeDasharray
            with inline set(value: string) = this.``stroke-dasharray`` <- value
        [<Erase>]
        member _.``stroke-dashoffset``
            with set(_: U2<float, string>) = ()
        member this.strokeDashoffset
            with inline set(value: string) = this.``stroke-dashoffset`` <- !^value
        [<Erase>]
        member _.``stroke-linecap``
            with set(_: string) = ()
        member this.strokeLinecap
            with inline set(value: string) = this.``stroke-linecap`` <- value
        [<Erase>]
        member _.``stroke-linejoin``
            with set(_: string) = ()
        member this.strokeLinejoin
            with inline set(value: string) = this.``stroke-linejoin`` <- value
        [<Erase>]
        member _.``stroke-miterlimit``
            with set(_: string) = ()
        member this.strokeMiterlimit
            with inline set(value: string) = this.``stroke-miterlimit`` <- value
        [<Erase>]
        member _.``stroke-opacity``
            with set(_: string) = ()
        member this.strokeOpacity
            with inline set(value: string) = this.``stroke-opacity`` <- value
        [<Erase>]
        member _.``stroke-width``
            with set(_: U2<float, string>) = ()
        member this.strokeWidth
            with inline set(value: string) = this.``stroke-width`` <- !^value
        [<Erase>]
        member _.``text-anchor``
            with set(_: string) = ()
        member this.textAnchor
            with inline set(value: string) = this.``text-anchor`` <- value
        [<Erase>]
        member _.``text-decoration``
            with set(_: string) = ()
        member this.textDecoration
            with inline set(value: string) = this.``text-decoration`` <- value
        [<Erase>]
        member _.``text-rendering``
            with set(_: string) = ()
        member this.textRendering
            with inline set(value: string) = this.``text-rendering`` <- value
        [<Erase>]
        member _.``unicode-bidi``
            with set(_: string) = ()
        member this.unicodeBidi
            with inline set(value: string) = this.``unicode-bidi`` <- value
        [<Erase>]
        member _.visibility
            with set(_: string) = ()
        [<Erase>]
        member _.``word-spacing``
            with set(_: U2<float, string>) = ()
        member this.wordSpacing
            with inline set(value: string) = this.``word-spacing`` <- !^value
        [<Erase>]
        member _.``writing-mode``
            with set(_: string) = ()
        member this.writingMode
            with inline set(value: string) = this.``writing-mode`` <- value


    type FilterPrimitiveElementSVGAttributes with
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.width
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.height
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.result
            with set(_: string) = ()
        [<Erase>]
        member _.``color-interpolation-filters``
            with set(_: string) = ()
        member this.colorInterpolationFilters
            with inline set(value: string) = this.``color-interpolation-filters`` <- value

    type SingleInputFilterSVGAttributes with
        [<Erase>]
        member _.in'
            with set(_: string) = ()
    type DoubleInputFilterSVGAttributes with
        [<Erase>]
        member _.in'
            with set(_: string) = ()
        [<Erase>]
        member _.in2
            with set(_: string) = ()
    type FitToViewBoxSVGAttributes with
        [<Erase>]
        member _.viewBox
            with set(_: string) = ()
        [<Erase>]
        member _.preserveAspectRatio
            with set(_: string) = ()
    type GradientElementSVGAttributes with
        [<Erase>]
        member _.gradientUnits
            with set(_: string) = ()
        [<Erase>]
        member _.gradientTransform
            with set(_: string) = ()
        [<Erase>]
        member _.spreadMethod
            with set(_: string) = ()
        [<Erase>]
        member _.href
            with set(_: string) = ()
    type NewViewportSVGAttributes with
        [<Erase>]
        member _.viewBox
            with set(_: string) = ()
        [<Erase>]
        member _.overflow
            with set(_: string) = ()
        [<Erase>]
        member _.clip
            with set(_: string) = ()
    type ZoomAndPanSVGAttributes with
        [<Erase>]
        member _.zoomAndPan
            with set(_: string) = ()
    type AnimateMotionSVGAttributes with
        [<Erase>]
        member _.path
            with set(_: string) = ()
        [<Erase>]
        member _.keyPoints
            with set(_: string) = ()
        [<Erase>]
        member _.rotate
            with set(_: string) = ()
        [<Erase>]
        member _.origin
            with set(_: string) = ()
    type AnimateTransformSVGAttributes with
        [<Erase>]
        member _.type'
            with set(_: string) = ()
    type CircleSVGAttributes with
        [<Erase>]
        member _.cx
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.cy
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.r
            with set(_: U2<float, string>) = ()
    type ClipPathSVGAttributes with
        [<Erase>]
        member _.clipPathUnits
            with set(_: string) = ()
        [<Erase>]
        member _.``clip-path``
            with set(_: string) = ()
        member this.clipPath
            with inline set(value: string) = this.``clip-path`` <- value

    type EllipseSVGAttributes with
        [<Erase>]
        member _.cx
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.cy
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.rx
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.ry
            with set(_: U2<float, string>) = ()
    type FeBlendSVGAttributes with
        [<Erase>]
        member _.mode
            with set(_: string) = ()
    type FeColorMatrixSVGAttributes with
        [<Erase>]
        member _.type'
            with set(_: string) = ()
        [<Erase>]
        member _.values
            with set(_: string) = ()
    type FeCompositeSVGAttributes with
        [<Erase>]
        member _.operator
            with set(_: string) = ()
        [<Erase>]
        member _.k1
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.k2
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.k3
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.k4
            with set(_: U2<float, string>) = ()
    type FeConvolveMatrixSVGAttributes with
        [<Erase>]
        member _.order
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.kernelMatrix
            with set(_: string) = ()
        [<Erase>]
        member _.divisor
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.bias
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.targetX
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.targetY
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.edgeMode
            with set(_: string) = ()
        [<Erase>]
        member _.kernelUnitLength
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.preserveAlpha
            with set(_: string) = ()
    type FeDiffuseLightingSVGAttributes with
        [<Erase>]
        member _.``lighting-color``
            with set(_: string) = ()
        member this.lightingColor
            with inline set(value: string) = this.``lighting-color`` <- value
        [<Erase>]
        member _.color
            with set(_: string) = ()
        [<Erase>]
        member _.surfaceScale
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.diffuseConstant
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.kernelUnitLength
            with set(_: U2<float, string>) = ()
    type FeDisplacementMapSVGAttributes with
        [<Erase>]
        member _.scale
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.xChannelSelector
            with set(_: string) = ()
        [<Erase>]
        member _.yChannelSelector
            with set(_: string) = ()
    type FeDistantLightSVGAttributes with
        [<Erase>]
        member _.azimuth
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.elevation
            with set(_: U2<float, string>) = ()
    type FeDropShadowSVGAttributes with
        [<Erase>]
        member _.color
            with set(_: string) = ()
        [<Erase>]
        member _.``flood-color``
            with set(_: string) = ()
        member this.floodColor
            with inline set(value: string) = this.``flood-color`` <- value
        [<Erase>]
        member _.``flood-opacity``
            with set(_: string) = ()
        member this.floodOpacity
            with inline set(value: string) = this.``flood-opacity`` <- value
        [<Erase>]
        member _.dx
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.dy
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.stdDeviation
            with set(_: U2<float, string>) = ()
    type FeFloodSVGAttributes with
        [<Erase>]
        member _.color
            with set(_: string) = ()
        [<Erase>]
        member _.``flood-color``
            with set(_: string) = ()
        member this.floodColor
            with inline set(value: string) = this.``flood-color`` <- value
        [<Erase>]
        member _.``flood-opacity``
            with set(_: string) = ()
        member this.floodOpacity
            with inline set(value: string) = this.``flood-opacity`` <- value

    type FeFuncSVGAttributes with
        [<Erase>]
        member _.type'
            with set(_: string) = ()
        [<Erase>]
        member _.tableValues
            with set(_: string) = ()
        [<Erase>]
        member _.slope
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.intercept
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.amplitude
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.exponent
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.offset
            with set(_: U2<float, string>) = ()
    type FeGaussianBlurSVGAttributes with
        [<Erase>]
        member _.stdDeviation
            with set(_: U2<float, string>) = ()
    type FeImageSVGAttributes with
        [<Erase>]
        member _.preserveAspectRatio
            with set(_: string) = ()
        [<Erase>]
        member _.href
            with set(_: string) = ()
    type FeMorphologySVGAttributes with
        [<Erase>]
        member _.operator
            with set(_: string) = ()
        [<Erase>]
        member _.radius
            with set(_: U2<float, string>) = ()
    type FeOffsetSVGAttributes with
        [<Erase>]
        member _.dx
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.dy
            with set(_: U2<float, string>) = ()
    type FePointLightSVGAttributes with
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.z
            with set(_: U2<float, string>) = ()
    type FeSpecularLightingSVGAttributes with
        [<Erase>]
        member _.``lighting-color``
            with set(_: string) = ()
        member this.lightingColor
            with inline set(value: string) = this.``lighting-color`` <- value
        [<Erase>]
        member _.color
            with set(_: string) = ()
        [<Erase>]
        member _.surfaceScale
            with set(_: string) = ()
        [<Erase>]
        member _.specularConstant
            with set(_: string) = ()
        [<Erase>]
        member _.specularExponent
            with set(_: string) = ()
        [<Erase>]
        member _.kernelUnitLength
            with set(_: U2<float, string>) = ()
    type FeSpotLightSVGAttributes with
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.z
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.pointsAtX
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.pointsAtY
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.pointsAtZ
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.specularExponent
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.limitingConeAngle
            with set(_: U2<float, string>) = ()
    type FeTurbulanceSVGAttributes with
        [<Erase>]
        member _.baseFrequency
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.numOctaves
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.seed
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.stitchTiles
            with set(_: string) = ()
        [<Erase>]
        member _.type'
            with set(_: string) = ()
    type FilterSVGAttributes with
        [<Erase>]
        member _.filterUnits
            with set(_: string) = ()
        [<Erase>]
        member _.primitiveUnits
            with set(_: string) = ()
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.width
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.height
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.filterRes
            with set(_: U2<float, string>) = ()
    type ForeignObjectSVGAttributes with
        [<Erase>]
        member _.visibility
            with set(_: string) = ()
        [<Erase>]
        member _.display
            with set(_: string) = ()
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.width
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.height
            with set(_: U2<float, string>) = ()
    type GSVGAttributes with
        [<Erase>]
        member _.visibility
            with set(_: string) = ()
        [<Erase>]
        member _.display
            with set(_: string) = ()

    type ImageSVGAttributes with
        [<Erase>]
        member _.``image-rendering``
            with set(_: string) = ()
        member this.imageRendering
            with inline set(value: string) = this.``image-rendering`` <- value
        [<Erase>]
        member _.``color-profile``
            with set(_: string) = ()
        member this.colorProfile
            with inline set(value: string) = this.``color-profile`` <- value
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.width
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.height
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.preserveAspectRatio
            with set(_: string) = ()
        [<Erase>]
        member _.href
            with set(_: string) = ()
    type LineSVGAttributes with
        [<Erase>]
        member _.``marker-end``
            with set(_: string) = ()
        member this.markerEnd
            with inline set(value: string) = this.``marker-end`` <- value
        [<Erase>]
        member _.``marker-mid``
            with set(_: string) = ()
        member this.markerMid
            with inline set(value: string) = this.``marker-mid`` <- value
        [<Erase>]
        member _.``marker-start``
            with set(_: string) = ()
        member this.markerStart
            with inline set(value: string) = this.``marker-start`` <- value
        [<Erase>]
        member _.x1
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y1
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.x2
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y2
            with set(_: U2<float, string>) = ()
    type LinearGradientSVGAttributes with
        [<Erase>]
        member _.x1
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.x2
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y1
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y2
            with set(_: U2<float, string>) = ()
    type MarkerSVGAttributes with
        [<Erase>]
        member _.clip
            with set(_: string) = ()
        [<Erase>]
        member _.overflow
            with set(_: string) = ()
        [<Erase>]
        member _.markerUnits
            with set(_: string) = ()
        [<Erase>]
        member _.refX
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.refY
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.markerWidth
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.markerHeight
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.orient
            with set(_: string) = ()
    type MaskSVGAttributes with
        [<Erase>]
        member _.filter
            with set(_: string) = ()
        [<Erase>]
        member _.opacity
            with set(_: string) = ()
        [<Erase>]
        member _.maskUnits
            with set(_: string) = ()
        [<Erase>]
        member _.maskContentUnits
            with set(_: string) = ()
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.width
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.height
            with set(_: U2<float, string>) = ()
    type PathSVGAttributes with
        [<Erase>]
        member _.``marker-end``
            with set(_: string) = ()
        member this.markerEnd
            with inline set(value: string) = this.``marker-end`` <- value
        [<Erase>]
        member _.``marker-mid``
            with set(_: string) = ()
        member this.markerMid
            with inline set(value: string) = this.``marker-mid`` <- value
        [<Erase>]
        member _.``marker-start``
            with set(_: string) = ()
        member this.markerStart
            with inline set(value: string) = this.``marker-start`` <- value
        [<Erase>]
        member _.d
            with set(_: string) = ()
        [<Erase>]
        member _.pathLength
            with set(_: U2<float, string>) = ()
    type PatternSVGAttributes with
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.width
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.height
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.patternUnits
            with set(_: string) = ()
        [<Erase>]
        member _.patternContentUnits
            with set(_: string) = ()
        [<Erase>]
        member _.patternTransform
            with set(_: string) = ()
        [<Erase>]
        member _.href
            with set(_: string) = ()
        [<Erase>]
        member _.clip
            with set(_: string) = ()
        [<Erase>]
        member _.overflow
            with set(_: string) = ()

    type PolygonSVGAttributes with
        [<Erase>]
        member _.``marker-end``
            with set(_: string) = ()
        member this.markerEnd
            with inline set(value: string) = this.``marker-end`` <- value
        [<Erase>]
        member _.``marker-mid``
            with set(_: string) = ()
        member this.markerMid
            with inline set(value: string) = this.``marker-mid`` <- value
        [<Erase>]
        member _.``marker-start``
            with set(_: string) = ()
        member this.markerStart
            with inline set(value: string) = this.``marker-start`` <- value
        [<Erase>]
        member _.points
            with set(_: string) = ()
    type PolylineSVGAttributes with
        [<Erase>]
        member _.``marker-end``
            with set(_: string) = ()
        member this.markerEnd
            with inline set(value: string) = this.``marker-end`` <- value
        [<Erase>]
        member _.``marker-mid``
            with set(_: string) = ()
        member this.markerMid
            with inline set(value: string) = this.``marker-mid`` <- value
        [<Erase>]
        member _.``marker-start``
            with set(_: string) = ()
        member this.markerStart
            with inline set(value: string) = this.``marker-start`` <- value
        [<Erase>]
        member _.points
            with set(_: string) = ()
    type RadialGradientSVGAttributes with
        [<Erase>]
        member _.cx
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.cy
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.r
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.fx
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.fy
            with set(_: U2<float, string>) = ()

    type RectSVGAttributes with
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.width
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.height
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.rx
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.ry
            with set(_: U2<float, string>) = ()
    type StopSVGAttributes with
        [<Erase>]
        member _.color
            with set(_: string) = ()
        [<Erase>]
        member _.``stop-color``
            with set(_: string) = ()
        member this.stopColor
            with inline set(value: string) = this.``stop-color`` <- value
        [<Erase>]
        member _.``stop-opacity``
            with set(_: string) = ()
        member this.stopOpacity
            with inline set(value: string) = this.``stop-opacity`` <- value
        [<Erase>]
        member _.offset
            with set(_: U2<float, string>) = ()

    type SvgSVGAttributes with
        [<Erase>]
        member _.version
            with set(_: string) = ()
        [<Erase>]
        member _.baseProfile
            with set(_: string) = ()
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.width
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.height
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.contentScriptType
            with set(_: string) = ()
        [<Erase>]
        member _.contentStyleType
            with set(_: string) = ()
        [<Erase>]
        member _.xmlns
            with set(_: string) = ()


    type SymbolSVGAttributes with
        [<Erase>]
        member _.width
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.height
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.preserveAspectRatio
            with set(_: string) = ()
        [<Erase>]
        member _.refX
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.refY
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.viewBox
            with set(_: string) = ()
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()

    type TextSVGAttributes with
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.dx
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.dy
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.rotate
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.textLength
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.lengthAdjust
            with set(_: string) = ()
        [<Erase>]
        member _.``writing-mode``
            with set(_: string) = ()
        member this.writingMode
            with inline set(value: string) = this.``writing-mode`` <- value
        [<Erase>]
        member _.``text-rendering``
            with set(_: string) = ()
        member this.textRendering
            with inline set(value: string) = this.``text-rendering`` <- value

    type TextPathSVGAttributes with
        [<Erase>]
        member _.startOffset
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.method
            with set(_: string) = ()
        [<Erase>]
        member _.spacing
            with set(_: string) = ()
        [<Erase>]
        member _.``alignment-baseline``
            with set(_: string) = ()
        member this.alignmentBaseline
            with inline set(value: string) = this.``alignment-baseline`` <- value
        [<Erase>]
        member _.``baseline-shift``
            with set(_: U2<float, string>) = ()
        member this.baselineShift
            with inline set(value: string) = this.``baseline-shift`` <- !^value
        [<Erase>]
        member _.display
            with set(_: string) = ()
        [<Erase>]
        member _.visibility
            with set(_: string) = ()
        [<Erase>]
        member _.href
            with set(_: string) = ()

    type TSpanSVGAttributes with
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.dx
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.dy
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.rotate
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.textLength
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.lengthAdjust
            with set(_: string) = ()
        [<Erase>]
        member _.``alignment-baseline``
            with set(_: string) = ()
        member this.alignmentBaseline
            with inline set(value: string) = this.``alignment-baseline`` <- value
        [<Erase>]
        member _.``baseline-shift``
            with set(_: U2<float, string>) = ()
        member this.baselineShift
            with inline set(value: string) = this.``baseline-shift`` <- !^value
        [<Erase>]
        member _.display
            with set(_: string) = ()
        [<Erase>]
        member _.visibility
            with set(_: string) = ()

    type UseSVGAttributes with
        [<Erase>]
        member _.x
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.y
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.width
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.height
            with set(_: U2<float, string>) = ()
        [<Erase>]
        member _.href
            with set(_: string) = ()

    type ViewSVGAttributes with
        [<Erase>]
        member _.viewTarget
            with set(_: string) = ()

    [<Erase>]
    module Svg =
        [<Erase>]
        type animate() =
            interface HtmlTag
            interface HtmlContainer
            interface AnimateSVGAttributes
        [<Erase>]
        type animateMotion() =
            interface HtmlTag
            interface HtmlContainer
            interface AnimateMotionSVGAttributes
        [<Erase>]
        type animateTransform() =
            interface HtmlTag
            interface HtmlContainer
            interface AnimateTransformSVGAttributes
        [<Erase>]
        type circle() =
            interface HtmlTag
            interface HtmlContainer
            interface CircleSVGAttributes
        [<Erase>]
        type clipPath() =
            interface HtmlTag
            interface HtmlContainer
            interface ClipPathSVGAttributes
        [<Erase>]
        type defs() =
            interface HtmlTag
            interface HtmlContainer
            interface DefsSVGAttributes
        [<Erase>]
        type desc() =
            interface HtmlTag
            interface HtmlContainer
            interface DescSVGAttributes
        [<Erase>]
        type ellipse() =
            interface HtmlTag
            interface HtmlContainer
            interface EllipseSVGAttributes
        [<Erase>]
        type feBlend() =
            interface HtmlTag
            interface HtmlContainer
            interface FeBlendSVGAttributes
        [<Erase>]
        type feColorMatrix() =
            interface HtmlTag
            interface HtmlContainer
            interface FeColorMatrixSVGAttributes
        [<Erase>]
        type feComponentTransfer() =
            interface HtmlTag
            interface HtmlContainer
            interface FeComponentTransferSVGAttributes
        [<Erase>]
        type feComposite() =
            interface HtmlTag
            interface HtmlContainer
            interface FeCompositeSVGAttributes
        [<Erase>]
        type feConvolveMatrix() =
            interface HtmlTag
            interface HtmlContainer
            interface FeConvolveMatrixSVGAttributes
        [<Erase>]
        type feDiffuseLighting() =
            interface HtmlTag
            interface HtmlContainer
            interface FeDiffuseLightingSVGAttributes
        [<Erase>]
        type feDisplacementMap() =
            interface HtmlTag
            interface HtmlContainer
            interface FeDisplacementMapSVGAttributes
        [<Erase>]
        type feDistantLight() =
            interface HtmlTag
            interface HtmlContainer
            interface FeDistantLightSVGAttributes
        [<Erase>]
        type feDropShadow() =
            interface HtmlTag
            interface HtmlContainer
            interface FeDropShadowSVGAttributes
        [<Erase>]
        type feFlood() =
            interface HtmlTag
            interface HtmlContainer
            interface FeFloodSVGAttributes
        [<Erase>]
        type feFuncA() =
            interface HtmlTag
            interface HtmlContainer
            interface FeFuncSVGAttributes
        [<Erase>]
        type feFuncB() =
            interface HtmlTag
            interface HtmlContainer
            interface FeFuncSVGAttributes
        [<Erase>]
        type feFuncG() =
            interface HtmlTag
            interface HtmlContainer
            interface FeFuncSVGAttributes
        [<Erase>]
        type feFuncR() =
            interface HtmlTag
            interface HtmlContainer
            interface FeFuncSVGAttributes
        [<Erase>]
        type feGaussianBlur() =
            interface HtmlTag
            interface HtmlContainer
            interface FeGaussianBlurSVGAttributes
        [<Erase>]
        type feImage() =
            interface HtmlTag
            interface HtmlContainer
            interface FeImageSVGAttributes
        [<Erase>]
        type feMerge() =
            interface HtmlTag
            interface HtmlContainer
            interface FeMergeSVGAttributes
        [<Erase>]
        type feMergeNode() =
            interface HtmlTag
            interface HtmlContainer
            interface FeMergeNodeSVGAttributes
        [<Erase>]
        type feMorphology() =
            interface HtmlTag
            interface HtmlContainer
            interface FeMorphologySVGAttributes
        [<Erase>]
        type feOffset() =
            interface HtmlTag
            interface HtmlContainer
            interface FeOffsetSVGAttributes
        [<Erase>]
        type fePointLight() =
            interface HtmlTag
            interface HtmlContainer
            interface FePointLightSVGAttributes
        [<Erase>]
        type feSpecularLighting() =
            interface HtmlTag
            interface HtmlContainer
            interface FeSpecularLightingSVGAttributes
        [<Erase>]
        type feSpotLight() =
            interface HtmlTag
            interface HtmlContainer
            interface FeSpotLightSVGAttributes
        [<Erase>]
        type feTile() =
            interface HtmlTag
            interface HtmlContainer
            interface FeTileSVGAttributes
        [<Erase>]
        type feTurbulence() =
            interface HtmlTag
            interface HtmlContainer
            interface FeTurbulanceSVGAttributes
        [<Erase>]
        type filter() =
            interface HtmlTag
            interface HtmlContainer
            interface FilterSVGAttributes
        [<Erase>]
        type foreignObject() =
            interface HtmlTag
            interface HtmlContainer
            interface ForeignObjectSVGAttributes
        [<Erase>]
        type g() =
            interface HtmlTag
            interface HtmlContainer
            interface GSVGAttributes
        [<Erase>]
        type image() =
            interface HtmlTag
            interface HtmlContainer
            interface ImageSVGAttributes
        [<Erase>]
        type line() =
            interface HtmlTag
            interface HtmlContainer
            interface LineSVGAttributes
        [<Erase>]
        type linearGradient() =
            interface HtmlTag
            interface HtmlContainer
            interface LinearGradientSVGAttributes
        [<Erase>]
        type marker() =
            interface HtmlTag
            interface HtmlContainer
            interface MarkerSVGAttributes
        [<Erase>]
        type mask() =
            interface HtmlTag
            interface HtmlContainer
            interface MaskSVGAttributes
        [<Erase>]
        type metadata() =
            interface HtmlTag
            interface HtmlContainer
            interface MetadataSVGAttributes
        [<Erase>]
        type mpath() =
            interface HtmlTag
            interface HtmlContainer
            interface MPathSVGAttributes
        [<Erase>]
        type path() =
            interface HtmlTag
            interface HtmlContainer
            interface PathSVGAttributes
        [<Erase>]
        type pattern() =
            interface HtmlTag
            interface HtmlContainer
            interface PatternSVGAttributes
        [<Erase>]
        type polygon() =
            interface HtmlTag
            interface HtmlContainer
            interface PolygonSVGAttributes
        [<Erase>]
        type polyline() =
            interface HtmlTag
            interface HtmlContainer
            interface PolylineSVGAttributes
        [<Erase>]
        type radialGradient() =
            interface HtmlTag
            interface HtmlContainer
            interface RadialGradientSVGAttributes
        [<Erase>]
        type rect() =
            interface HtmlTag
            interface HtmlContainer
            interface RectSVGAttributes
        [<Erase>]
        type set() =
            interface HtmlTag
            interface HtmlContainer
            interface SetSVGAttributes
        [<Erase>]
        type stop() =
            interface HtmlTag
            interface HtmlContainer
            interface StopSVGAttributes
        [<Erase>]
        type svg() =
            interface HtmlTag
            interface HtmlContainer
            interface SvgSVGAttributes
        [<Erase>]
        type switch() =
            interface HtmlTag
            interface HtmlContainer
            interface SwitchSVGAttributes
        [<Erase>]
        type symbol() =
            interface HtmlTag
            interface HtmlContainer
            interface SymbolSVGAttributes
        [<Erase>]
        type text() =
            interface HtmlTag
            interface HtmlContainer
            interface TextSVGAttributes
        [<Erase>]
        type textPath() =
            interface HtmlTag
            interface HtmlContainer
            interface TextPathSVGAttributes
        [<Erase>]
        type tspan() =
            interface HtmlTag
            interface HtmlContainer
            interface TSpanSVGAttributes
        [<Erase>]
        type use'() =
            interface HtmlTag
            interface HtmlContainer
            interface UseSVGAttributes
        [<Erase>]
        type view() =
            interface HtmlTag
            interface HtmlContainer
            interface ViewSVGAttributes
