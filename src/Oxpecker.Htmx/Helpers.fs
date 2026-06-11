namespace Oxpecker.Htmx

open System.Diagnostics.CodeAnalysis

/// Global modifiers for attributes.
/// Each constant includes the leading `:` and is appended verbatim after the attribute name.
[<RequireQualifiedAccess>]
module HxModifier =
    /// `:inherited` — the attribute is inherited by child elements.
    [<Literal>]
    let inherited = ":inherited"
    /// `:append` — for `hxVals`, the attribute value is appended to any existing value instead of replacing it.
    [<Literal>]
    let append = ":append"
    /// `:merge` — for `hxDisable`, the selector is merged with any existing selector instead of replacing it.
    [<Literal>]
    let merge = ":merge"

/// Builders for extended selectors.
/// See https://four.htmx.org/docs/features/extended-selectors
/// Each helper returns the htmx selector string ready to pass into selector-typed attributes
/// such as `hxTarget`, `hxSelect`, `hxSelectOob`, `hxIndicator`, `hxInclude`, `hxDisable`, `hxOptimistic`.
[<RequireQualifiedAccess>]
module HxSelector =
    /// `this` — the element itself.
    [<Literal>]
    let this' = "this"
    /// `body` — the document body.
    [<Literal>]
    let body = "body"
    /// `document` — the document object (mainly for event triggers).
    [<Literal>]
    let document = "document"
    /// `window` — the window object (mainly for event triggers).
    [<Literal>]
    let window = "window"
    /// `host` — the shadow DOM host element (only valid inside shadow DOM).
    [<Literal>]
    let host = "host"
    /// `next` — the next sibling element.
    [<Literal>]
    let nextSibling = "next"
    /// `previous` — the previous sibling element.
    [<Literal>]
    let previousSibling = "previous"
    /// `closest <selector>` — nearest ancestor (or self) matching the selector.
    let inline closest ([<StringSyntax("css")>] selector: string) = $"closest {selector}"
    /// `find <selector>` — first child descendant matching the selector.
    let inline find ([<StringSyntax("css")>] selector: string) = $"find {selector}"
    /// `findAll <selector>` — all child descendants matching the selector.
    let inline findAll ([<StringSyntax("css")>] selector: string) = $"findAll {selector}"
    /// `next <selector>` — first following sibling matching the selector.
    let inline next ([<StringSyntax("css")>] selector: string) = $"next {selector}"
    /// `previous <selector>` — first preceding sibling matching the selector.
    let inline previous ([<StringSyntax("css")>] selector: string) = $"previous {selector}"
    /// `global <selector>` — search the entire document, crossing shadow DOM boundaries.
    let inline global' ([<StringSyntax("css")>] selector: string) = $"global {selector}"

/// Constants for `hxSwap` values, which control how the response content is swapped into the DOM.
[<RequireQualifiedAccess>]
module HxSwapMethod =
    /// `innerHTML` — replaces content inside element.
    [<Literal>]
    let innerHtml = "innerHTML"
    /// `outerHTML` — replaces entire element.
    [<Literal>]
    let outerHtml = "outerHTML"
    /// `textContent` — replace the text content of the element, without parsing the response as HTML.
    [<Literal>]
    let textContent = "textContent"
    /// `before` — insert before the element itself.
    [<Literal>]
    let before = "before"
    /// `prepend` — insert as the first child of the element.
    [<Literal>]
    let prepend = "prepend"
    /// `append` — insert as the last child of the element.
    [<Literal>]
    let append = "append"
    /// `after` — insert after the element itself.
    [<Literal>]
    let after = "after"
    /// `innerMorph` — morphs content inside element, preserving state and focus.
    [<Literal>]
    let innerMorph = "innerMorph"
    /// `outerMorph` — morphs entire element, preserving state and focus.
    [<Literal>]
    let outerMorph = "outerMorph"
    /// `outerSync` — syncs attributes from the response's outer element onto the target, then replaces the target's children, preserving the element itself (listeners and state).
    [<Literal>]
    let outerSync = "outerSync"
    /// `delete` — removes element (ignores response content).
    [<Literal>]
    let delete = "delete"
    /// `none` — doesn’t insert content (out-of-band swaps still work).
    [<Literal>]
    let none = "none"
    /// `upsert` — updates existing elements by ID and inserts new ones.
    [<Literal>]
    let upsert = "upsert"
    /// `download` — saves the response as a file download instead of swapping it (requires the `hx-download` extension).
    [<Literal>]
    let download = "download"

