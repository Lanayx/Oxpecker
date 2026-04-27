module Attributes.Tests

open Oxpecker.ViewEngine
open Oxpecker.Alpine
open Xunit
open FsUnit.Light

[<Fact>]
let ``xData renders x-data with value`` () =
    div().xData("{ open: false }") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-data="{ open: false }">content</div>"""

[<Fact>]
let ``xData without value renders boolean x-data`` () =
    div().xData() { "content" }
    |> Render.toString
    |> shouldEqual """<div x-data>content</div>"""

[<Fact>]
let ``xInit renders x-init`` () =
    div().xInit("loaded = true") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-init="loaded = true">content</div>"""

[<Fact>]
let ``xShow renders x-show`` () =
    div().xShow("open") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-show="open">content</div>"""

[<Fact>]
let ``xShow with modifier renders x-show modifier`` () =
    div().xShow("open", "important") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-show.important="open">content</div>"""

[<Fact>]
let ``xBind with attribute renders x-bind attribute`` () =
    input().xData("{ placeholderText: 'Type here...' }").xBind("placeholder", "placeholderText")
    |> Render.toString
    |> shouldEqual """<input x-data="{ placeholderText: &#39;Type here...&#39; }" x-bind:placeholder="placeholderText">"""

[<Fact>]
let ``xBind without attribute renders x-bind object binding`` () =
    button().xBind("trigger") { "Open" }
    |> Render.toString
    |> shouldEqual """<button x-bind="trigger">Open</button>"""

[<Fact>]
let ``xOn renders x-on event`` () =
    button().xOn("click", "open = !open") { "Toggle" }
    |> Render.toString
    |> shouldEqual """<button x-on:click="open = !open">Toggle</button>"""

[<Fact>]
let ``xOn with modifiers renders modified event`` () =
    form().xOn("submit.prevent.once", "save()") { "form" }
    |> Render.toString
    |> shouldEqual """<form x-on:submit.prevent.once="save()">form</form>"""

[<Fact>]
let ``xText renders x-text`` () =
    span().xText("message")
    |> Render.toString
    |> shouldEqual """<span x-text="message"></span>"""

[<Fact>]
let ``xHtml renders x-html`` () =
    div().xHtml("markup") { "fallback" }
    |> Render.toString
    |> shouldEqual """<div x-html="markup">fallback</div>"""

[<Fact>]
let ``xModel renders x-model`` () =
    input().xModel("search")
    |> Render.toString
    |> shouldEqual """<input x-model="search">"""

[<Fact>]
let ``xModel with modifiers renders modified x-model`` () =
    input().xModel("age", "number.debounce.500ms")
    |> Render.toString
    |> shouldEqual """<input x-model.number.debounce.500ms="age">"""

[<Fact>]
let ``xModelable renders x-modelable`` () =
    div().xModelable("count") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-modelable="count">content</div>"""

[<Fact>]
let ``xFor renders x-for`` () =
    template().xFor("item in items") { li().xText("item") }
    |> Render.toString
    |> shouldEqual """<template x-for="item in items"><li x-text="item"></li></template>"""

[<Fact>]
let ``xTransition without value renders boolean x-transition`` () =
    div().xTransition() { "content" }
    |> Render.toString
    |> shouldEqual """<div x-transition>content</div>"""

[<Fact>]
let ``xTransition with modifiers renders modified boolean x-transition`` () =
    div().xTransition("duration.500ms") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-transition.duration.500ms>content</div>"""

[<Fact>]
let ``xEffect renders x-effect`` () =
    div().xEffect("console.log(open)") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-effect="console.log(open)">content</div>"""

[<Fact>]
let ``xIgnore true renders x-ignore`` () =
    div().xIgnore(true) { "content" }
    |> Render.toString
    |> shouldEqual """<div x-ignore>content</div>"""

[<Fact>]
let ``xIgnore false does not render`` () =
    div().xIgnore(false) { "content" }
    |> Render.toString
    |> shouldEqual """<div>content</div>"""

[<Fact>]
let ``xIgnore with modifier renders modified x-ignore`` () =
    div().xIgnore(true, "self") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-ignore.self>content</div>"""

[<Fact>]
let ``xRef renders x-ref`` () =
    button().xRef("trigger") { "Open" }
    |> Render.toString
    |> shouldEqual """<button x-ref="trigger">Open</button>"""

[<Fact>]
let ``xCloak true renders x-cloak`` () =
    div().xCloak(true) { "content" }
    |> Render.toString
    |> shouldEqual """<div x-cloak>content</div>"""

[<Fact>]
let ``xCloak false does not render`` () =
    div().xCloak(false) { "content" }
    |> Render.toString
    |> shouldEqual """<div>content</div>"""

[<Fact>]
let ``xTeleport renders x-teleport`` () =
    template().xTeleport("body") { div() }
    |> Render.toString
    |> shouldEqual """<template x-teleport="body"><div></div></template>"""

[<Fact>]
let ``xIf renders x-if`` () =
    template().xIf("open") { div() }
    |> Render.toString
    |> shouldEqual """<template x-if="open"><div></div></template>"""

[<Fact>]
let ``xId renders x-id`` () =
    div().xId("['dropdown-button']") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-id="[&#39;dropdown-button&#39;]">content</div>"""

[<Fact>]
let ``multiple chained Alpine extensions`` () =
    div().xData("{ open: false }").xOn("click", "open = !open").xShow("open") { "content" }
    |> Render.toString
    |> shouldEqual """<div x-data="{ open: false }" x-on:click="open = !open" x-show="open">content</div>"""

[<Fact>]
let ``null xText does not render attribute`` () =
    span().xText(null) { "fallback" }
    |> Render.toString
    |> shouldEqual """<span>fallback</span>"""

[<Fact>]
let ``Alpine attribute gets combined with normal attributes`` () =
    div(id = "dropdown").xData("{ open: false }") { "content" }
    |> Render.toString
    |> shouldEqual """<div id="dropdown" x-data="{ open: false }">content</div>"""
