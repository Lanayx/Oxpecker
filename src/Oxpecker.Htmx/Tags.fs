namespace Oxpecker.Htmx

open Oxpecker.ViewEngine

/// `<hx-partial>` — a response-side element that swaps its children into its own target, letting a single
/// response update several elements. Set the target with `hxTarget` (CSS or extended selector, resolved
/// relative to the triggering element) or with `id` as a shorthand for `#id`; `hxTarget` wins when both are set.
/// Set the swap strategy with `hxSwap` (defaults to `innerHTML`).
/// Partials are swapped after the main content, in document order. A response made only of partials leaves
/// the main target untouched unless `HxSwapModifier.swapEmpty true` is used.
/// See https://four.htmx.org/reference/tags/hx-partial
type hxPartial() =
    inherit RegularNode("hx-partial")
