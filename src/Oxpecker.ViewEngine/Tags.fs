namespace Oxpecker.ViewEngine

open System.Diagnostics.CodeAnalysis
open Oxpecker.ViewEngine.Tools
open System.Runtime.CompilerServices
open JetBrains.Annotations

[<AutoOpen>]
module Tags =

    open Oxpecker.ViewEngine.Builder

    /// Fragment (or template) node, only renders children, not itself
    type Fragment() =
        inherit FragmentNode()

    /// Set of html extensions that keep original type
    type HtmlElementExtensions =

        /// Add an attribute to the element
        [<Extension>]
        static member attr(this: #HtmlTag, name: string, value: string | null) =
            if isNotNull value then
                this.AddAttribute({ Name = name; Value = value })
            this

        /// Adds an attribute switch to an element based on conditional
        [<Extension>]
        static member bool(this: #HtmlTag, name: string, value: bool) =
            if value then
                this.AddAttribute({ Name = name; Value = null })
            this
        /// Add event handler to the element through the corresponding attribute
        [<Extension>]
        static member on(this: #HtmlTag, eventName: string, [<StringSyntax("js")>] eventHandler: string) =
            this.attr($"on%s{eventName}", eventHandler)

        /// Add data attribute to the element
        [<Extension>]
        static member data(this: #HtmlTag, name: string, value: string) = this.attr($"data-%s{name}", value)

    // global attributes
    type HtmlTag with
        /// Unique element identifier; IDs should be unique in the document.
        member this.id
            with set (value: string | null) = this.attr("id", value) |> ignore
        /// Space-separated CSS class names for the element.
        member this.class'
            with set (value: string | null) = this.attr("class", value) |> ignore
        /// Inline CSS declarations, for example `color: red; margin: 1rem`.
        [<LanguageInjection(InjectedLanguage.CSS, Prefix = ".x{", Suffix = ";}")>]
        member this.style
            with set (value: string | null) = this.attr("style", value) |> ignore
        /// Content language tag, for example `en` or `fr-CA`.
        member this.lang
            with set (value: string | null) = this.attr("lang", value) |> ignore
        /// Text direction. Values: `ltr`, `rtl`, or `auto`.
        member this.dir
            with set (value: string | null) = this.attr("dir", value) |> ignore
        /// Keyboard focus order; use `0` for natural order and `-1` for programmatic focus.
        member this.tabindex
            with set (value: int) = this.attr("tabindex", string value) |> ignore
        /// Advisory text, commonly shown as a tooltip.
        member this.title
            with set (value: string | null) = this.attr("title", value) |> ignore
        /// Single-character keyboard shortcut.
        member this.accesskey
            with set (value: char) = this.attr("accesskey", string value) |> ignore
        /// Whether content can be edited. Values: `true`, `false`, or `plaintext-only`.
        member this.contenteditable
            with set (value: string | null) = this.attr("contenteditable", value) |> ignore
        /// Whether the element can be dragged. Values: `true` or `false`.
        member this.draggable
            with set (value: string | null) = this.attr("draggable", value) |> ignore
        /// Virtual keyboard Enter key hint: `enter`, `done`, `go`, `next`, `previous`, `search`, or `send`.
        member this.enterkeyhint
            with set (value: string | null) = this.attr("enterkeyhint", value) |> ignore
        /// Visibility state; values include `hidden` and `until-found`.
        member this.hidden
            with set (value: string | null) = this.attr("hidden", value) |> ignore
        /// When true, disables interaction and focus for this element and its descendants.
        member this.inert
            with set (value: bool) = this.bool("inert", value) |> ignore
        /// Virtual keyboard hint: `none`, `text`, `decimal`, `numeric`, `tel`, `search`, `email`, or `url`.
        member this.inputmode
            with set (value: string | null) = this.attr("inputmode", value) |> ignore
        /// Popover mode: `auto`, `manual`, or `hint`; an empty value also enables it.
        member this.popover
            with set (value: string | null) = this.attr("popover", value) |> ignore
        /// Whether editable text should be spellchecked.
        member this.spellcheck
            with set (value: bool) = this.attr("spellcheck", (if value then "true" else "false")) |> ignore
        /// Translation hint. Values: `yes` or `no`.
        member this.translate
            with set (value: string | null) = this.attr("translate", value) |> ignore
        /// Capitalization hint: `off`/`none`, `on`/`sentences`, `words`, or `characters`.
        member this.autocapitalize
            with set (value: string | null) = this.attr("autocapitalize", value) |> ignore
        /// Names the custom element that extends a built-in HTML element.
        member this.is
            with set (value: string | null) = this.attr("is", value) |> ignore
        /// Exposes named shadow-tree parts for styling.
        member this.part
            with set (value: string | null) = this.attr("part", value) |> ignore
        /// Names the shadow-DOM slot assigned to this element.
        member this.slot
            with set (value: string | null) = this.attr("slot", value) |> ignore


    /// Contains document metadata, including the title and linked resources.
    type head() =
        inherit RegularNode("head")
    /// Contains the visible content of the document.
    type body() =
        inherit RegularNode("body")
    /// Sets the document title shown in the browser tab.
    type title() =
        inherit RegularNode("title")
    /// A generic block container with no inherent semantic meaning.
    type div() =
        inherit RegularNode("div")
    /// A self-contained composition such as a post or comment.
    type article() =
        inherit RegularNode("article")
    /// Groups thematically related content, usually with a heading.
    type section() =
        inherit RegularNode("section")
    /// Introductory content for a page or section.
    type header() =
        inherit RegularNode("header")
    /// Footer content for a page or section.
    type footer() =
        inherit RegularNode("footer")
    /// The dominant content of the document.
    type main() =
        inherit RegularNode("main")
    /// The top-level heading of a page or section.
    type h1() =
        inherit RegularNode("h1")
    /// A second-level heading.
    type h2() =
        inherit RegularNode("h2")
    /// A third-level heading.
    type h3() =
        inherit RegularNode("h3")
    /// A fourth-level heading.
    type h4() =
        inherit RegularNode("h4")
    /// A fifth-level heading.
    type h5() =
        inherit RegularNode("h5")
    /// A sixth-level heading.
    type h6() =
        inherit RegularNode("h6")
    /// An unordered list, typically displayed with bullets.
    type ul() =
        inherit RegularNode("ul")
    /// An ordered list whose items form a sequence.
    type ol() =
        inherit RegularNode("ol")
    /// An item in an ordered or unordered list.
    type li() =
        inherit RegularNode("li")
    /// A paragraph of text.
    type p() =
        inherit RegularNode("p")
    /// A generic inline container with no inherent semantic meaning.
    type span() =
        inherit RegularNode("span")
    /// Side comments or fine print.
    type small() =
        inherit RegularNode("small")
    /// Content with strong importance or urgency.
    type strong() =
        inherit RegularNode("strong")
    /// Text with stress emphasis.
    type em() =
        inherit RegularNode("em")
    /// A title or explanation for a table.
    type caption() =
        inherit RegularNode("caption")
    /// A section containing major navigation links.
    type nav() =
        inherit RegularNode("nav")
    /// A section containing search or filtering controls.
    type search() =
        inherit RegularNode("search")
    /// Text set apart from normal prose, such as a technical term.
    type i() =
        inherit RegularNode("i")
    /// Text stylistically offset from normal prose without added importance.
    type b() =
        inherit RegularNode("b")
    /// Text annotated as non-textual, such as a proper name.
    type u() =
        inherit RegularNode("u")
    /// Text that is no longer accurate or relevant.
    type s() =
        inherit RegularNode("s")
    /// Fallback content shown when scripts are unavailable.
    type noscript() =
        inherit RegularNode("noscript")
    /// A fragment of computer code.
    type code() =
        inherit RegularNode("code")
    /// Preformatted text whose whitespace is preserved.
    type pre() =
        inherit RegularNode("pre")
    /// A block quotation.
    type blockquote() =
        inherit RegularNode("blockquote")
    /// The title of a cited creative work.
    type cite() =
        inherit RegularNode("cite")
    /// A short inline quotation.
    type q() =
        inherit RegularNode("q")
    /// Contact information for the nearest page or article owner.
    type address() =
        inherit RegularNode("address")
    /// Content deleted from a document.
    type del() =
        inherit RegularNode("del")
    /// Content inserted into a document.
    type ins() =
        inherit RegularNode("ins")
    /// An abbreviation or acronym.
    type abbr() =
        inherit RegularNode("abbr")
    /// The defining instance of a term.
    type dfn() =
        inherit RegularNode("dfn")
    /// Subscript text.
    type sub() =
        inherit RegularNode("sub")
    /// Superscript text.
    type sup() =
        inherit RegularNode("sup")
    /// Inert content that can be instantiated later by script.
    type template() =
        inherit RegularNode("template")

    /// A line break in text.
    type br() =
        inherit VoidNode("br")

    /// A thematic break between sections.
    type hr() =
        inherit VoidNode("hr")

    /// A hyperlink or download link.
    type a() =
        inherit RegularNode("a")
        /// Destination URL.
        member this.href
            with set (value: string | null) = this.attr("href", value) |> ignore
        /// Language tag for the linked resource.
        member this.hreflang
            with set (value: string | null) = this.attr("hreflang", value) |> ignore
        /// Relationship to the destination or resource; use one or more link relation tokens.
        member this.rel
            with set (value: string | null) = this.attr("rel", value) |> ignore
        /// Browsing context: `_self`, `_blank`, `_parent`, `_top`, or a named context.
        member this.target
            with set (value: string | null) = this.attr("target", value) |> ignore
        /// Suggests downloading the resource; a string may specify the filename.
        member this.download
            with set (value: string | null) = this.attr("download", value) |> ignore
        /// Space-separated URLs to notify when the link is followed.
        member this.ping
            with set (value: string | null) = this.attr("ping", value) |> ignore
        /// MIME type of the linked resource as a hint to the browser.
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        /// Referrer behavior: `no-referrer`, `no-referrer-when-downgrade`, `origin`, `origin-when-cross-origin`, `same-origin`, `strict-origin`, `strict-origin-when-cross-origin`, or `unsafe-url`.
        member this.referrerpolicy
            with set (value: string | null) = this.attr("referrerpolicy", value) |> ignore

    /// Sets the base URL for relative URLs in the document.
    type base'() =
        inherit VoidNode("base")
        /// Destination URL.
        member this.href
            with set (value: string | null) = this.attr("href", value) |> ignore
        /// Browsing context: `_self`, `_blank`, `_parent`, `_top`, or a named context.
        member this.target
            with set (value: string | null) = this.attr("target", value) |> ignore

    /// Embeds an image; provide alternative text unless it is decorative.
    type img() =
        inherit VoidNode("img")
        /// URL of the media, image, script, or embedded resource.
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        /// Alternative text; use an empty string for decorative images.
        member this.alt
            with set (value: string | null) = this.attr("alt", value) |> ignore
        /// Width in CSS pixels.
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        /// Height in CSS pixels.
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore
        /// Candidate image URLs with width or pixel-density descriptors.
        member this.srcset
            with set (value: string | null) = this.attr("srcset", value) |> ignore
        /// Referrer behavior: `no-referrer`, `no-referrer-when-downgrade`, `origin`, `origin-when-cross-origin`, `same-origin`, `strict-origin`, `strict-origin-when-cross-origin`, or `unsafe-url`.
        member this.referrerpolicy
            with set (value: string | null) = this.attr("referrerpolicy", value) |> ignore
        /// CORS mode: `anonymous`, `use-credentials`, or an empty string for anonymous mode.
        member this.crossorigin
            with set (value: string | null) = this.attr("crossorigin", value) |> ignore
        /// Image slot sizes used with a responsive srcset.
        member this.sizes
            with set (value: string | null) = this.attr("sizes", value) |> ignore
        /// Map association using a fragment URL such as `#map-name`.
        member this.usemap
            with set (value: string | null) = this.attr("usemap", value) |> ignore
        /// Whether this linked image acts as a server-side image map.
        member this.ismap
            with set (value: bool) = this.bool("ismap", value) |> ignore
        /// Image decoding hint: `async`, `sync`, or `auto`.
        member this.decoding
            with set (value: string | null) = this.attr("decoding", value) |> ignore
        /// Loading hint: `eager` or `lazy`.
        member this.loading
            with set (value: string | null) = this.attr("loading", value) |> ignore
        /// Fetch priority hint: `high`, `low`, or `auto`.
        member this.fetchpriority
            with set (value: string | null) = this.attr("fetchpriority", value) |> ignore
        /// Identifier used to observe element rendering performance.
        member this.elementtiming
            with set (value: string | null) = this.attr("elementtiming", value) |> ignore

    /// A form for submitting user-provided data.
    type form() =
        inherit RegularNode("form")
        /// URL that receives submitted form data.
        member this.action
            with set (value: string | null) = this.attr("action", value) |> ignore
        /// Submission method: `get`, `post`, or `dialog`.
        member this.method
            with set (value: string | null) = this.attr("method", value) |> ignore
        /// Form encoding: `application/x-www-form-urlencoded`, `multipart/form-data`, or `text/plain`.
        member this.enctype
            with set (value: string | null) = this.attr("enctype", value) |> ignore
        /// Browsing context: `_self`, `_blank`, `_parent`, `_top`, or a named context.
        member this.target
            with set (value: string | null) = this.attr("target", value) |> ignore
        /// Character encodings accepted by the form, usually `UTF-8`.
        member this.acceptCharset
            with set (value: string | null) = this.attr("accept-charset", value) |> ignore
        /// Autofill hint; use `on`, `off`, or a recognized token such as `email`, `name`, or `postal-code`.
        member this.autocomplete
            with set (value: string | null) = this.attr("autocomplete", value) |> ignore
        /// Element name, used for form submission or script access as applicable.
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        /// Whether to skip constraint validation when submitting the form.
        member this.novalidate
            with set (value: bool) = this.bool("novalidate", value) |> ignore
        /// Relationship to the destination or resource; use one or more link relation tokens.
        member this.rel
            with set (value: string | null) = this.attr("rel", value) |> ignore

    /// Embeds or references a script or data block.
    type script() =
        inherit RegularNode("script")
        /// URL of the media, image, script, or embedded resource.
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        /// Script type: `module` for modules, a JavaScript MIME type for classic scripts, or `importmap`/`speculationrules` for those data blocks.
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        /// Whether an external script executes asynchronously when available.
        member this.async
            with set (value: bool) = this.bool("async", value) |> ignore
        /// Whether an external classic script waits until parsing finishes.
        member this.defer
            with set (value: bool) = this.bool("defer", value) |> ignore
        /// Subresource Integrity hash used to verify fetched content.
        member this.integrity
            with set (value: string | null) = this.attr("integrity", value) |> ignore
        /// CORS mode: `anonymous`, `use-credentials`, or an empty string for anonymous mode.
        member this.crossorigin
            with set (value: string | null) = this.attr("crossorigin", value) |> ignore
        /// Prevents execution in browsers that support JavaScript modules.
        member this.nomodule
            with set (value: bool) = this.bool("nomodule", value) |> ignore
        /// Content Security Policy nonce for an inline script or style.
        member this.nonce
            with set (value: string | null) = this.attr("nonce", value) |> ignore
        /// Referrer behavior: `no-referrer`, `no-referrer-when-downgrade`, `origin`, `origin-when-cross-origin`, `same-origin`, `strict-origin`, `strict-origin-when-cross-origin`, or `unsafe-url`.
        member this.referrerpolicy
            with set (value: string | null) = this.attr("referrerpolicy", value) |> ignore

    /// Links to an external resource, commonly a stylesheet or icon.
    type link() =
        inherit VoidNode("link")
        /// Relationship to the destination or resource; use one or more link relation tokens.
        member this.rel
            with set (value: string | null) = this.attr("rel", value) |> ignore
        /// Destination URL.
        member this.href
            with set (value: string | null) = this.attr("href", value) |> ignore
        /// MIME type of the linked resource, for example `text/css` for a stylesheet.
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        /// Media condition for which a resource applies.
        member this.media
            with set (value: string | null) = this.attr("media", value) |> ignore
        /// Destination resource type for a preload link, such as `script`, `style`, `image`, or `font`.
        member this.as'
            with set (value: string | null) = this.attr("as", value) |> ignore
        /// Image slot sizes used with a responsive srcset.
        member this.sizes
            with set (value: string | null) = this.attr("sizes", value) |> ignore
        /// CORS mode: `anonymous`, `use-credentials`, or an empty string for anonymous mode.
        member this.crossorigin
            with set (value: string | null) = this.attr("crossorigin", value) |> ignore
        /// Subresource Integrity hash used to verify fetched content.
        member this.integrity
            with set (value: string | null) = this.attr("integrity", value) |> ignore
        /// Referrer behavior: `no-referrer`, `no-referrer-when-downgrade`, `origin`, `origin-when-cross-origin`, `same-origin`, `strict-origin`, `strict-origin-when-cross-origin`, or `unsafe-url`.
        member this.referrerpolicy
            with set (value: string | null) = this.attr("referrerpolicy", value) |> ignore
        /// Disables a linked stylesheet when set to true.
        member this.disabled
            with set (value: bool) = this.bool("disabled", value) |> ignore
        /// Language tag for the linked resource.
        member this.hreflang
            with set (value: string | null) = this.attr("hreflang", value) |> ignore
        /// Responsive image sizes for a preload link.
        member this.imagesizes
            with set (value: string | null) = this.attr("imagesizes", value) |> ignore
        /// Responsive image candidates for a preload link.
        member this.imagesrcset
            with set (value: string | null) = this.attr("imagesrcset", value) |> ignore
        /// Advisory text, commonly shown as a tooltip.
        member this.title
            with set (value: string | null) = this.attr("title", value) |> ignore


    /// The root element of an HTML document.
    type html() =
        inherit RegularNode("html")
        /// XML namespace URI; for HTML use `http://www.w3.org/1999/xhtml`.
        member this.xmlns
            with set (value: string | null) = this.attr("xmlns", value) |> ignore

    /// Provides metadata about the document.
    type meta() =
        inherit VoidNode("meta")
        /// Identifies the kind of metadata provided, such as `description` or `author`.
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        /// Value associated with a meta name or http-equiv.
        member this.content
            with set (value: string | null) = this.attr("content", value) |> ignore
        /// Character encoding; use `UTF-8` for HTML5 documents.
        member this.charset
            with set (value: string | null) = this.attr("charset", value) |> ignore
        /// HTTP pragma such as `refresh` or `content-security-policy`.
        member this.httpEquiv
            with set (value: string | null) = this.attr("http-equiv", value) |> ignore

    /// An interactive form control; its type determines its behavior.
    type input() =
        inherit VoidNode("input")
        /// Input control type: `hidden`, `text`, `search`, `tel`, `url`, `email`, `password`, `date`, `month`, `week`, `time`, `datetime-local`, `number`, `range`, `color`, `checkbox`, `radio`, `file`, `submit`, `image`, `reset`, or `button`.
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        /// Element name, used for form submission or script access as applicable.
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        /// Submitted or machine-readable value.
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore
        /// Short hint shown when a control has no value.
        member this.placeholder
            with set (value: string | null) = this.attr("placeholder", value) |> ignore
        /// Whether a value is required before form submission.
        member this.required
            with set (value: bool) = this.bool("required", value) |> ignore
        /// Requests focus when the page or dialog loads.
        member this.autofocus
            with set (value: bool) = this.bool("autofocus", value) |> ignore
        /// Autofill hint; use `on`, `off`, or a recognized token such as `email`, `name`, or `postal-code`.
        member this.autocomplete
            with set (value: string | null) = this.attr("autocomplete", value) |> ignore
        /// Minimum permitted value.
        member this.min
            with set (value: string | null) = this.attr("min", value) |> ignore
        /// Maximum permitted value.
        member this.max
            with set (value: string | null) = this.attr("max", value) |> ignore
        /// Permitted step interval.
        member this.step
            with set (value: string | null) = this.attr("step", value) |> ignore
        /// Regular expression that the input value must match.
        member this.pattern
            with set (value: string | null) = this.attr("pattern", value) |> ignore
        /// Prevents editing while allowing the value to be submitted.
        member this.readonly
            with set (value: bool) = this.bool("readonly", value) |> ignore
        /// Disables the control and excludes it from form submission.
        member this.disabled
            with set (value: bool) = this.bool("disabled", value) |> ignore
        /// Whether multiple values can be selected or entered.
        member this.multiple
            with set (value: bool) = this.bool("multiple", value) |> ignore
        /// Accepted file MIME types or extensions, separated by commas.
        member this.accept
            with set (value: string | null) = this.attr("accept", value) |> ignore
        /// Id of a datalist providing suggestions.
        member this.list
            with set (value: string | null) = this.attr("list", value) |> ignore
        /// Maximum number of characters allowed.
        member this.maxlength
            with set (value: int) = this.attr("maxlength", string value) |> ignore
        /// Minimum number of characters required.
        member this.minlength
            with set (value: int) = this.attr("minlength", string value) |> ignore
        /// Visible control size or, for select, number of visible options.
        member this.size
            with set (value: int) = this.attr("size", string value) |> ignore
        /// URL of the media, image, script, or embedded resource.
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        /// Width in CSS pixels.
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        /// Height in CSS pixels.
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore
        /// Alternative text; use an empty string for decorative images.
        member this.alt
            with set (value: string | null) = this.attr("alt", value) |> ignore
        /// Initial checked state of a checkbox or radio input.
        member this.checked'
            with set (value: bool) = this.bool("checked", value) |> ignore
        /// Name of the form field used to submit text direction.
        member this.dirname
            with set (value: string | null) = this.attr("dirname", value) |> ignore
        /// Id of the form associated with this control.
        member this.form
            with set (value: string | null) = this.attr("form", value) |> ignore
        /// Overrides the form submission URL for this submit control.
        member this.formaction
            with set (value: string | null) = this.attr("formaction", value) |> ignore
        /// Form encoding override: `application/x-www-form-urlencoded`, `multipart/form-data`, or `text/plain`.
        member this.formenctype
            with set (value: string | null) = this.attr("formenctype", value) |> ignore
        /// Form method override: `get`, `post`, or `dialog`.
        member this.formmethod
            with set (value: string | null) = this.attr("formmethod", value) |> ignore
        /// Whether to submit without constraint validation.
        member this.formnovalidate
            with set (value: bool) = this.bool("formnovalidate", value) |> ignore
        /// Browsing context that receives the form response.
        member this.formtarget
            with set (value: string | null) = this.attr("formtarget", value) |> ignore
        /// Virtual keyboard hint: `none`, `text`, `decimal`, `numeric`, `tel`, `search`, `email`, or `url`.
        member this.inputmode
            with set (value: string | null) = this.attr("inputmode", value) |> ignore
        /// Capture hint for file inputs: `user`, `environment`, or an empty string.
        member this.capture
            with set (value: string | null) = this.attr("capture", value) |> ignore



    /// Displays the result of a calculation or user action.
    type output() =
        inherit RegularNode("output")
        /// Id of the associated element or elements.
        member this.for'
            with set (value: string | null) = this.attr("for", value) |> ignore
        /// Id of the form associated with this control.
        member this.form
            with set (value: string | null) = this.attr("form", value) |> ignore
        /// Element name, used for form submission or script access as applicable.
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore

    /// A multiline text-entry control.
    type textarea() =
        inherit RegularNode("textarea")
        /// Element name, used for form submission or script access as applicable.
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        /// Short hint shown when a control has no value.
        member this.placeholder
            with set (value: string | null) = this.attr("placeholder", value) |> ignore
        /// Whether a value is required before form submission.
        member this.required
            with set (value: bool) = this.bool("required", value) |> ignore
        /// Autofill hint; use `on`, `off`, or a recognized token such as `email`, `name`, or `postal-code`.
        member this.autocomplete
            with set (value: string | null) = this.attr("autocomplete", value) |> ignore
        /// Requests focus when the page or dialog loads.
        member this.autofocus
            with set (value: bool) = this.bool("autofocus", value) |> ignore
        /// Prevents editing while allowing the value to be submitted.
        member this.readonly
            with set (value: bool) = this.bool("readonly", value) |> ignore
        /// Disables the control and excludes it from form submission.
        member this.disabled
            with set (value: bool) = this.bool("disabled", value) |> ignore
        /// Number of visible text lines or rows, depending on the element.
        member this.rows
            with set (value: int) = this.attr("rows", string value) |> ignore
        /// Visible textarea width in average character widths.
        member this.cols
            with set (value: int) = this.attr("cols", string value) |> ignore
        /// Textarea line wrapping mode: `soft`, `hard`, or `off`.
        member this.wrap
            with set (value: string | null) = this.attr("wrap", value) |> ignore
        /// Maximum number of characters allowed.
        member this.maxlength
            with set (value: int) = this.attr("maxlength", string value) |> ignore
        /// Minimum number of characters required.
        member this.minlength
            with set (value: int) = this.attr("minlength", string value) |> ignore
        /// Name of the form field used to submit text direction.
        member this.dirname
            with set (value: string | null) = this.attr("dirname", value) |> ignore
        /// Id of the form associated with this control.
        member this.form
            with set (value: string | null) = this.attr("form", value) |> ignore

    /// A clickable button for form submission or actions.
    type button() =
        inherit RegularNode("button")
        /// Button behavior: `submit`, `reset`, or `button`.
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        /// Element name, used for form submission or script access as applicable.
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        /// Submitted or machine-readable value.
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore
        /// Disables the control and excludes it from form submission.
        member this.disabled
            with set (value: bool) = this.bool("disabled", value) |> ignore
        /// Requests focus when the page or dialog loads.
        member this.autofocus
            with set (value: bool) = this.bool("autofocus", value) |> ignore
        /// Id of the form associated with this control.
        member this.form
            with set (value: string | null) = this.attr("form", value) |> ignore
        /// Overrides the form submission URL for this submit control.
        member this.formaction
            with set (value: string | null) = this.attr("formaction", value) |> ignore
        /// Form encoding override: `application/x-www-form-urlencoded`, `multipart/form-data`, or `text/plain`.
        member this.formenctype
            with set (value: string | null) = this.attr("formenctype", value) |> ignore
        /// Form method override: `get`, `post`, or `dialog`.
        member this.formmethod
            with set (value: string | null) = this.attr("formmethod", value) |> ignore
        /// Whether to submit without constraint validation.
        member this.formnovalidate
            with set (value: bool) = this.bool("formnovalidate", value) |> ignore
        /// Browsing context that receives the form response.
        member this.formtarget
            with set (value: string | null) = this.attr("formtarget", value) |> ignore
        /// Id of the popover controlled by this button.
        member this.popovertarget
            with set (value: string | null) = this.attr("popovertarget", value) |> ignore
        /// Popover action: `toggle`, `show`, or `hide`.
        member this.popovertargetaction
            with set (value: string | null) = this.attr("popovertargetaction", value) |> ignore

    /// A control for choosing one or more options.
    type select() =
        inherit RegularNode("select")
        /// Element name, used for form submission or script access as applicable.
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        /// Whether a value is required before form submission.
        member this.required
            with set (value: bool) = this.bool("required", value) |> ignore
        /// Requests focus when the page or dialog loads.
        member this.autofocus
            with set (value: bool) = this.bool("autofocus", value) |> ignore
        /// Autofill hint; use `on`, `off`, or a recognized token such as `email`, `name`, or `postal-code`.
        member this.autocomplete
            with set (value: string | null) = this.attr("autocomplete", value) |> ignore
        /// Disables the control and excludes it from form submission.
        member this.disabled
            with set (value: bool) = this.bool("disabled", value) |> ignore
        /// Whether multiple values can be selected or entered.
        member this.multiple
            with set (value: bool) = this.bool("multiple", value) |> ignore
        /// Visible control size or, for select, number of visible options.
        member this.size
            with set (value: int) = this.attr("size", string value) |> ignore
        /// Id of the form associated with this control.
        member this.form
            with set (value: string | null) = this.attr("form", value) |> ignore

    /// A selectable choice in a select, optgroup, or datalist.
    type option() =
        inherit RegularNode("option")
        /// Submitted or machine-readable value.
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore
        /// Whether this option is initially selected.
        member this.selected
            with set (value: bool) = this.bool("selected", value) |> ignore
        /// Disables the control and excludes it from form submission.
        member this.disabled
            with set (value: bool) = this.bool("disabled", value) |> ignore
        /// Human-readable label for this option or group.
        member this.label
            with set (value: string | null) = this.attr("label", value) |> ignore

    /// Groups related options in a select control.
    type optgroup() =
        inherit RegularNode("optgroup")
        /// Human-readable label for this option or group.
        member this.label
            with set (value: string | null) = this.attr("label", value) |> ignore
        /// Disables the control and excludes it from form submission.
        member this.disabled
            with set (value: bool) = this.bool("disabled", value) |> ignore

    /// A caption for a form control.
    type label() =
        inherit RegularNode("label")
        /// Id of the associated element or elements.
        member this.for'
            with set (value: string | null) = this.attr("for", value) |> ignore

    /// Contains CSS rules that apply to the document.
    type style() =
        inherit RegularNode("style")
        /// MIME type of the embedded style; use `text/css` (the HTML default).
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        /// Media condition for which a resource applies.
        member this.media
            with set (value: string | null) = this.attr("media", value) |> ignore

    /// Embeds another HTML page as a nested browsing context.
    type iframe() =
        inherit RegularNode("iframe")
        /// URL of the media, image, script, or embedded resource.
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        /// Name of the nested browsing context, usable as a link or form target.
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        /// Iframe restrictions; empty means maximum restrictions, otherwise use space-separated tokens such as `allow-scripts` and `allow-forms`.
        member this.sandbox
            with set (value: string | null) = this.attr("sandbox", value) |> ignore
        /// Width in CSS pixels.
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        /// Height in CSS pixels.
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore
        /// Permissions Policy features allowed for the embedded iframe page.
        member this.allow
            with set (value: string | null) = this.attr("allow", value) |> ignore
        /// Loading hint: `eager` or `lazy`.
        member this.loading
            with set (value: string | null) = this.attr("loading", value) |> ignore
        /// Referrer behavior: `no-referrer`, `no-referrer-when-downgrade`, `origin`, `origin-when-cross-origin`, `same-origin`, `strict-origin`, `strict-origin-when-cross-origin`, or `unsafe-url`.
        member this.referrerpolicy
            with set (value: string | null) = this.attr("referrerpolicy", value) |> ignore
        /// Inline HTML content for the iframe.
        member this.srcdoc
            with set (value: string | null) = this.attr("srcdoc", value) |> ignore

    /// Embeds video content.
    type video() =
        inherit RegularNode("video")
        /// URL of the media, image, script, or embedded resource.
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        /// Image URL shown before video playback.
        member this.poster
            with set (value: string | null) = this.attr("poster", value) |> ignore
        /// Requests automatic playback; browsers may restrict autoplay.
        member this.autoplay
            with set (value: bool) = this.bool("autoplay", value) |> ignore
        /// Whether to show native media playback controls.
        member this.controls
            with set (value: bool) = this.bool("controls", value) |> ignore
        /// Allows video playback inline instead of automatically entering fullscreen.
        member this.playsinline
            with set (value: bool) = this.bool("playsinline", value) |> ignore
        /// Media control hints: `nodownload`, `nofullscreen`, or `noremoteplayback`.
        member this.controlsList
            with set (value: string | null) = this.attr("controlsList", value) |> ignore
        /// CORS mode: `anonymous`, `use-credentials`, or an empty string for anonymous mode.
        member this.crossorigin
            with set (value: string | null) = this.attr("crossorigin", value) |> ignore
        /// Whether playback restarts at the end.
        member this.loop
            with set (value: bool) = this.bool("loop", value) |> ignore
        /// Whether media starts muted.
        member this.muted
            with set (value: bool) = this.bool("muted", value) |> ignore
        /// Width in CSS pixels.
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        /// Height in CSS pixels.
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore
        /// Preload hint: `none`, `metadata`, or `auto`.
        member this.preload
            with set (value: string | null) = this.attr("preload", value) |> ignore
        /// Disables remote playback UI for media.
        member this.disableremoteplayback
            with set (value: bool) = this.bool("disableremoteplayback", value) |> ignore
        /// Disables the picture-in-picture control where supported.
        member this.disablepictureinpicture
            with set (value: bool) = this.bool("disablepictureinpicture", value) |> ignore

    /// Embeds audio content.
    type audio() =
        inherit RegularNode("audio")
        /// URL of the media, image, script, or embedded resource.
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        /// Requests automatic playback; browsers may restrict autoplay.
        member this.autoplay
            with set (value: bool) = this.bool("autoplay", value) |> ignore
        /// Whether to show native media playback controls.
        member this.controls
            with set (value: bool) = this.bool("controls", value) |> ignore
        /// Media control hints: `nodownload`, `nofullscreen`, or `noremoteplayback`.
        member this.controlsList
            with set (value: string | null) = this.attr("controlsList", value) |> ignore
        /// CORS mode: `anonymous`, `use-credentials`, or an empty string for anonymous mode.
        member this.crossorigin
            with set (value: string | null) = this.attr("crossorigin", value) |> ignore
        /// Preload hint: `none`, `metadata`, or `auto`.
        member this.preload
            with set (value: string | null) = this.attr("preload", value) |> ignore
        /// Whether playback restarts at the end.
        member this.loop
            with set (value: bool) = this.bool("loop", value) |> ignore
        /// Whether media starts muted.
        member this.muted
            with set (value: bool) = this.bool("muted", value) |> ignore
        /// Disables remote playback UI for media.
        member this.disableremoteplayback
            with set (value: bool) = this.bool("disableremoteplayback", value) |> ignore

    /// Provides a media or image source.
    type source() =
        inherit VoidNode("source")
        /// URL of the media, image, script, or embedded resource.
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        /// MIME type of the media source, for example `audio/ogg` or `video/mp4`.
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        /// Media condition for which a resource applies.
        member this.media
            with set (value: string | null) = this.attr("media", value) |> ignore
        /// Image slot sizes used with a responsive srcset.
        member this.sizes
            with set (value: string | null) = this.attr("sizes", value) |> ignore
        /// Candidate image URLs with width or pixel-density descriptors.
        member this.srcset
            with set (value: string | null) = this.attr("srcset", value) |> ignore

    /// A scriptable bitmap drawing surface.
    type canvas() =
        inherit RegularNode("canvas")
        /// Width in CSS pixels.
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        /// Height in CSS pixels.
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore

    /// Embeds an external resource.
    type object'() =
        inherit RegularNode("object")
        /// URL of the resource embedded by this object element.
        member this.data
            with set (value: string | null) = this.attr("data", value) |> ignore
        /// MIME type of the embedded resource.
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        /// Width in CSS pixels.
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        /// Height in CSS pixels.
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore
        /// Id of the form associated with this control.
        member this.form
            with set (value: string | null) = this.attr("form", value) |> ignore
        /// Names the nested browsing context created by this object.
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore

    /// Provides a parameter for an object element.
    type param() =
        inherit VoidNode("param")
        /// Name of the parameter passed to the embedded object.
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
        /// Submitted or machine-readable value.
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore

    /// Associates machine-readable data with human-readable content.
    type data() =
        inherit RegularNode("data")
        /// Submitted or machine-readable value.
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore

    /// Represents a date, time, or duration.
    type time() =
        inherit RegularNode("time")
        /// Machine-readable date, time, or duration.
        member this.datetime
            with set (value: string | null) = this.attr("datetime", value) |> ignore

    /// Displays the completion progress of a task.
    type progress() =
        inherit RegularNode("progress")
        /// Submitted or machine-readable value.
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore
        /// Maximum permitted value.
        member this.max
            with set (value: string | null) = this.attr("max", value) |> ignore

    /// Displays a scalar measurement within a known range.
    type meter() =
        inherit RegularNode("meter")
        /// Id of the form associated with this control.
        member this.form
            with set (value: string | null) = this.attr("form", value) |> ignore
        /// Submitted or machine-readable value.
        member this.value
            with set (value: string | null) = this.attr("value", value) |> ignore
        /// Minimum permitted value.
        member this.min
            with set (value: string | null) = this.attr("min", value) |> ignore
        /// Maximum permitted value.
        member this.max
            with set (value: string | null) = this.attr("max", value) |> ignore
        /// Lower threshold of a meter's low range.
        member this.low
            with set (value: string | null) = this.attr("low", value) |> ignore
        /// Upper threshold of a meter's high range.
        member this.high
            with set (value: string | null) = this.attr("high", value) |> ignore
        /// Optimal value within the meter's range.
        member this.optimum
            with set (value: string | null) = this.attr("optimum", value) |> ignore

    /// A disclosure widget that can show or hide its contents.
    type details() =
        inherit RegularNode("details")
        /// Whether the details disclosure or dialog is open.
        member this.open'
            with set (value: bool) = this.bool("open", value) |> ignore

    /// The visible label for a details disclosure widget.
    type summary() =
        inherit RegularNode("summary")

    /// A dialog or other interactive component.
    type dialog() =
        inherit RegularNode("dialog")
        /// Whether the details disclosure or dialog is open.
        member this.open'
            with set (value: bool) = this.bool("open", value) |> ignore

    /// A list of commands or menu items.
    type menu() =
        inherit RegularNode("menu")

    /// Provides suggested options for an input control.
    type datalist() =
        inherit RegularNode("datalist")

    /// Groups related form controls.
    type fieldset() =
        inherit RegularNode("fieldset")
        /// Disables the control and excludes it from form submission.
        member this.disabled
            with set (value: bool) = this.bool("disabled", value) |> ignore
        /// Id of the form associated with this control.
        member this.form
            with set (value: string | null) = this.attr("form", value) |> ignore
        /// Element name, used for form submission or script access as applicable.
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore

    /// A caption for a fieldset.
    type legend() =
        inherit RegularNode("legend")

    /// Tabular data arranged in rows and columns.
    type table() =
        inherit RegularNode("table")
    /// Groups the body rows of a table.
    type tbody() =
        inherit RegularNode("tbody")
    /// Groups the header rows of a table.
    type thead() =
        inherit RegularNode("thead")
    /// Groups the footer rows of a table.
    type tfoot() =
        inherit RegularNode("tfoot")
    /// A row of table cells.
    type tr() =
        inherit RegularNode("tr")
    /// A header cell in a table.
    type th() =
        inherit RegularNode("th")
        /// Abbreviated description of a table header cell.
        member this.abbr
            with set (value: string | null) = this.attr("abbr", value) |> ignore
        /// Number of table columns spanned; must be a positive integer.
        member this.colspan
            with set (value: int) = this.attr("colspan", string value) |> ignore
        /// Number of table rows spanned; `0` spans the remaining rows in the row group.
        member this.rowspan
            with set (value: int) = this.attr("rowspan", string value) |> ignore
        /// Ids of header cells that apply to this table cell.
        member this.headers
            with set (value: string | null) = this.attr("headers", value) |> ignore
        /// Header scope: `row`, `col`, `rowgroup`, or `colgroup`.
        member this.scope
            with set (value: string | null) = this.attr("scope", value) |> ignore
    /// A data cell in a table.
    type td() =
        inherit RegularNode("td")
        /// Number of table columns spanned; must be a positive integer.
        member this.colspan
            with set (value: int) = this.attr("colspan", string value) |> ignore
        /// Number of table rows spanned; `0` spans the remaining rows in the row group.
        member this.rowspan
            with set (value: int) = this.attr("rowspan", string value) |> ignore
        /// Ids of header cells that apply to this table cell.
        member this.headers
            with set (value: string | null) = this.attr("headers", value) |> ignore

    /// Defines an image map containing clickable areas.
    type map() =
        inherit RegularNode("map")
        /// Name of the image map referenced by an image usemap attribute.
        member this.name
            with set (value: string | null) = this.attr("name", value) |> ignore
    /// A clickable region in an image map.
    type area() =
        inherit VoidNode("area")
        /// Image-map area shape: `rect`, `circle`, `poly`, or `default`.
        member this.shape
            with set (value: string | null) = this.attr("shape", value) |> ignore
        /// Comma-separated pixel coordinates for the image-map area.
        member this.coords
            with set (value: string | null) = this.attr("coords", value) |> ignore
        /// Destination URL.
        member this.href
            with set (value: string | null) = this.attr("href", value) |> ignore
        /// Alternative text; use an empty string for decorative images.
        member this.alt
            with set (value: string | null) = this.attr("alt", value) |> ignore
        /// Suggests downloading the resource; a string may specify the filename.
        member this.download
            with set (value: string | null) = this.attr("download", value) |> ignore
        /// Browsing context: `_self`, `_blank`, `_parent`, `_top`, or a named context.
        member this.target
            with set (value: string | null) = this.attr("target", value) |> ignore
        /// Relationship to the destination or resource; use one or more link relation tokens.
        member this.rel
            with set (value: string | null) = this.attr("rel", value) |> ignore
        /// Referrer behavior: `no-referrer`, `no-referrer-when-downgrade`, `origin`, `origin-when-cross-origin`, `same-origin`, `strict-origin`, `strict-origin-when-cross-origin`, or `unsafe-url`.
        member this.referrerpolicy
            with set (value: string | null) = this.attr("referrerpolicy", value) |> ignore
        /// Space-separated URLs to notify when the link is followed.
        member this.ping
            with set (value: string | null) = this.attr("ping", value) |> ignore

    /// Content related indirectly to surrounding content, such as a sidebar.
    type aside() =
        inherit RegularNode("aside")
    /// Isolates text with directionality that may differ from surrounding text.
    type bdi() =
        inherit RegularNode("bdi")
    /// Overrides the directionality of its text.
    type bdo() =
        inherit RegularNode("bdo")
    /// Applies properties to one or more table columns.
    type col() =
        inherit VoidNode("col")
        /// Number of table columns represented; use a positive integer.
        member this.span
            with set (value: int) = this.attr("span", string value) |> ignore
    /// Groups table columns for shared properties.
    type colgroup() =
        inherit RegularNode("colgroup")
        /// Number of table columns represented; use a positive integer.
        member this.span
            with set (value: int) = this.attr("span", string value) |> ignore
    /// The description or value associated with a term.
    type dd() =
        inherit RegularNode("dd")
    /// A description list of terms and their descriptions.
    type dl() =
        inherit RegularNode("dl")
    /// A term or name in a description list.
    type dt() =
        inherit RegularNode("dt")
    /// Embeds external content.
    type embed() =
        inherit VoidNode("embed")
        /// URL of the media, image, script, or embedded resource.
        member this.src
            with set (value: string | null) = this.attr("src", value) |> ignore
        /// MIME type of the embedded resource.
        member this.type'
            with set (value: string | null) = this.attr("type", value) |> ignore
        /// Width in CSS pixels.
        member this.width
            with set (value: int) = this.attr("width", string value) |> ignore
        /// Height in CSS pixels.
        member this.height
            with set (value: int) = this.attr("height", string value) |> ignore
    /// A caption or legend for a figure.
    type figcaption() =
        inherit RegularNode("figcaption")
    /// Self-contained content such as an illustration or diagram.
    type figure() =
        inherit RegularNode("figure")
    /// Text representing user input, commonly keyboard input.
    type kbd() =
        inherit RegularNode("kbd")
    /// Text highlighted for relevance.
    type mark() =
        inherit RegularNode("mark")
    /// Provides responsive image sources and an img fallback.
    type picture() =
        inherit RegularNode("picture")
    /// Sample output from a computer program.
    type samp() =
        inherit RegularNode("samp")
    /// A variable in a mathematical or programming context.
    type var() =
        inherit RegularNode("var")
    /// An optional line-break opportunity within text.
    type wbr() =
        inherit VoidNode("wbr")
