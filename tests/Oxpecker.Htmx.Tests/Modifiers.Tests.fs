module Modifiers.Tests

open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Xunit
open FsUnit.Light

// ─── HxSwapModifier.swapMs ───

[<Fact>]
let ``HxSwapModifier.swapMs returns delay with ms suffix`` () =
    HxSwapModifier.swapMs 200 |> shouldEqual " swap:200ms"

[<Fact>]
let ``HxSwapModifier.swapMs composes with hxSwap`` () =
    div().hxGet("/data").hxSwap(HxSwapMethod.innerHtml + HxSwapModifier.swapMs 300) { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/data" hx-swap="innerHTML swap:300ms">content</div>"""

// ─── HxSwapModifier.settleMs ───

[<Fact>]
let ``HxSwapModifier.settleMs returns settle with ms suffix`` () =
    HxSwapModifier.settleMs 500 |> shouldEqual "settle:500ms"

[<Fact>]
let ``HxSwapModifier.settleMs composes with hxSwap`` () =
    div().hxGet("/data").hxSwap(HxSwapMethod.outerHtml + " " + HxSwapModifier.settleMs 250) { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/data" hx-swap="outerHTML settle:250ms">content</div>"""

// ─── HxTriggerModifier.delayMs ───

[<Fact>]
let ``HxTriggerModifier.delayMs returns delay with ms suffix`` () =
    HxTriggerModifier.delayMs 200 |> shouldEqual " delay:200ms"

[<Fact>]
let ``HxTriggerModifier.delayMs composes with hxTrigger`` () =
    input().hxGet("/search").hxTrigger("keyup" + HxTriggerModifier.delayMs 200 + HxTriggerModifier.changed)
    |> Render.toString
    |> shouldEqual """<input hx-get="/search" hx-trigger="keyup delay:200ms changed">"""

// ─── HxTriggerModifier.throttleMs ───

[<Fact>]
let ``HxTriggerModifier.throttleMs returns throttle with ms suffix`` () =
    HxTriggerModifier.throttleMs 500 |> shouldEqual " throttle:500ms"

[<Fact>]
let ``HxTriggerModifier.throttleMs composes with hxTrigger`` () =
    input().hxGet("/search").hxTrigger("keyup" + HxTriggerModifier.throttleMs 500)
    |> Render.toString
    |> shouldEqual """<input hx-get="/search" hx-trigger="keyup throttle:500ms">"""

// ─── HxSyncModifier ───

[<Fact>]
let ``HxSyncModifier.drop has expected value`` () =
    HxSyncModifier.drop |> shouldEqual ":drop"

[<Fact>]
let ``HxSyncModifier.abort has expected value`` () =
    HxSyncModifier.abort |> shouldEqual ":abort"

[<Fact>]
let ``HxSyncModifier.replace has expected value`` () =
    HxSyncModifier.replace |> shouldEqual ":replace"

[<Fact>]
let ``HxSyncModifier.queue has expected value`` () =
    HxSyncModifier.queue |> shouldEqual ":queue"

[<Fact>]
let ``HxSyncModifier.queueFirst has expected value`` () =
    HxSyncModifier.queueFirst |> shouldEqual ":queue first"

[<Fact>]
let ``HxSyncModifier.queueLast has expected value`` () =
    HxSyncModifier.queueLast |> shouldEqual ":queue last"

[<Fact>]
let ``HxSyncModifier.queueAll has expected value`` () =
    HxSyncModifier.queueAll |> shouldEqual ":queue all"

[<Fact>]
let ``HxSyncModifier.drop composes with hxSync`` () =
    button().hxPost("/action").hxSync(HxSelector.closest "form" + HxSyncModifier.drop) { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/action" hx-sync="closest form:drop">Go</button>"""

[<Fact>]
let ``HxSyncModifier.abort composes with hxSync`` () =
    button().hxPost("/action").hxSync(HxSelector.closest "form" + HxSyncModifier.abort) { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/action" hx-sync="closest form:abort">Go</button>"""

[<Fact>]
let ``HxSyncModifier.replace composes with hxSync`` () =
    button().hxPost("/action").hxSync(HxSelector.this' + HxSyncModifier.replace) { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/action" hx-sync="this:replace">Go</button>"""

[<Fact>]
let ``HxSyncModifier.queue composes with hxSync`` () =
    button().hxPost("/action").hxSync(HxSelector.this' + HxSyncModifier.queue) { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/action" hx-sync="this:queue">Go</button>"""

[<Fact>]
let ``HxSyncModifier.queueFirst composes with hxSync`` () =
    button().hxPost("/action").hxSync(HxSelector.this' + HxSyncModifier.queueFirst) { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/action" hx-sync="this:queue first">Go</button>"""

[<Fact>]
let ``HxSyncModifier.queueLast composes with hxSync`` () =
    button().hxPost("/action").hxSync(HxSelector.this' + HxSyncModifier.queueLast) { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/action" hx-sync="this:queue last">Go</button>"""

[<Fact>]
let ``HxSyncModifier.queueAll composes with hxSync`` () =
    button().hxPost("/action").hxSync(HxSelector.this' + HxSyncModifier.queueAll) { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/action" hx-sync="this:queue all">Go</button>"""

// ─── HxBoostModifier ───

[<Fact>]
let ``HxBoostModifier.swap returns swap value`` () =
    HxBoostModifier.swap HxSwapMethod.innerHtml |> shouldEqual " swap:innerHTML"

[<Fact>]
let ``HxBoostModifier.target returns target value`` () =
    HxBoostModifier.target "#main" |> shouldEqual " target:#main"

[<Fact>]
let ``HxBoostModifier.select returns select value`` () =
    HxBoostModifier.select ".content" |> shouldEqual " select:.content"

[<Fact>]
let ``HxBoostModifier composes with hxBoost`` () =
    body()
        .hxBoost(
            "true"
            + HxBoostModifier.swap HxSwapMethod.outerHtml
            + HxBoostModifier.target "#main"
        ) {
        "content"
    }
    |> Render.toString
    |> shouldEqual """<body hx-boost="true swap:outerHTML target:#main">content</body>"""

[<Fact>]
let ``HxBoostModifier.select composes with hxBoost`` () =
    body().hxBoost("true" + HxBoostModifier.select ".main") { "content" }
    |> Render.toString
    |> shouldEqual """<body hx-boost="true select:.main">content</body>"""
