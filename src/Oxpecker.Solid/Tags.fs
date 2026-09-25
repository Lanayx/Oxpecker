namespace Oxpecker.Solid

open System.Runtime.CompilerServices
open Browser.Types
open JetBrains.Annotations
open Fable.Core

[<AutoOpen>]
module Tags =

    /// Fragment (or template) node, only renders children, not itself.
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
        /// Unique element identifier; IDs should be unique in the document.
        [<Erase>]
        member this.id
            with set (_: string) = ()
        /// Space-separated CSS class names for the element.
        [<Erase>]
        member this.class'
            with set (_: string) = ()
        /// Inline CSS declarations, for example `color: red; margin: 1rem`.
        [<LanguageInjection(InjectedLanguage.CSS, Prefix = ".x{", Suffix = ";}")>]
        [<Erase>]
        member this.style
            with set (_: string) = ()
        /// Content language tag, for example `en` or `fr-CA`.
        [<Erase>]
        member this.lang
            with set (_: string) = ()
        /// Text direction. Allowed values: `ltr`, `rtl`, or `auto`.
        [<Erase>]
        member this.dir
            with set (_: string) = ()
        /// Keyboard focus order; use `0` for natural order and `-1` for programmatic focus.
        [<Erase>]
        member this.tabindex
            with set (_: int) = ()
        /// Advisory text, commonly shown as a tooltip.
        [<Erase>]
        member this.title
            with set (_: string) = ()
        /// Single-character keyboard shortcut.
        [<Erase>]
        member this.accesskey
            with set (_: char) = ()
        /// Whether content can be edited. Values: `true`, `false`, or `plaintext-only`.
        [<Erase>]
        member this.contenteditable
            with set (_: string) = ()
        /// Whether the element can be dragged. Values: `true` or `false`.
        [<Erase>]
        member this.draggable
            with set (_: string) = ()
        /// Virtual keyboard Enter key hint: `enter`, `done`, `go`, `next`, `previous`, `search`, or `send`.
        [<Erase>]
        member this.enterkeyhint
            with set (_: string) = ()
        /// Visibility state; values include `hidden` and `until-found`.
        [<Erase>]
        member this.hidden
            with set (_: string) = ()
        /// When true, disables interaction and focus for this element and its descendants.
        [<Erase>]
        member this.inert
            with set (_: bool) = ()
        /// Virtual keyboard hint: `none`, `text`, `decimal`, `numeric`, `tel`, `search`, `email`, or `url`.
        [<Erase>]
        member this.inputmode
            with set (_: string) = ()
        /// Popover mode: `auto`, `manual`, or `hint`; an empty value also enables it.
        [<Erase>]
        member this.popover
            with set (_: string) = ()
        /// Whether editable text should be spellchecked.
        [<Erase>]
        member this.spellcheck
            with set (_: bool) = ()
        /// Translation hint. Values: `yes` or `no`.
        [<Erase>]
        member this.translate
            with set (_: string) = ()
        /// Capitalization hint: `off`/`none`, `on`/`sentences`, `words`, or `characters`.
        [<Erase>]
        member this.autocapitalize
            with set (_: string) = ()
        /// Names the custom element that extends a built-in HTML element.
        [<Erase>]
        member this.is
            with set (_: string) = ()
        /// Exposes named shadow-tree parts for styling.
        [<Erase>]
        member this.part
            with set (_: string) = ()
        /// Names the shadow-DOM slot assigned to this element.
        [<Erase>]
        member this.slot
            with set (_: string) = ()

    /// Contains document metadata, including the title and linked resources.
    [<Erase>]
    type head() =
        interface RegularNode
    /// Contains the visible content of the document.
    [<Erase>]
    type body() =
        interface RegularNode
    /// Sets the document title shown in the browser tab.
    [<Erase>]
    type title() =
        interface RegularNode
    /// A generic block container with no inherent semantic meaning.
    [<Erase>]
    type div() =
        interface RegularNode
    /// A self-contained composition such as a post or comment.
    [<Erase>]
    type article() =
        interface RegularNode
    /// Groups thematically related content, usually with a heading.
    [<Erase>]
    type section() =
        interface RegularNode
    /// Introductory content for a page or section.
    [<Erase>]
    type header() =
        interface RegularNode
    /// Footer content for a page or section.
    [<Erase>]
    type footer() =
        interface RegularNode
    /// The dominant content of the document.
    [<Erase>]
    type main() =
        interface RegularNode
    /// The top-level heading of a page or section.
    [<Erase>]
    type h1() =
        interface RegularNode
    /// A second-level heading.
    [<Erase>]
    type h2() =
        interface RegularNode
    /// A third-level heading.
    [<Erase>]
    type h3() =
        interface RegularNode
    /// A fourth-level heading.
    [<Erase>]
    type h4() =
        interface RegularNode
    /// A fifth-level heading.
    [<Erase>]
    type h5() =
        interface RegularNode
    /// A sixth-level heading.
    [<Erase>]
    type h6() =
        interface RegularNode
    /// An unordered list, typically displayed with bullets.
    [<Erase>]
    type ul() =
        interface RegularNode
    /// An ordered list whose items form a sequence.
    [<Erase>]
    type ol() =
        interface RegularNode
    /// An item in an ordered or unordered list.
    [<Erase>]
    type li() =
        interface RegularNode
    /// A paragraph of text.
    [<Erase>]
    type p() =
        interface RegularNode
    /// A generic inline container with no inherent semantic meaning.
    [<Erase>]
    type span() =
        interface RegularNode
    /// Side comments or fine print.
    [<Erase>]
    type small() =
        interface RegularNode
    /// Content with strong importance or urgency.
    [<Erase>]
    type strong() =
        interface RegularNode
    /// Text with stress emphasis.
    [<Erase>]
    type em() =
        interface RegularNode
    /// A title or explanation for a table.
    [<Erase>]
    type caption() =
        interface RegularNode
    /// A section containing major navigation links.
    [<Erase>]
    type nav() =
        interface RegularNode
    /// A section containing search or filtering controls.
    [<Erase>]
    type search() =
        interface RegularNode
    /// Text set apart from normal prose, such as a technical term.
    [<Erase>]
    type i() =
        interface RegularNode
    /// Text stylistically offset from normal prose without added importance.
    [<Erase>]
    type b() =
        interface RegularNode
    /// Text annotated as non-textual, such as a proper name.
    [<Erase>]
    type u() =
        interface RegularNode
    /// Text that is no longer accurate or relevant.
    [<Erase>]
    type s() =
        interface RegularNode
    /// Fallback content shown when scripts are unavailable.
    [<Erase>]
    type noscript() =
        interface RegularNode
    /// A fragment of computer code.
    [<Erase>]
    type code() =
        interface RegularNode
    /// Preformatted text whose whitespace is preserved.
    [<Erase>]
    type pre() =
        interface RegularNode
    /// A block quotation.
    [<Erase>]
    type blockquote() =
        interface RegularNode
    /// The title of a cited creative work.
    [<Erase>]
    type cite() =
        interface RegularNode
    /// A short inline quotation.
    [<Erase>]
    type q() =
        interface RegularNode
    /// Contact information for the nearest page or article owner.
    [<Erase>]
    type address() =
        interface RegularNode
    /// Content deleted from a document.
    [<Erase>]
    type del() =
        interface RegularNode
    /// Content inserted into a document.
    [<Erase>]
    type ins() =
        interface RegularNode
    /// An abbreviation or acronym.
    [<Erase>]
    type abbr() =
        interface RegularNode
    /// The defining instance of a term.
    [<Erase>]
    type dfn() =
        interface RegularNode
    /// Subscript text.
    [<Erase>]
    type sub() =
        interface RegularNode
    /// Superscript text.
    [<Erase>]
    type sup() =
        interface RegularNode
    /// Inert content that can be instantiated later by script.
    [<Erase>]
    type template() =
        interface RegularNode

    /// A line break in text.
    [<Erase>]
    type br() =
        interface VoidNode
    /// A thematic break between sections.
    [<Erase>]
    type hr() =
        interface VoidNode

    /// A hyperlink or download link.
    [<Erase>]
    type a() =
        interface RegularNode
        /// Destination URL.
        [<Erase>]
        member this.href
            with set (_: string) = ()
        /// Language tag for the linked resource.
        [<Erase>]
        member this.hreflang
            with set (_: string) = ()
        /// Relationship to the destination or resource; use one or more link relation tokens.
        [<Erase>]
        member this.rel
            with set (_: string) = ()
        /// Browsing context: `_self`, `_blank`, `_parent`, `_top`, or a named context.
        [<Erase>]
        member this.target
            with set (_: string) = ()
        /// Suggests downloading the resource; a string may specify the filename.
        [<Erase>]
        member this.download
            with set (_: string) = ()
        /// Space-separated URLs to notify when the link is followed.
        [<Erase>]
        member this.ping
            with set (_: string) = ()
        /// Sets the linked resource MIME type as a hint to the browser.
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        /// Referrer behavior: `no-referrer`, `no-referrer-when-downgrade`, `origin`, `origin-when-cross-origin`, `same-origin`, `strict-origin`, `strict-origin-when-cross-origin`, or `unsafe-url`.
        [<Erase>]
        member this.referrerpolicy
            with set (_: string) = ()

    /// Sets the base URL for relative URLs in the document.
    [<Erase>]
    type base'() =
        interface VoidNode
        /// Destination URL.
        [<Erase>]
        member this.href
            with set (_: string) = ()
        /// Browsing context: `_self`, `_blank`, `_parent`, `_top`, or a named context.
        [<Erase>]
        member this.target
            with set (_: string) = ()

    /// Embeds an image; provide alternative text unless it is decorative.
    [<Erase>]
    type img() =
        interface VoidNode
        /// URL of the media, image, script, or embedded resource.
        [<Erase>]
        member this.src
            with set (_: string) = ()
        /// Alternative text; use an empty string for decorative images.
        [<Erase>]
        member this.alt
            with set (_: string) = ()
        /// Width in CSS pixels.
        [<Erase>]
        member this.width
            with set (_: int) = ()
        /// Height in CSS pixels.
        [<Erase>]
        member this.height
            with set (_: int) = ()
        /// Candidate image URLs with width or pixel-density descriptors.
        [<Erase>]
        member this.srcset
            with set (_: string) = ()
        /// Referrer behavior: `no-referrer`, `no-referrer-when-downgrade`, `origin`, `origin-when-cross-origin`, `same-origin`, `strict-origin`, `strict-origin-when-cross-origin`, or `unsafe-url`.
        [<Erase>]
        member this.referrerpolicy
            with set (_: string) = ()
        /// CORS mode: `anonymous`, `use-credentials`, or an empty string for anonymous mode.
        [<Erase>]
        member this.crossorigin
            with set (_: string) = ()
        /// Image slot sizes used with a responsive srcset.
        [<Erase>]
        member this.sizes
            with set (_: string) = ()
        /// Map association using a fragment URL such as `#map-name`.
        [<Erase>]
        member this.usemap
            with set (_: string) = ()
        /// Whether this linked image acts as a server-side image map.
        [<Erase>]
        member this.ismap
            with set (_: bool) = ()
        /// Image decoding hint: `async`, `sync`, or `auto`.
        [<Erase>]
        member this.decoding
            with set (_: string) = ()
        /// Loading hint: `eager` or `lazy`.
        [<Erase>]
        member this.loading
            with set (_: string) = ()
        /// Fetch priority hint: `high`, `low`, or `auto`.
        [<Erase>]
        member this.fetchpriority
            with set (_: string) = ()
        /// Identifier used to observe element rendering performance.
        [<Erase>]
        member this.elementtiming
            with set (_: string) = ()

    /// A form for submitting user-provided data.
    [<Erase>]
    type form() =
        interface RegularNode
        /// URL that receives submitted form data.
        [<Erase>]
        member this.action
            with set (_: string) = ()
        /// Submission method: `get`, `post`, or `dialog`.
        [<Erase>]
        member this.method
            with set (_: string) = ()
        /// Form encoding: `application/x-www-form-urlencoded`, `multipart/form-data`, or `text/plain`.
        [<Erase>]
        member this.enctype
            with set (_: string) = ()
        /// Browsing context: `_self`, `_blank`, `_parent`, `_top`, or a named context.
        [<Erase>]
        member this.target
            with set (_: string) = ()
        /// Character encodings accepted by the form, usually `UTF-8`.
        [<Erase>]
        member this.acceptCharset
            with set (_: string) = ()
        /// Autofill hint; use `on`, `off`, or a recognized token such as `email`, `name`, or `postal-code`.
        [<Erase>]
        member this.autocomplete
            with set (_: string) = ()
        /// Element name, used for form submission or script access as applicable.
        [<Erase>]
        member this.name
            with set (_: string) = ()
        /// Whether to skip constraint validation when submitting the form.
        [<Erase>]
        member this.novalidate
            with set (_: bool) = ()
        /// Relationship to the destination or resource; use one or more link relation tokens.
        [<Erase>]
        member this.rel
            with set (_: string) = ()

    /// Embeds or references a script or data block.
    [<Erase>]
    type script() =
        interface RegularNode
        /// URL of the media, image, script, or embedded resource.
        [<Erase>]
        member this.src
            with set (_: string) = ()
        /// Sets the script type. Use `module` for a JavaScript module, a JavaScript MIME type for classic scripts, or `importmap`/`speculationrules` for those data blocks.
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        /// Whether an external script executes asynchronously when available.
        [<Erase>]
        member this.async
            with set (_: bool) = ()
        /// Whether an external classic script waits until parsing finishes.
        [<Erase>]
        member this.defer
            with set (_: bool) = ()
        /// Subresource Integrity hash used to verify fetched content.
        [<Erase>]
        member this.integrity
            with set (_: string) = ()
        /// CORS mode: `anonymous`, `use-credentials`, or an empty string for anonymous mode.
        [<Erase>]
        member this.crossorigin
            with set (_: string) = ()
        /// Prevents execution in browsers that support JavaScript modules.
        [<Erase>]
        member this.nomodule
            with set (_: bool) = ()
        /// Content Security Policy nonce for an inline script or style.
        [<Erase>]
        member this.nonce
            with set (_: string) = ()
        /// Referrer behavior: `no-referrer`, `no-referrer-when-downgrade`, `origin`, `origin-when-cross-origin`, `same-origin`, `strict-origin`, `strict-origin-when-cross-origin`, or `unsafe-url`.
        [<Erase>]
        member this.referrerpolicy
            with set (_: string) = ()

    /// Links to an external resource, commonly a stylesheet or icon.
    [<Erase>]
    type link() =
        interface VoidNode
        /// Relationship to the destination or resource; use one or more link relation tokens.
        [<Erase>]
        member this.rel
            with set (_: string) = ()
        /// Destination URL.
        [<Erase>]
        member this.href
            with set (_: string) = ()
        /// MIME type of the linked resource, for example `text/css` for a stylesheet.
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        /// Media condition for which a resource applies.
        [<Erase>]
        member this.media
            with set (_: string) = ()
        /// Destination resource type for a preload link, such as `script`, `style`, `image`, or `font`.
        [<Erase>]
        member this.as'
            with set (_: string) = ()
        /// Icon dimensions for a link such as `rel="icon"`; use `any` for a scalable icon.
        [<Erase>]
        member this.sizes
            with set (_: string) = ()
        /// CORS mode: `anonymous`, `use-credentials`, or an empty string for anonymous mode.
        [<Erase>]
        member this.crossorigin
            with set (_: string) = ()
        /// Subresource Integrity hash used to verify fetched content.
        [<Erase>]
        member this.integrity
            with set (_: string) = ()
        /// Referrer behavior: `no-referrer`, `no-referrer-when-downgrade`, `origin`, `origin-when-cross-origin`, `same-origin`, `strict-origin`, `strict-origin-when-cross-origin`, or `unsafe-url`.
        [<Erase>]
        member this.referrerpolicy
            with set (_: string) = ()
        /// Disables a linked stylesheet when set to true.
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()
        /// Language tag for the linked resource.
        [<Erase>]
        member this.hreflang
            with set (_: string) = ()
        /// Responsive image sizes for a preload link.
        [<Erase>]
        member this.imagesizes
            with set (_: string) = ()
        /// Responsive image candidates for a preload link.
        [<Erase>]
        member this.imagesrcset
            with set (_: string) = ()
        /// Advisory text, commonly shown as a tooltip.
        [<Erase>]
        member this.title
            with set (_: string) = ()

    /// The root element of an HTML document.
    [<Erase>]
    type html() =
        interface RegularNode
        /// XML namespace URI; for HTML use `http://www.w3.org/1999/xhtml`.
        [<Erase>]
        member this.xmlns
            with set (_: string) = ()

    /// Provides metadata about the document.
    [<Erase>]
    type meta() =
        interface VoidNode
        /// Identifies the kind of metadata provided, such as `description` or `author`.
        [<Erase>]
        member this.name
            with set (_: string) = ()
        /// Value associated with a meta name or http-equiv.
        [<Erase>]
        member this.content
            with set (_: string) = ()
        /// Character encoding; use `UTF-8` for HTML5 documents.
        [<Erase>]
        member this.charset
            with set (_: string) = ()
        /// HTTP pragma such as `refresh` or `content-security-policy`.
        [<Erase>]
        member this.httpEquiv
            with set (_: string) = ()

    /// An interactive form control; its type determines its behavior.
    [<Erase>]
    type input() =
        interface VoidNode
        /// Sets the input control type. Values: `hidden`, `text`, `search`, `tel`, `url`, `email`, `password`, `date`, `month`, `week`, `time`, `datetime-local`, `number`, `range`, `color`, `checkbox`, `radio`, `file`, `submit`, `image`, `reset`, or `button`.
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        /// Element name, used for form submission or script access as applicable.
        [<Erase>]
        member this.name
            with set (_: string) = ()
        /// Submitted or machine-readable value.
        [<Erase>]
        member this.value
            with set (_: string) = ()
        /// Short hint shown when a control has no value.
        [<Erase>]
        member this.placeholder
            with set (_: string) = ()
        /// Whether a value is required before form submission.
        [<Erase>]
        member this.required
            with set (_: bool) = ()
        /// Requests focus when the page or dialog loads.
        [<Erase>]
        member this.autofocus
            with set (_: bool) = ()
        /// Autofill hint; use `on`, `off`, or a recognized token such as `email`, `name`, or `postal-code`.
        [<Erase>]
        member this.autocomplete
            with set (_: string) = ()
        /// Minimum permitted value.
        [<Erase>]
        member this.min
            with set (_: string) = ()
        /// Maximum permitted value.
        [<Erase>]
        member this.max
            with set (_: string) = ()
        /// Permitted step interval.
        [<Erase>]
        member this.step
            with set (_: string) = ()
        /// Regular expression that the input value must match.
        [<Erase>]
        member this.pattern
            with set (_: string) = ()
        /// Prevents editing while allowing the value to be submitted.
        [<Erase>]
        member this.readonly
            with set (_: bool) = ()
        /// Disables the control and excludes it from form submission.
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()
        /// Whether multiple values can be selected or entered.
        [<Erase>]
        member this.multiple
            with set (_: bool) = ()
        /// Accepted file MIME types or extensions, separated by commas.
        [<Erase>]
        member this.accept
            with set (_: string) = ()
        /// Id of a datalist providing suggestions.
        [<Erase>]
        member this.list
            with set (_: string) = ()
        /// Maximum number of characters allowed.
        [<Erase>]
        member this.maxlength
            with set (_: int) = ()
        /// Minimum number of characters required.
        [<Erase>]
        member this.minlength
            with set (_: int) = ()
        /// Visible control size or, for select, number of visible options.
        [<Erase>]
        member this.size
            with set (_: int) = ()
        /// URL of the media, image, script, or embedded resource.
        [<Erase>]
        member this.src
            with set (_: string) = ()
        /// Width in CSS pixels.
        [<Erase>]
        member this.width
            with set (_: int) = ()
        /// Height in CSS pixels.
        [<Erase>]
        member this.height
            with set (_: int) = ()
        /// Accessible label for an image submit button (`type="image"`).
        [<Erase>]
        member this.alt
            with set (_: string) = ()
        /// Initial checked state of a checkbox or radio input.
        [<Erase>]
        member this.checked'
            with set (_: bool) = ()
        /// Name of the form field used to submit text direction.
        [<Erase>]
        member this.dirname
            with set (_: string) = ()
        /// Id of the form associated with this control.
        [<Erase>]
        member this.form
            with set (_: string) = ()
        /// Overrides the form submission URL for this submit control.
        [<Erase>]
        member this.formaction
            with set (_: string) = ()
        /// Form encoding override: `application/x-www-form-urlencoded`, `multipart/form-data`, or `text/plain`.
        [<Erase>]
        member this.formenctype
            with set (_: string) = ()
        /// Form method override: `get`, `post`, or `dialog`.
        [<Erase>]
        member this.formmethod
            with set (_: string) = ()
        /// Whether to submit without constraint validation.
        [<Erase>]
        member this.formnovalidate
            with set (_: bool) = ()
        /// Browsing context that receives the form response.
        [<Erase>]
        member this.formtarget
            with set (_: string) = ()
        /// Virtual keyboard hint: `none`, `text`, `decimal`, `numeric`, `tel`, `search`, `email`, or `url`.
        [<Erase>]
        member this.inputmode
            with set (_: string) = ()
        /// Capture hint for file inputs: `user`, `environment`, or an empty string.
        [<Erase>]
        member this.capture
            with set (_: string) = ()

    /// Displays the result of a calculation or user action.
    [<Erase>]
    type output() =
        interface RegularNode
        /// Space-separated IDs of controls that contributed to the calculated result.
        [<Erase>]
        member this.for'
            with set (_: string) = ()
        /// Id of the form associated with this control.
        [<Erase>]
        member this.form
            with set (_: string) = ()
        /// Element name, used for form submission or script access as applicable.
        [<Erase>]
        member this.name
            with set (_: string) = ()

    /// A multiline text-entry control.
    [<Erase>]
    type textarea() =
        interface RegularNode
        /// Element name, used for form submission or script access as applicable.
        [<Erase>]
        member this.name
            with set (_: string) = ()
        /// Short hint shown when a control has no value.
        [<Erase>]
        member this.placeholder
            with set (_: string) = ()
        /// Whether a value is required before form submission.
        [<Erase>]
        member this.required
            with set (_: bool) = ()
        /// Requests focus when the page or dialog loads.
        [<Erase>]
        member this.autofocus
            with set (_: bool) = ()
        /// Autofill hint; use `on`, `off`, or a recognized token such as `email`, `name`, or `postal-code`.
        [<Erase>]
        member this.autocomplete
            with set (_: string) = ()
        /// Prevents editing while allowing the value to be submitted.
        [<Erase>]
        member this.readonly
            with set (_: bool) = ()
        /// Disables the control and excludes it from form submission.
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()
        /// Number of visible text lines or rows, depending on the element.
        [<Erase>]
        member this.rows
            with set (_: int) = ()
        /// Visible textarea width in average character widths.
        [<Erase>]
        member this.cols
            with set (_: int) = ()
        /// Textarea line wrapping mode: `soft`, `hard`, or `off`.
        [<Erase>]
        member this.wrap
            with set (_: string) = ()
        /// Maximum number of characters allowed.
        [<Erase>]
        member this.maxlength
            with set (_: int) = ()
        /// Minimum number of characters required.
        [<Erase>]
        member this.minlength
            with set (_: int) = ()
        /// Name of the form field used to submit text direction.
        [<Erase>]
        member this.dirname
            with set (_: string) = ()
        /// Id of the form associated with this control.
        [<Erase>]
        member this.form
            with set (_: string) = ()


    /// A clickable button for form submission or actions.
    [<Erase>]
    type button() =
        interface RegularNode
        /// Sets the button behavior. Values: `submit`, `reset`, or `button`.
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        /// Element name, used for form submission or script access as applicable.
        [<Erase>]
        member this.name
            with set (_: string) = ()
        /// Submitted or machine-readable value.
        [<Erase>]
        member this.value
            with set (_: string) = ()
        /// Disables the control and excludes it from form submission.
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()
        /// Requests focus when the page or dialog loads.
        [<Erase>]
        member this.autofocus
            with set (_: bool) = ()
        /// Id of the form associated with this control.
        [<Erase>]
        member this.form
            with set (_: string) = ()
        /// Overrides the form submission URL for this submit control.
        [<Erase>]
        member this.formaction
            with set (_: string) = ()
        /// Form encoding override: `application/x-www-form-urlencoded`, `multipart/form-data`, or `text/plain`.
        [<Erase>]
        member this.formenctype
            with set (_: string) = ()
        /// Form method override: `get`, `post`, or `dialog`.
        [<Erase>]
        member this.formmethod
            with set (_: string) = ()
        /// Whether to submit without constraint validation.
        [<Erase>]
        member this.formnovalidate
            with set (_: bool) = ()
        /// Browsing context that receives the form response.
        [<Erase>]
        member this.formtarget
            with set (_: string) = ()
        /// Id of the popover controlled by this button.
        [<Erase>]
        member this.popovertarget
            with set (_: string) = ()
        /// Popover action: `toggle`, `show`, or `hide`.
        [<Erase>]
        member this.popovertargetaction
            with set (_: string) = ()

    /// A control for choosing one or more options.
    [<Erase>]
    type select() =
        interface RegularNode
        /// Element name, used for form submission or script access as applicable.
        [<Erase>]
        member this.name
            with set (_: string) = ()
        /// Whether a value is required before form submission.
        [<Erase>]
        member this.required
            with set (_: bool) = ()
        /// Requests focus when the page or dialog loads.
        [<Erase>]
        member this.autofocus
            with set (_: bool) = ()
        /// Autofill hint; use `on`, `off`, or a recognized token such as `email`, `name`, or `postal-code`.
        [<Erase>]
        member this.autocomplete
            with set (_: string) = ()
        /// Disables the control and excludes it from form submission.
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()
        /// Whether multiple values can be selected or entered.
        [<Erase>]
        member this.multiple
            with set (_: bool) = ()
        /// Visible control size or, for select, number of visible options.
        [<Erase>]
        member this.size
            with set (_: int) = ()
        /// Id of the form associated with this control.
        [<Erase>]
        member this.form
            with set (_: string) = ()

    /// A selectable choice in a select, optgroup, or datalist.
    [<Erase>]
    type option() =
        interface RegularNode
        /// Submitted or machine-readable value.
        [<Erase>]
        member this.value
            with set (_: string) = ()
        /// Whether this option is initially selected.
        [<Erase>]
        member this.selected
            with set (_: bool) = ()
        /// Disables the control and excludes it from form submission.
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()
        /// Human-readable label for this option or group.
        [<Erase>]
        member this.label
            with set (_: string) = ()

    /// Groups related options in a select control.
    [<Erase>]
    type optgroup() =
        interface RegularNode
        /// Human-readable label for this option or group.
        [<Erase>]
        member this.label
            with set (_: string) = ()
        /// Disables the control and excludes it from form submission.
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()

    /// A caption for a form control.
    [<Erase>]
    type label() =
        interface RegularNode
        /// ID of the single labelable form control associated with this label.
        [<Erase>]
        member this.for'
            with set (_: string) = ()

    /// Contains CSS rules that apply to the document.
    [<Erase>]
    type style() =
        interface RegularNode
        /// MIME type of the embedded style; use `text/css` (the HTML default).
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        /// Media condition for which a resource applies.
        [<Erase>]
        member this.media
            with set (_: string) = ()

    /// Embeds another HTML page as a nested browsing context.
    [<Erase>]
    type iframe() =
        interface RegularNode
        /// URL of the media, image, script, or embedded resource.
        [<Erase>]
        member this.src
            with set (_: string) = ()
        /// Name of the nested browsing context, usable as a link or form target.
        [<Erase>]
        member this.name
            with set (_: string) = ()
        /// Iframe restrictions; empty means maximum restrictions, otherwise use space-separated tokens such as `allow-scripts` and `allow-forms`.
        [<Erase>]
        member this.sandbox
            with set (_: string) = ()
        /// Width in CSS pixels.
        [<Erase>]
        member this.width
            with set (_: int) = ()
        /// Height in CSS pixels.
        [<Erase>]
        member this.height
            with set (_: int) = ()
        /// Permissions Policy features allowed for the embedded iframe page.
        [<Erase>]
        member this.allow
            with set (_: string) = ()
        /// Loading hint: `eager` or `lazy`.
        [<Erase>]
        member this.loading
            with set (_: string) = ()
        /// Referrer behavior: `no-referrer`, `no-referrer-when-downgrade`, `origin`, `origin-when-cross-origin`, `same-origin`, `strict-origin`, `strict-origin-when-cross-origin`, or `unsafe-url`.
        [<Erase>]
        member this.referrerpolicy
            with set (_: string) = ()
        /// Inline HTML content for the iframe.
        [<Erase>]
        member this.srcdoc
            with set (_: string) = ()

    /// Embeds video content.
    [<Erase>]
    type video() =
        interface RegularNode
        /// URL of the media, image, script, or embedded resource.
        [<Erase>]
        member this.src
            with set (_: string) = ()
        /// Image URL shown before video playback.
        [<Erase>]
        member this.poster
            with set (_: string) = ()
        /// Requests automatic playback; browsers may restrict autoplay.
        [<Erase>]
        member this.autoplay
            with set (_: bool) = ()
        /// Whether to show native media playback controls.
        [<Erase>]
        member this.controls
            with set (_: bool) = ()
        /// Allows video playback inline instead of automatically entering fullscreen.
        [<Erase>]
        member this.playsinline
            with set (_: bool) = ()
        /// Media control hints: `nodownload`, `nofullscreen`, or `noremoteplayback`.
        [<Erase>]
        member this.controlsList
            with set (_: string) = ()
        /// CORS mode: `anonymous`, `use-credentials`, or an empty string for anonymous mode.
        [<Erase>]
        member this.crossorigin
            with set (_: string) = ()
        /// Whether playback restarts at the end.
        [<Erase>]
        member this.loop
            with set (_: bool) = ()
        /// Whether media starts muted.
        [<Erase>]
        member this.muted
            with set (_: bool) = ()
        /// Width in CSS pixels.
        [<Erase>]
        member this.width
            with set (_: int) = ()
        /// Height in CSS pixels.
        [<Erase>]
        member this.height
            with set (_: int) = ()
        /// Preload hint: `none`, `metadata`, or `auto`.
        [<Erase>]
        member this.preload
            with set (_: string) = ()
        /// Disables remote playback UI for media.
        [<Erase>]
        member this.disableremoteplayback
            with set (_: bool) = ()
        /// Disables the picture-in-picture control where supported.
        [<Erase>]
        member this.disablepictureinpicture
            with set (_: bool) = ()

    /// Embeds audio content.
    [<Erase>]
    type audio() =
        interface RegularNode
        /// URL of the media, image, script, or embedded resource.
        [<Erase>]
        member this.src
            with set (_: string) = ()
        /// Requests automatic playback; browsers may restrict autoplay.
        [<Erase>]
        member this.autoplay
            with set (_: bool) = ()
        /// Whether to show native media playback controls.
        [<Erase>]
        member this.controls
            with set (_: bool) = ()
        /// Media control hints: `nodownload`, `nofullscreen`, or `noremoteplayback`.
        [<Erase>]
        member this.controlsList
            with set (_: string) = ()
        /// CORS mode: `anonymous`, `use-credentials`, or an empty string for anonymous mode.
        [<Erase>]
        member this.crossorigin
            with set (_: string) = ()
        /// Preload hint: `none`, `metadata`, or `auto`.
        [<Erase>]
        member this.preload
            with set (_: string) = ()
        /// Whether playback restarts at the end.
        [<Erase>]
        member this.loop
            with set (_: bool) = ()
        /// Whether media starts muted.
        [<Erase>]
        member this.muted
            with set (_: bool) = ()
        /// Disables remote playback UI for media.
        [<Erase>]
        member this.disableremoteplayback
            with set (_: bool) = ()

    /// Provides a media or image source.
    [<Erase>]
    type source() =
        interface VoidNode
        /// URL of the media, image, script, or embedded resource.
        [<Erase>]
        member this.src
            with set (_: string) = ()
        /// MIME type of the media source, for example `audio/ogg` or `video/mp4`.
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        /// Media condition for which a resource applies.
        [<Erase>]
        member this.media
            with set (_: string) = ()
        /// Image slot sizes used with a responsive srcset.
        [<Erase>]
        member this.sizes
            with set (_: string) = ()
        /// Candidate image URLs with width or pixel-density descriptors.
        [<Erase>]
        member this.srcset
            with set (_: string) = ()

    /// A scriptable bitmap drawing surface.
    [<Erase>]
    type canvas() =
        interface RegularNode
        /// Width in CSS pixels.
        [<Erase>]
        member this.width
            with set (_: int) = ()
        /// Height in CSS pixels.
        [<Erase>]
        member this.height
            with set (_: int) = ()

    /// Embeds an external resource.
    [<Erase>]
    type object'() =
        interface RegularNode
        /// URL of the resource embedded by this object element.
        [<Erase>]
        member this.data
            with set (_: string) = ()
        /// MIME type of the embedded resource.
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        /// Width in CSS pixels.
        [<Erase>]
        member this.width
            with set (_: int) = ()
        /// Height in CSS pixels.
        [<Erase>]
        member this.height
            with set (_: int) = ()

    /// Provides a parameter for an object element.
    [<Erase>]
    type param() =
        interface VoidNode
        /// Name of the parameter passed to the embedded object.
        [<Erase>]
        member this.name
            with set (_: string) = ()
        /// Submitted or machine-readable value.
        [<Erase>]
        member this.value
            with set (_: string) = ()

    /// Associates machine-readable data with human-readable content.
    [<Erase>]
    type data() =
        interface RegularNode
        /// Submitted or machine-readable value.
        [<Erase>]
        member this.value
            with set (_: string) = ()

    /// Represents a date, time, or duration.
    [<Erase>]
    type time() =
        interface RegularNode
        /// Machine-readable date, time, or duration.
        [<Erase>]
        member this.datetime
            with set (_: string) = ()

    /// Displays the completion progress of a task.
    [<Erase>]
    type progress() =
        interface RegularNode
        /// Submitted or machine-readable value.
        [<Erase>]
        member this.value
            with set (_: string) = ()
        /// Maximum permitted value.
        [<Erase>]
        member this.max
            with set (_: string) = ()

    /// Displays a scalar measurement within a known range.
    [<Erase>]
    type meter() =
        interface RegularNode
        /// Id of the form associated with this control.
        [<Erase>]
        member this.form
            with set (_: string) = ()
        /// Submitted or machine-readable value.
        [<Erase>]
        member this.value
            with set (_: string) = ()
        /// Minimum permitted value.
        [<Erase>]
        member this.min
            with set (_: string) = ()
        /// Maximum permitted value.
        [<Erase>]
        member this.max
            with set (_: string) = ()
        /// Lower threshold of a meter's low range.
        [<Erase>]
        member this.low
            with set (_: string) = ()
        /// Upper threshold of a meter's high range.
        [<Erase>]
        member this.high
            with set (_: string) = ()
        /// Optimal value within the meter's range.
        [<Erase>]
        member this.optimum
            with set (_: string) = ()

    /// A disclosure widget that can show or hide its contents.
    [<Erase>]
    type details() =
        interface RegularNode
        /// Whether the details disclosure or dialog is open.
        [<Erase>]
        member this.open'
            with set (_: bool) = ()

    /// The visible label for a details disclosure widget.
    [<Erase>]
    type summary() =
        interface RegularNode

    /// A dialog or other interactive component.
    [<Erase>]
    type dialog() =
        interface RegularNode
        /// Whether the details disclosure or dialog is open.
        [<Erase>]
        member this.open'
            with set (_: bool) = ()

    /// A list of commands or menu items.
    [<Erase>]
    type menu() =
        interface RegularNode

    /// Provides suggested options for an input control.
    [<Erase>]
    type datalist() =
        interface RegularNode

    /// Groups related form controls.
    [<Erase>]
    type fieldset() =
        interface RegularNode
        /// Disables the control and excludes it from form submission.
        [<Erase>]
        member this.disabled
            with set (_: bool) = ()

        /// Id of the form associated with this control.
        [<Erase>]
        member this.form
            with set (_: string) = ()
        /// Element name, used for form submission or script access as applicable.
        [<Erase>]
        member this.name
            with set (_: string) = ()

    /// A caption for a fieldset.
    [<Erase>]
    type legend() =
        interface RegularNode
    /// Tabular data arranged in rows and columns.
    [<Erase>]
    type table() =
        interface RegularNode
    /// Groups the body rows of a table.
    [<Erase>]
    type tbody() =
        interface RegularNode
    /// Groups the header rows of a table.
    [<Erase>]
    type thead() =
        interface RegularNode
    /// Groups the footer rows of a table.
    [<Erase>]
    type tfoot() =
        interface RegularNode
    /// A row of table cells.
    [<Erase>]
    type tr() =
        interface RegularNode
    /// A header cell in a table.
    [<Erase>]
    type th() =
        interface RegularNode
        /// Abbreviated description of a table header cell.
        [<Erase>]
        member this.abbr
            with set (_: string) = ()
        /// Number of table columns spanned; must be a positive integer.
        [<Erase>]
        member this.colspan
            with set (_: int) = ()
        /// Number of table rows spanned; `0` spans the remaining rows in the row group.
        [<Erase>]
        member this.rowspan
            with set (_: int) = ()
        /// Ids of header cells that apply to this table cell.
        [<Erase>]
        member this.headers
            with set (_: string) = ()
        /// Header scope: `row`, `col`, `rowgroup`, or `colgroup`.
        [<Erase>]
        member this.scope
            with set (_: string) = ()
    /// A data cell in a table.
    [<Erase>]
    type td() =
        interface RegularNode
        /// Number of table columns spanned; must be a positive integer.
        [<Erase>]
        member this.colspan
            with set (_: int) = ()
        /// Number of table rows spanned; `0` spans the remaining rows in the row group.
        [<Erase>]
        member this.rowspan
            with set (_: int) = ()
        /// Ids of header cells that apply to this table cell.
        [<Erase>]
        member this.headers
            with set (_: string) = ()

    /// Defines an image map containing clickable areas.
    [<Erase>]
    type map() =
        interface RegularNode
        /// Name of the image map referenced by an image `usemap` attribute.
        [<Erase>]
        member this.name
            with set (_: string) = ()
    /// A clickable region in an image map.
    [<Erase>]
    type area() =
        interface VoidNode
        /// Image-map area shape: `rect`, `circle`, `poly`, or `default`.
        [<Erase>]
        member this.shape
            with set (_: string) = ()
        /// Comma-separated pixel coordinates for the image-map area.
        [<Erase>]
        member this.coords
            with set (_: string) = ()
        /// Destination URL.
        [<Erase>]
        member this.href
            with set (_: string) = ()
        /// Accessible name describing this image-map link when `href` is present.
        [<Erase>]
        member this.alt
            with set (_: string) = ()
        /// Suggests downloading the resource; a string may specify the filename.
        [<Erase>]
        member this.download
            with set (_: string) = ()
        /// Browsing context: `_self`, `_blank`, `_parent`, `_top`, or a named context.
        [<Erase>]
        member this.target
            with set (_: string) = ()
        /// Relationship to the destination or resource; use one or more link relation tokens.
        [<Erase>]
        member this.rel
            with set (_: string) = ()
        /// Referrer behavior: `no-referrer`, `no-referrer-when-downgrade`, `origin`, `origin-when-cross-origin`, `same-origin`, `strict-origin`, `strict-origin-when-cross-origin`, or `unsafe-url`.
        [<Erase>]
        member this.referrerpolicy
            with set (_: string) = ()
        /// Space-separated URLs to notify when the link is followed.
        [<Erase>]
        member this.ping
            with set (_: string) = ()

    /// Content related indirectly to the surrounding content, such as a sidebar.
    [<Erase>]
    type aside() =
        interface RegularNode
    /// Isolates text with directionality that may differ from surrounding text.
    [<Erase>]
    type bdi() =
        interface RegularNode
    /// Overrides the directionality of its text.
    [<Erase>]
    type bdo() =
        interface RegularNode
    /// Applies properties to one or more table columns.
    [<Erase>]
    type col() =
        interface VoidNode
        /// Number of table columns represented; use a positive integer.
        [<Erase>]
        member this.span
            with set (_: int) = ()
    /// Groups table columns for shared properties.
    [<Erase>]
    type colgroup() =
        interface RegularNode
        /// Number of table columns represented; use a positive integer.
        [<Erase>]
        member this.span
            with set (_: int) = ()
    /// The description or value associated with a term.
    [<Erase>]
    type dd() =
        interface RegularNode
    /// A description list of terms and their descriptions.
    [<Erase>]
    type dl() =
        interface RegularNode
    /// A term or name in a description list.
    [<Erase>]
    type dt() =
        interface RegularNode
    /// Embeds external content.
    [<Erase>]
    type embed() =
        interface VoidNode
        /// URL of the media, image, script, or embedded resource.
        [<Erase>]
        member this.src
            with set (_: string) = ()
        /// MIME type of the embedded resource.
        [<Erase>]
        member this.type'
            with set (_: string) = ()
        /// Width in CSS pixels.
        [<Erase>]
        member this.width
            with set (_: int) = ()
        /// Height in CSS pixels.
        [<Erase>]
        member this.height
            with set (_: int) = ()
    /// A caption or legend for a figure.
    [<Erase>]
    type figcaption() =
        interface RegularNode
    /// Self-contained content such as an illustration or diagram.
    [<Erase>]
    type figure() =
        interface RegularNode
    /// Text representing user input, commonly keyboard input.
    [<Erase>]
    type kbd() =
        interface RegularNode
    /// Text highlighted for relevance.
    [<Erase>]
    type mark() =
        interface RegularNode
    /// Provides responsive image sources and an img fallback.
    [<Erase>]
    type picture() =
        interface RegularNode
    /// Sample output from a computer program.
    [<Erase>]
    type samp() =
        interface RegularNode
    /// A variable in a mathematical or programming context.
    [<Erase>]
    type var() =
        interface RegularNode
    /// An optional line-break opportunity within text.
    [<Erase>]
    type wbr() =
        interface RegularNode
