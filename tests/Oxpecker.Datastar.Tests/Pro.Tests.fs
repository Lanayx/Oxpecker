module Pro.Tests

open Oxpecker.ViewEngine
open Oxpecker.Datastar
open Oxpecker.Datastar.Pro
open Xunit

[<Fact>]
let ``Pro expression attributes render every supported form`` () =
    Assert.All(
        [
            div().dataAnimate("{ opacity: $opacity }") { } :> HtmlTag,
            """<div data-animate="{ opacity: $opacity }"></div>"""
            input().dataCustomValidity("$valid ? '' : 'Invalid'"),
            """<input data-custom-validity="$valid ? &#39;&#39; : &#39;Invalid&#39;">"""
            div().dataMatchMedia("dark", "'prefers-color-scheme: dark'") { },
            """<div data-match-media:dark="&#39;prefers-color-scheme: dark&#39;"></div>"""
            div().dataMatchMedia("is-dark", "'prefers-color-scheme: dark'", DsCase.kebab) { },
            """<div data-match-media:is-dark__case.kebab="&#39;prefers-color-scheme: dark&#39;"></div>"""
            div().dataOnRaf("$count++") { }, """<div data-on-raf="$count++"></div>"""
            div().dataOnRaf("$count++", DsModifier.throttleMs 100) { },
            """<div data-on-raf__throttle.100ms="$count++"></div>"""
            div().dataOnResize("$width = el.clientWidth") { },
            """<div data-on-resize="$width = el.clientWidth"></div>"""
            div().dataOnResize("$width = el.clientWidth", DsModifier.debounceMs 100) { },
            """<div data-on-resize__debounce.100ms="$width = el.clientWidth"></div>"""
            div().dataReplaceUrl("'next'") { }, """<div data-replace-url="&#39;next&#39;"></div>"""
            div().dataViewTransition("$name") { }, """<div data-view-transition="$name"></div>"""
        ],
        fun (tag, expected) -> Assert.Equal(expected, Render.toString tag)
    )

[<Fact>]
let ``Pro persistence attributes render every supported form`` () =
    Assert.All(
        [
            div().dataPersist() { }, """<div data-persist></div>"""
            div().dataPersist("{ include: /cart/ }") { }, """<div data-persist="{ include: /cart/ }"></div>"""
            div().dataPersistKey("cart") { }, """<div data-persist:cart></div>"""
            div().dataPersistKey("cart", DsProModifier.session) { }, """<div data-persist:cart__session></div>"""
            div().dataQueryString() { }, """<div data-query-string></div>"""
            div().dataQueryString("{ include: /page/ }") { }, """<div data-query-string="{ include: /page/ }"></div>"""
            div().dataQueryStringWith(DsProModifier.filter + DsProModifier.history) { },
            """<div data-query-string__filter__history></div>"""
            div().dataScrollIntoView() { }, """<div data-scroll-into-view></div>"""
            div().dataScrollIntoView(DsProModifier.smooth + DsProModifier.focus) { },
            """<div data-scroll-into-view__smooth__focus></div>"""
        ],
        fun (tag, expected) -> Assert.Equal(expected, Render.toString tag)
    )

[<Fact>]
let ``Pro modifiers have the documented spellings`` () =
    Assert.All(
        [
            DsProModifier.session, "__session"
            DsProModifier.filter, "__filter"
            DsProModifier.history, "__history"
            DsProModifier.smooth, "__smooth"
            DsProModifier.instant, "__instant"
            DsProModifier.auto, "__auto"
            DsProModifier.hstart, "__hstart"
            DsProModifier.hcenter, "__hcenter"
            DsProModifier.hend, "__hend"
            DsProModifier.hnearest, "__hnearest"
            DsProModifier.vstart, "__vstart"
            DsProModifier.vcenter, "__vcenter"
            DsProModifier.vend, "__vend"
            DsProModifier.vnearest, "__vnearest"
            DsProModifier.focus, "__focus"
        ],
        fun (actual, expected) -> Assert.Equal(expected, actual)
    )
