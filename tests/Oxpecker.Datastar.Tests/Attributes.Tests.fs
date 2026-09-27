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
    |> shouldEqual """<section data-signals="{ count: 0 }"><strong data-text="$count">0</strong><button type="button" data-on:click="$count &lt; 10 &amp;&amp; $count++">+</button></section>"""

[<Fact>]
let ``signal binding and event modifiers render on the attribute name`` () =
    input().dataBind("query", DsCase.kebab + DsModifier.bindEvent "input.change").dataOn("input", "search()", DsModifier.debounceMs 500)
    |> Render.toString
    |> shouldEqual """<input data-bind:query__case.kebab__event.input.change data-on:input__debounce.500ms="search()">"""

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
    div().dataPersistKey("cart", DsProModifier.session).dataMatchMedia("dark", "'prefers-color-scheme: dark'") { "content" }
    |> Render.toString
    |> shouldEqual """<div data-persist:cart__session data-match-media:dark="&#39;prefers-color-scheme: dark&#39;">content</div>"""
