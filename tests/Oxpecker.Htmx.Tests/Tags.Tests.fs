module Tags.Tests

open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Xunit
open FsUnit.Light

// ─── hx-partial ───

[<Fact>]
let ``hxPartial renders hx-partial with target and swap`` () =
    hxPartial().hxTarget("#messages").hxSwap(HxSwapMethod.append) { div() { "New message" } }
    |> Render.toString
    |> shouldEqual """<hx-partial hx-target="#messages" hx-swap="append"><div>New message</div></hx-partial>"""

[<Fact>]
let ``hxPartial accepts id as target shorthand`` () =
    hxPartial(id = "count") { span() { "5" } }
    |> Render.toString
    |> shouldEqual """<hx-partial id="count"><span>5</span></hx-partial>"""

[<Fact>]
let ``hxPartial accepts extended selector target`` () =
    hxPartial().hxTarget(HxSelector.closest "li").hxSwap(HxSwapMethod.outerHtml) { li() { "Updated item" } }
    |> Render.toString
    |> shouldEqual """<hx-partial hx-target="closest li" hx-swap="outerHTML"><li>Updated item</li></hx-partial>"""

[<Fact>]
let ``hxPartial renders empty`` () =
    hxPartial().hxTarget("#status")
    |> Render.toString
    |> shouldEqual """<hx-partial hx-target="#status"></hx-partial>"""

[<Fact>]
let ``Multiple hxPartials render in document order`` () =
    Fragment() {
        hxPartial().hxTarget("#messages").hxSwap(HxSwapMethod.append) { div() { "New message" } }
        hxPartial(id = "count") { "5" }
    }
    |> Render.toString
    |> shouldEqual
        """<hx-partial hx-target="#messages" hx-swap="append"><div>New message</div></hx-partial><hx-partial id="count">5</hx-partial>"""