/// Modifiers for `hxSwap` that control additional aspects of the swapping behavior.
[<RequireQualifiedAccess>]
module HxSwapModifier =
    /// `transition` — enables View Transitions API for smooth page transitions.
    let inline transition value = $" transition:%b{value}"
    /// `swap` — adds delay before swap.
    let inline swap value = $" swap:%s{value}"
    /// `swapMs` — adds delay before swap, in milliseconds.
    let inline swapMs value = $" swap:%i{value}ms"
    /// `settle` — adds delay between the swap and the settle phase.
    let inline settle value = $"settle:%s{value}"
    /// `settleMs` — adds delay between the swap and the settle phase, in milliseconds.
    let inline settleMs value = $"settle:%i{value}ms"
    /// `ignoreTitle` — prevents htmx from updating the document title based on the response.
    let inline ignoreTitle value = $" ignoreTitle:%b{value}"
    /// `scroll` — auto-scroll to swapped content.
    let inline scroll value = $" scroll:%s{value}"
    /// `scrollTarget` — CSS selector for the element to scroll, used together with `scroll`.
    let inline scrollTarget ([<StringSyntax("css")>] value) = $" scrollTarget:%s{value}"
    /// `show` — scrolls to show the target element in viewport.
    let inline show value = $" show:%s{value}"
    /// `showTarget` — CSS selector for the element to show, used together with `show`.
    let inline showTarget ([<StringSyntax("css")>] value) = $" showTarget:%s{value}"
    /// `target` — override swap target inline. Alternative to using hx-target attribute.
    let inline target value = $" target:%s{value}"
    /// `strip` — controls whether the outer element of the response content is removed before swapping.
    let inline strip value = $" strip:%b{value}"

/// Synthetic event names for `hxTrigger`, beyond the standard DOM events.
[<RequireQualifiedAccess>]
module HxTriggerEvent =
    /// `load` — fires when the element is loaded into the DOM.
    [<Literal>]
    let load = "load"
    /// `revealed` — fires when the element is scrolled into the viewport.
    [<Literal>]
    let revealed = "revealed"
    /// `intersect` — fires when the element becomes visible in the viewport (uses the IntersectionObserver API).
    [<Literal>]
    let intersect = "intersect"
    /// `every <time>` — fires repeatedly on an interval, e.g. `every "1s"`.
    let inline every value = $"every %s{value}"

