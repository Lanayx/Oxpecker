namespace Oxpecker.Datastar

open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices
open Oxpecker.ViewEngine


/// Attributes that declare reactive signals.
type DatastarSignalExtensions =

    /// Patches (adds, updates or removes) one or more signals.
    /// Object/expression form renders `data-signals="…"`.
    [<Extension>]
    static member dataSignals(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-signals", value)

    /// Patches a single signal. Key form renders `data-signals:{name}="…"`.
    [<Extension>]
    static member dataSignals(this: #HtmlTag, name: string, [<StringSyntax("js")>] value: string | null) =
        this.attr ($"data-signals:%s{name}", value)

    /// Patches a single signal with modifiers. Renders `data-signals:{name}{modifiers}="…"`.
    /// `modifiers` is appended verbatim and must include the leading `__` (see `DsCase` and `DsModifier`).
    [<Extension>]
    static member dataSignals
        (this: #HtmlTag, name: string, [<StringSyntax("js")>] value: string | null, modifiers: string)
        =
        this.attr ($"data-signals:%s{name}%s{modifiers}", value)

    /// Creates one or more computed signals.
    /// Object/expression form renders `data-computed="…"`.
    [<Extension>]
    static member dataComputed(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-computed", value)

    /// Creates a computed signal. Key form renders `data-computed:{name}="…"`.
    [<Extension>]
    static member dataComputed(this: #HtmlTag, name: string, [<StringSyntax("js")>] value: string | null) =
        this.attr ($"data-computed:%s{name}", value)

    /// Creates a computed signal with modifiers. Renders `data-computed:{name}{modifiers}="…"`.
    [<Extension>]
    static member dataComputed
        (this: #HtmlTag, name: string, [<StringSyntax("js")>] value: string | null, modifiers: string)
        =
        this.attr ($"data-computed:%s{name}%s{modifiers}", value)

    /// Creates a signal that references the element. Renders `data-ref:{name}`.
    [<Extension>]
    static member dataRef(this: #HtmlTag, name: string) = this.bool ($"data-ref:%s{name}", true)

    /// Creates a signal that references the element using the value form. Renders `data-ref="{name}"`.
    [<Extension>]
    static member dataRefValue(this: #HtmlTag, name: string | null) = this.attr ("data-ref", name)

    /// Creates a reference signal with modifiers. Renders `data-ref:{name}{modifiers}`.
    [<Extension>]
    static member dataRef(this: #HtmlTag, name: string, modifiers: string) =
        this.bool ($"data-ref:%s{name}%s{modifiers}", true)

    /// Creates a boolean indicator signal that is `true` while a fetch is in flight. Renders `data-indicator:{name}`.
    [<Extension>]
    static member dataIndicator(this: #HtmlTag, name: string) =
        this.bool ($"data-indicator:%s{name}", true)

    /// Creates an indicator signal using the value form. Renders `data-indicator="{name}"`.
    [<Extension>]
    static member dataIndicatorValue(this: #HtmlTag, name: string | null) = this.attr ("data-indicator", name)

    /// Creates an indicator signal with modifiers. Renders `data-indicator:{name}{modifiers}`.
    [<Extension>]
    static member dataIndicator(this: #HtmlTag, name: string, modifiers: string) =
        this.bool ($"data-indicator:%s{name}%s{modifiers}", true)

    /// Two-way binds an element to a signal. Renders `data-bind:{name}`.
    [<Extension>]
    static member dataBind(this: #HtmlTag, name: string) = this.bool ($"data-bind:%s{name}", true)

    /// Two-way binds an element to a signal using the value form. Renders `data-bind="{name}"`.
    [<Extension>]
    static member dataBindValue(this: #HtmlTag, name: string | null) = this.attr ("data-bind", name)

    /// Two-way binds an element to a signal with modifiers. Renders `data-bind:{name}{modifiers}`.
    /// Common modifiers are `DsCase.*`, `DsModifier.bindProp` and `DsModifier.bindEvent`.
    [<Extension>]
    static member dataBind(this: #HtmlTag, name: string, modifiers: string) =
        this.bool ($"data-bind:%s{name}%s{modifiers}", true)


/// Attributes that keep the DOM in sync with signals.
type DatastarDomExtensions =

    /// Sets one or more HTML attributes reactively.
    /// Object form renders `data-attr="…"`.
    [<Extension>]
    static member dataAttr(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr ("data-attr", value)

    /// Sets a single HTML attribute reactively. Renders `data-attr:{name}="…"`.
    [<Extension>]
    static member dataAttr(this: #HtmlTag, name: string, [<StringSyntax("js")>] value: string | null) =
        this.attr ($"data-attr:%s{name}", value)

    /// Sets one or more inline CSS styles reactively.
    /// Object form renders `data-style="…"`.
    [<Extension>]
    static member dataStyle(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-style", value)

    /// Sets a single inline CSS property reactively. Renders `data-style:{property}="…"`.
    [<Extension>]
    static member dataStyle(this: #HtmlTag, property: string, [<StringSyntax("js")>] value: string | null) =
        this.attr ($"data-style:%s{property}", value)

    /// Adds or removes one or more CSS classes reactively.
    /// Object form renders `data-class="…"`.
    [<Extension>]
    static member dataClass(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-class", value)

    /// Adds or removes a single CSS class reactively. Renders `data-class:{name}="…"`.
    [<Extension>]
    static member dataClass(this: #HtmlTag, name: string, [<StringSyntax("js")>] value: string | null) =
        this.attr ($"data-class:%s{name}", value)

    /// Adds or removes a single CSS class with modifiers. Renders `data-class:{name}{modifiers}="…"`.
    [<Extension>]
    static member dataClass
        (this: #HtmlTag, name: string, [<StringSyntax("js")>] value: string | null, modifiers: string)
        =
        this.attr ($"data-class:%s{name}%s{modifiers}", value)

    /// Sets the text content reactively. Renders `data-text="…"`.
    [<Extension>]
    static member dataText(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr ("data-text", value)

    /// Shows or hides the element reactively. Renders `data-show="…"`.
    [<Extension>]
    static member dataShow(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr ("data-show", value)


/// Event-driven attributes.
type DatastarEventExtensions =

    /// Attaches an event listener. Renders `data-on:{event}="…"`.
    [<Extension>]
    static member dataOn(this: #HtmlTag, event: string, [<StringSyntax("js")>] value: string | null) =
        this.attr ($"data-on:%s{event}", value)

    /// Attaches an event listener with modifiers. Renders `data-on:{event}{modifiers}="…"`.
    /// `modifiers` is appended verbatim and must include the leading `__` (see `DsModifier`).
    [<Extension>]
    static member dataOn
        (this: #HtmlTag, event: string, [<StringSyntax("js")>] value: string | null, modifiers: string)
        =
        this.attr ($"data-on:%s{event}%s{modifiers}", value)

    /// Runs an expression when the element intersects the viewport. Renders `data-on-intersect="…"`.
    [<Extension>]
    static member dataOnIntersect(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-on-intersect", value)

    /// Runs an expression on intersection with modifiers. Renders `data-on-intersect{modifiers}="…"`.
    [<Extension>]
    static member dataOnIntersect(this: #HtmlTag, [<StringSyntax("js")>] value: string | null, modifiers: string) =
        this.attr ($"data-on-intersect%s{modifiers}", value)

    /// Runs an expression at a regular interval. Renders `data-on-interval="…"`.
    [<Extension>]
    static member dataOnInterval(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-on-interval", value)

    /// Runs an expression at a regular interval with modifiers. Renders `data-on-interval{modifiers}="…"`.
    [<Extension>]
    static member dataOnInterval(this: #HtmlTag, [<StringSyntax("js")>] value: string | null, modifiers: string) =
        this.attr ($"data-on-interval%s{modifiers}", value)

    /// Runs an expression whenever any signal is patched. Renders `data-on-signal-patch="…"`.
    [<Extension>]
    static member dataOnSignalPatch(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-on-signal-patch", value)

    /// Runs an expression on signal patches with modifiers. Renders `data-on-signal-patch{modifiers}="…"`.
    [<Extension>]
    static member dataOnSignalPatch(this: #HtmlTag, [<StringSyntax("js")>] value: string | null, modifiers: string) =
        this.attr ($"data-on-signal-patch%s{modifiers}", value)

    /// Filters which signals `data-on-signal-patch` reacts to. Renders `data-on-signal-patch-filter="…"`.
    [<Extension>]
    static member dataOnSignalPatchFilter(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-on-signal-patch-filter", value)


/// Lifecycle and utility attributes.
type DatastarLifecycleExtensions =

    /// Runs an expression when the attribute initialises. Renders `data-init="…"`.
    [<Extension>]
    static member dataInit(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) = this.attr ("data-init", value)

    /// Runs an initialisation expression with modifiers. Renders `data-init{modifiers}="…"`.
    [<Extension>]
    static member dataInit(this: #HtmlTag, [<StringSyntax("js")>] value: string | null, modifiers: string) =
        this.attr ($"data-init%s{modifiers}", value)

    /// Runs an expression on load and whenever its reactive dependencies change. Renders `data-effect="…"`.
    [<Extension>]
    static member dataEffect(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-effect", value)

    /// Tells Datastar to ignore this element and its descendants. Renders `data-ignore` when true.
    [<Extension>]
    static member dataIgnore(this: #HtmlTag, value: bool) = this.bool ("data-ignore", value)

    /// Ignores the element with modifiers (e.g. `DsModifier.self`). Renders `data-ignore{modifiers}` when true.
    [<Extension>]
    static member dataIgnore(this: #HtmlTag, value: bool, modifiers: string) =
        this.bool ($"data-ignore%s{modifiers}", value)

    /// Tells the morphing patcher to skip this element and its descendants. Renders `data-ignore-morph` when true.
    [<Extension>]
    static member dataIgnoreMorph(this: #HtmlTag, value: bool) = this.bool ("data-ignore-morph", value)

    /// Preserves attribute values when morphing DOM elements. Renders `data-preserve-attr="…"`.
    [<Extension>]
    static member dataPreserveAttr(this: #HtmlTag, value: string | null) = this.attr ("data-preserve-attr", value)

    /// Renders signals as reactive JSON (useful for debugging). Valueless form renders `data-json-signals`.
    [<Extension>]
    static member dataJsonSignals(this: #HtmlTag) = this.bool ("data-json-signals", true)

    /// Renders signals as reactive JSON with an include/exclude filter. Renders `data-json-signals="…"`.
    [<Extension>]
    static member dataJsonSignals(this: #HtmlTag, [<StringSyntax("js")>] value: string | null) =
        this.attr ("data-json-signals", value)

    /// Renders signals as reactive JSON with a filter and modifiers. Renders `data-json-signals{modifiers}="…"`.
    [<Extension>]
    static member dataJsonSignals(this: #HtmlTag, [<StringSyntax("js")>] value: string | null, modifiers: string) =
        this.attr ($"data-json-signals%s{modifiers}", value)
