module Attributes.Tests

open Oxpecker.ViewEngine
open Oxpecker.Alpine
open Xunit
open FsUnit.Light

[<Fact>]
let ``xData renders x-data with value`` () =
    div().attr(xData "{ open: false }") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-data="{ open: false }">content</div>"""

[<Fact>]
let ``xData without value renders boolean x-data`` () =
    div().attr(xData()) { "content" }
    |> Render.toString
    |> shouldEqual """<div x-data>content</div>"""

[<Fact>]
let ``xInit renders x-init`` () =
    div().attr(xInit "loaded = true") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-init="loaded = true">content</div>"""

[<Fact>]
let ``xShow renders x-show`` () =
    div().attr(xShow "open") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-show="open">content</div>"""

[<Fact>]
let ``xShow with modifier renders x-show modifier`` () =
    div().attr(xShow("open", "important")) { "content" }
    |> Render.toString
    |> shouldEqual """<div x-show.important="open">content</div>"""

[<Fact>]
let ``xBind with attribute renders x-bind attribute`` () =
    input().attr(xData "{ placeholderText: 'Type here...' }", xBind("placeholder", "placeholderText"))
    |> Render.toString
    |> shouldEqual """<input x-data="{ placeholderText: &#39;Type here...&#39; }" x-bind:placeholder="placeholderText">"""

[<Fact>]
let ``xBind without attribute renders x-bind object binding`` () =
    button().attr(xBind "trigger") { "Open" }
    |> Render.toString
    |> shouldEqual """<button x-bind="trigger">Open</button>"""

[<Fact>]
let ``xOn renders x-on event`` () =
    button().attr(xOn("click", "open = !open")) { "Toggle" }
    |> Render.toString
    |> shouldEqual """<button x-on:click="open = !open">Toggle</button>"""

[<Fact>]
let ``xOn with modifiers renders modified event`` () =
    form().attr(xOn("submit", "save()", "prevent", ".once")) { "form" }
    |> Render.toString
    |> shouldEqual """<form x-on:submit.prevent.once="save()">form</form>"""

[<Fact>]
let ``xText renders x-text`` () =
    span().attr(xText "message")
    |> Render.toString
    |> shouldEqual """<span x-text="message"></span>"""

[<Fact>]
let ``xHtml renders x-html`` () =
    div().attr(xHtml "markup") { "fallback" }
    |> Render.toString
    |> shouldEqual """<div x-html="markup">fallback</div>"""

[<Fact>]
let ``xModel renders x-model`` () =
    input().attr(xModel "search")
    |> Render.toString
    |> shouldEqual """<input x-model="search">"""

[<Fact>]
let ``xModel with modifiers renders modified x-model`` () =
    input().attr(xModel("age", "number", "debounce.500ms"))
    |> Render.toString
    |> shouldEqual """<input x-model.number.debounce.500ms="age">"""

[<Fact>]
let ``xModelable renders x-modelable`` () =
    div().attr(xModelable "count") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-modelable="count">content</div>"""

[<Fact>]
let ``xFor renders x-for`` () =
    template().attr(xFor "item in items") { li().attr(xText "item") }
    |> Render.toString
    |> shouldEqual """<template x-for="item in items"><li x-text="item"></li></template>"""

[<Fact>]
let ``xTransition without value renders boolean x-transition`` () =
    div().attr(xTransition()) { "content" }
    |> Render.toString
    |> shouldEqual """<div x-transition>content</div>"""

[<Fact>]
let ``xTransition with modifiers renders modified boolean x-transition`` () =
    div().attr(xTransition("duration.500ms")) { "content" }
    |> Render.toString
    |> shouldEqual """<div x-transition.duration.500ms>content</div>"""

[<Fact>]
let ``xTransition with argument and value renders transition phase`` () =
    div().attr(xTransition(XTransitionPhase.Enter, "transition ease-out", "duration.300ms")) { "content" }
    |> Render.toString
    |> shouldEqual """<div x-transition:enter.duration.300ms="transition ease-out">content</div>"""

[<Fact>]
let ``xEffect renders x-effect`` () =
    div().attr(xEffect "console.log(open)") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-effect="console.log(open)">content</div>"""

[<Fact>]
let ``xIgnore true renders x-ignore`` () =
    div().attr(xIgnore true) { "content" }
    |> Render.toString
    |> shouldEqual """<div x-ignore>content</div>"""

[<Fact>]
let ``xIgnore false does not render`` () =
    div().attr(xIgnore false) { "content" }
    |> Render.toString
    |> shouldEqual """<div>content</div>"""

[<Fact>]
let ``xIgnore with modifier renders modified x-ignore`` () =
    div().attr(xIgnore(true, "self")) { "content" }
    |> Render.toString
    |> shouldEqual """<div x-ignore.self>content</div>"""

[<Fact>]
let ``xRef renders x-ref`` () =
    button().attr(xRef "trigger") { "Open" }
    |> Render.toString
    |> shouldEqual """<button x-ref="trigger">Open</button>"""

[<Fact>]
let ``xCloak true renders x-cloak`` () =
    div().attr(xCloak true) { "content" }
    |> Render.toString
    |> shouldEqual """<div x-cloak>content</div>"""

[<Fact>]
let ``xCloak false does not render`` () =
    div().attr(xCloak false) { "content" }
    |> Render.toString
    |> shouldEqual """<div>content</div>"""

[<Fact>]
let ``xTeleport renders x-teleport`` () =
    template().attr(xTeleport "body") { div() }
    |> Render.toString
    |> shouldEqual """<template x-teleport="body"><div></div></template>"""

[<Fact>]
let ``xIf renders x-if`` () =
    template().attr(xIf "open") { div() }
    |> Render.toString
    |> shouldEqual """<template x-if="open"><div></div></template>"""

[<Fact>]
let ``xId renders x-id`` () =
    div().attr(xId "['dropdown-button']") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-id="[&#39;dropdown-button&#39;]">content</div>"""

[<Fact>]
let ``multiple typed attributes in one attr call`` () =
    div().attr(xData "{ open: false }", xOn("click", "open = !open"), xShow "open") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-data="{ open: false }" x-on:click="open = !open" x-show="open">content</div>"""

[<Fact>]
let ``null xText does not render attribute`` () =
    span().attr(xText null) { "fallback" }
    |> Render.toString
    |> shouldEqual """<span>fallback</span>"""

[<Fact>]
let ``Alpine attribute gets combined with normal attributes`` () =
    div(id = "dropdown").attr(xData "{ open: false }") { "content" }
    |> Render.toString
    |> shouldEqual """<div id="dropdown" x-data="{ open: false }">content</div>"""