/// Modifiers for event triggers, which control additional aspects of when and how requests are triggered.
[<RequireQualifiedAccess>]
module HxTriggerModifier =
    /// `delay` — adds delay before triggering the request.
    let inline delay value = $" delay:%s{value}"
    /// `delayMs` — adds delay before triggering the request, in milliseconds.
    let inline delayMs value = $" delay:%i{value}ms"
    /// `throttle` — throttles the trigger to prevent it from firing too frequently.
    let inline throttle value = $" throttle:%s{value}"
    /// `throttleMs` — throttles the trigger to prevent it from firing too frequently, in milliseconds.
    let inline throttleMs value = $" throttle:%i{value}ms"
    /// `once` — ensures the trigger only fires once.
    [<Literal>]
    let once = " once"
    /// `changed` — only triggers when the value of the element has changed.
    [<Literal>]
    let changed = " changed"
    /// `prevent` — calls `event.preventDefault()` on the triggering event.
    [<Literal>]
    let prevent = " prevent"
    /// `stop` — calls `event.stopPropagation()`, preventing the event from bubbling up the DOM.
    [<Literal>]
    let stop = " stop"
    /// `consume` — calls `event.stopPropagation()` (alias of `stop`), preventing the event from bubbling up the DOM.
    [<Literal>]
    let consume = " consume"
    /// `halt` — shorthand for `prevent stop`: calls both `event.preventDefault()` and `event.stopPropagation()`.
    [<Literal>]
    let halt = " halt"
    /// `capture` — listens during the capture phase (top-down) instead of the bubble phase (bottom-up).
    [<Literal>]
    let capture = " capture"
    /// `passive` — registers a passive listener so the browser doesn't wait for the handler before scrolling.
    [<Literal>]
    let passive = " passive"
    /// `from` — The event will be listened for on a different element.
    let inline from ([<StringSyntax("css")>] value) = $" from:%s{value}"
    /// `target` — The event will only trigger if its event.target matches the given CSS selector.
    let inline target ([<StringSyntax("css")>] value) = $" target:%s{value}"
    /// `root` — CSS selector for the IntersectionObserver root element (used with the `intersect` event).
    let inline root ([<StringSyntax("css")>] value) = $" root:%s{value}"
    /// `rootMargin` — margin around the IntersectionObserver root (used with the `intersect` event), e.g. `100px`.
    let inline rootMargin value = $" rootMargin:%s{value}"
    /// `threshold` — visibility threshold for the `intersect` event, e.g. `0.5`.
    let inline threshold value = $" threshold:%s{value}"

/// Modifiers for response status codes, which control additional aspects of how htmx handles responses with specific status codes.
[<RequireQualifiedAccess>]
module HxStatusModifier =
    /// `swap` — swap style for this status code.
    let inline swap value = $" swap:%s{value}"
    /// `target` — CSS selector for the swap target.
    let inline target ([<StringSyntax("css")>] value) = $" target:%s{value}"
    /// `select` — CSS selector to pick content from the response.
    let inline select ([<StringSyntax("css")>] value) = $" select:%s{value}"
    /// `push` — push a URL to browser history.
    let inline push value = $" push:%s{value}"
    /// `replace` — replace the current URL in browser history.
    let inline replace value = $" replace:%s{value}"
    /// `transition` — enables View Transitions API for this status code.
    let inline transition value = $" transition:%b{value}"

/// Modifiers for request synchronization, which control how htmx handles multiple requests that are triggered while a previous request is still in flight.
[<RequireQualifiedAccess>]
module HxSyncModifier =
    /// `drop` — drop (ignore) this request if an existing request is in flight (the default)
    [<Literal>]
    let drop = ":drop"
    /// `abort` — drop (ignore) this request if an existing request is in flight, and, if that is not the case, abort this request if another request occurs while it is still in flight
    [<Literal>]
    let abort = ":abort"
    /// `replace` — abort the current request, if any, and replace it with this request
    [<Literal>]
    let replace = ":replace"
    /// `queue` — place this request in the request queue associated with the given element
    [<Literal>]
    let queue = ":queue"
    /// `queueFirst` — queue the first request to show up while a request is in flight
    [<Literal>]
    let queueFirst = ":queue first"
    /// `queueLast` — queue the last request to show up while a request is in flight
    [<Literal>]
    let queueLast = ":queue last"
    /// `queueAll` — queue all requests that show up while a request is in flight
    [<Literal>]
    let queueAll = ":queue all"

/// Modifiers for boost mode, which control additional aspects of how htmx handles requests that are triggered on elements with `hx-boost` enabled.
[<RequireQualifiedAccess>]
module HxBoostModifier =
    /// `swap` — Swap strategy (innerHTML, outerHTML, etc.)
    let inline swap value = $" swap:%s{value}"
    /// `target` — CSS selector for the swap target.
    let inline target ([<StringSyntax("css")>] value) = $" target:%s{value}"
    /// `select` — CSS selector to pick content from the response.
    let inline select ([<StringSyntax("css")>] value) = $" select:%s{value}"
