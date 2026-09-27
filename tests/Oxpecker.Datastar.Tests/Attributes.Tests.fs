module Attributes.Tests

open Oxpecker.ViewEngine
open Oxpecker.Datastar
open Oxpecker.Datastar.Pro
open Xunit
open FsUnit.Light

[<Fact>]
let ``Datastar counter renders an escaped expression and nested content`` () =
    section().dataSignals("{ count: 0 }") {
        strong().dataText("$count") { "0" }
        button(type' = "button").dataOn("click", "$count < 10 && $count++") { "+" }
    }
    |> Render.toString
    |> shouldEqual
        """<section data-signals="{ count: 0 }"><strong data-text="$count">0</strong><button type="button" data-on:click="$count &lt; 10 &amp;&amp; $count++">+</button></section>"""

[<Fact>]
let ``signal binding and event modifiers render on the attribute name`` () =
    input()
        .dataBind("query", DsCase.kebab + DsModifier.bindEvent "input.change")
        .dataOn("input", "search()", DsModifier.debounceMs 500)
    |> Render.toString
    |> shouldEqual
        """<input data-bind:query__case.kebab__event.input.change data-on:input__debounce.500ms="search()">"""

[<Fact>]
let ``signal name value forms render on the attribute value`` () =
    div().dataBindValue("query").dataIndicatorValue("fetching").dataRefValue("element") { "content" }
    |> Render.toString
    |> shouldEqual """<div data-bind="query" data-indicator="fetching" data-ref="element">content</div>"""

[<Fact>]
let ``false boolean attributes are omitted`` () =
    div().dataIgnore(false).dataIgnoreMorph(true) { "content" }
    |> Render.toString
    |> shouldEqual """<div data-ignore-morph>content</div>"""

[<Fact>]
let ``Pro attributes remain available from their separate namespace`` () =
    div().dataPersistKey("cart", DsProModifier.session).dataMatchMedia("dark", "'prefers-color-scheme: dark'") {
        "content"
    }
    |> Render.toString
    |> shouldEqual
        """<div data-persist:cart__session data-match-media:dark="&#39;prefers-color-scheme: dark&#39;">content</div>"""

[<Fact>]
let ``signal attributes render every supported form`` () =
    Assert.All(
        [
            div().dataSignals("{ count: 0 }") { } :> HtmlTag, """<div data-signals="{ count: 0 }"></div>"""
            div().dataSignals("count", "0") { }, """<div data-signals:count="0"></div>"""
            div().dataSignals("my-count", "0", DsCase.kebab) { },
            """<div data-signals:my-count__case.kebab="0"></div>"""
            div().dataComputed("{ doubled: () => $count * 2 }") { },
            """<div data-computed="{ doubled: () =&gt; $count * 2 }"></div>"""
            div().dataComputed("doubled", "$count * 2") { }, """<div data-computed:doubled="$count * 2"></div>"""
            div().dataComputed("my-total", "$count * 2", DsCase.kebab) { },
            """<div data-computed:my-total__case.kebab="$count * 2"></div>"""
            div().dataRef("element") { }, """<div data-ref:element></div>"""
            div().dataRefValue("element") { }, """<div data-ref="element"></div>"""
            div().dataRef("my-element", DsCase.kebab) { }, """<div data-ref:my-element__case.kebab></div>"""
            div().dataIndicator("fetching") { }, """<div data-indicator:fetching></div>"""
            div().dataIndicatorValue("fetching") { }, """<div data-indicator="fetching"></div>"""
            div().dataIndicator("is-fetching", DsCase.kebab) { },
            """<div data-indicator:is-fetching__case.kebab></div>"""
            input().dataBind("query"), """<input data-bind:query>"""
            input().dataBindValue("query"), """<input data-bind="query">"""
            input().dataBind("is-checked", DsModifier.bindProp "checked"),
            """<input data-bind:is-checked__prop.checked>"""
        ],
        fun (tag, expected) -> Assert.Equal(expected, Render.toString tag)
    )

[<Fact>]
let ``DOM attributes render every supported form`` () =
    Assert.All(
        [
            div().dataAttr("{ hidden: $hidden }") { }, """<div data-attr="{ hidden: $hidden }"></div>"""
            div().dataAttr("aria-label", "$label") { }, """<div data-attr:aria-label="$label"></div>"""
            div().dataStyle("{ color: $color }") { }, """<div data-style="{ color: $color }"></div>"""
            div().dataStyle("background-color", "$color") { }, """<div data-style:background-color="$color"></div>"""
            div().dataClass("{ active: $active }") { }, """<div data-class="{ active: $active }"></div>"""
            div().dataClass("active", "$active") { }, """<div data-class:active="$active"></div>"""
            div().dataClass("my-class", "$active", DsCase.camel) { },
            """<div data-class:my-class__case.camel="$active"></div>"""
            div().dataText("$label") { }, """<div data-text="$label"></div>"""
            div().dataShow("$visible") { }, """<div data-show="$visible"></div>"""
        ],
        fun (tag, expected) -> Assert.Equal(expected, Render.toString tag)
    )

[<Fact>]
let ``event attributes render every supported form`` () =
    Assert.All(
        [
            div().dataOn("click", "$count++") { }, """<div data-on:click="$count++"></div>"""
            div().dataOn("click", "$count++", DsModifier.once) { }, """<div data-on:click__once="$count++"></div>"""
            div().dataOnIntersect("$seen = true") { }, """<div data-on-intersect="$seen = true"></div>"""
            div().dataOnIntersect("$seen = true", DsModifier.full) { },
            """<div data-on-intersect__full="$seen = true"></div>"""
            div().dataOnIntersect("$seen = true", DsModifier.threshold 50) { },
            """<div data-on-intersect__threshold.50="$seen = true"></div>"""
            div().dataOnInterval("$count++") { }, """<div data-on-interval="$count++"></div>"""
            div().dataOnInterval("$count++", DsModifier.durationMs 500) { },
            """<div data-on-interval__duration.500ms="$count++"></div>"""
            div().dataOnSignalPatch("console.log(patch)") { },
            """<div data-on-signal-patch="console.log(patch)"></div>"""
            div().dataOnSignalPatch("save()", DsModifier.debounceMs 100) { },
            """<div data-on-signal-patch__debounce.100ms="save()"></div>"""
            div().dataOnSignalPatchFilter("{ include: /user/ }") { },
            """<div data-on-signal-patch-filter="{ include: /user/ }"></div>"""
        ],
        fun (tag, expected) -> Assert.Equal(expected, Render.toString tag)
    )

[<Fact>]
let ``lifecycle attributes render every supported form`` () =
    Assert.All(
        [
            div().dataInit("$count = 1") { }, """<div data-init="$count = 1"></div>"""
            div().dataInit("$count = 1", DsModifier.delayMs 500) { },
            """<div data-init__delay.500ms="$count = 1"></div>"""
            div().dataEffect("$total = $count * 2") { }, """<div data-effect="$total = $count * 2"></div>"""
            div().dataIgnore(true) { }, """<div data-ignore></div>"""
            div().dataIgnore(false) { }, """<div></div>"""
            div().dataIgnore(true, DsModifier.self) { }, """<div data-ignore__self></div>"""
            div().dataIgnoreMorph(true) { }, """<div data-ignore-morph></div>"""
            div().dataIgnoreMorph(false) { }, """<div></div>"""
            div().dataPreserveAttr("open class") { }, """<div data-preserve-attr="open class"></div>"""
            div().dataJsonSignals() { }, """<div data-json-signals></div>"""
            div().dataJsonSignals("{ include: /user/ }") { }, """<div data-json-signals="{ include: /user/ }"></div>"""
            div().dataJsonSignals("{ include: /user/ }", DsModifier.terse) { },
            """<div data-json-signals__terse="{ include: /user/ }"></div>"""
        ],
        fun (tag, expected) -> Assert.Equal(expected, Render.toString tag)
    )
