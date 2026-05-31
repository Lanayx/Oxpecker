module Extensions.Tests

open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Oxpecker.Htmx.Extensions
open Xunit
open FsUnit.Light

// ─── hx-sse ───

[<Fact>]
let ``hxSseConnect renders hx-sse:connect`` () =
    div().hxSseConnect("/stream") { "Waiting..." }
    |> Render.toString
    |> shouldEqual """<div hx-sse:connect="/stream">Waiting...</div>"""

[<Fact>]
let ``hxSseClose renders hx-sse:close`` () =
    div().hxSseConnect("/stream").hxSseClose("done") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-sse:connect="/stream" hx-sse:close="done">content</div>"""

// ─── hx-ws ───

[<Fact>]
let ``hxWsConnect renders hx-ws:connect`` () =
    div().hxWsConnect("/chatroom") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-ws:connect="/chatroom">content</div>"""

[<Fact>]
let ``hxWsSend true renders hx-ws:send boolean attribute`` () =
    form().hxWsSend(true) { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-ws:send>form</form>"""

[<Fact>]
let ``hxWsSend false does not render`` () =
    form().hxWsSend(false) { "form" }
    |> Render.toString
    |> shouldEqual """<form>form</form>"""

[<Fact>]
let ``hxWsSend with url renders hx-ws:send=url`` () =
    button().hxWsSend("/notifications") { "Send" }
    |> Render.toString
    |> shouldEqual """<button hx-ws:send="/notifications">Send</button>"""

// ─── hx-head ───

[<Fact>]
let ``hxHead renders hx-head`` () =
    head().hxHead(HxHeadMode.merge) { title() { "Page" } }
    |> Render.toString
    |> shouldEqual """<head hx-head="merge"><title>Page</title></head>"""

[<Fact>]
let ``HxHeadMode.reEval renders re-eval`` () =
    script(src = "/js/script.js").hxHead(HxHeadMode.reEval)
    |> Render.toString
    |> shouldEqual """<script src="/js/script.js" hx-head="re-eval"></script>"""

// ─── hx-targets ───

[<Fact>]
let ``hxTargets renders hx-targets`` () =
    button().hxGet("/api/notification").hxTargets(".alert-box") { "Refresh" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/api/notification" hx-targets=".alert-box">Refresh</button>"""

[<Fact>]
let ``hxTargets composes with HxSelector`` () =
    button().hxGet("/api/status").hxTargets(HxSelector.find ".status") { "Update" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/api/status" hx-targets="find .status">Update</button>"""

[<Fact>]
let ``hxTargets with inherited modifier renders hx-targets:inherited`` () =
    div().hxTargets(".card-body", HxModifier.inherited) { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-targets:inherited=".card-body">content</div>"""

// ─── hx-ptag ───

[<Fact>]
let ``hxPtag renders hx-ptag`` () =
    div().hxGet("/news").hxTrigger("every 3s").hxPtag("v42") { "Latest News..." }
    |> Render.toString
    |> shouldEqual """<div hx-get="/news" hx-trigger="every 3s" hx-ptag="v42">Latest News...</div>"""

// ─── hx-browser-indicator ───

[<Fact>]
let ``hxBrowserIndicator true renders hx-browser-indicator=true`` () =
    button().hxGet("/api/data").hxBrowserIndicator(true) { "Load Data" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/api/data" hx-browser-indicator="true">Load Data</button>"""

[<Fact>]
let ``hxBrowserIndicator false renders hx-browser-indicator=false`` () =
    button().hxGet("/api/data").hxBrowserIndicator(false) { "Load Data" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/api/data" hx-browser-indicator="false">Load Data</button>"""

// ─── hx-history-cache ───

[<Fact>]
let ``hxHistory false renders hx-history=false`` () =
    div().hxHistory(false) { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-history="false">content</div>"""

// ─── hx-csp ───

[<Fact>]
let ``hxNonce renders hx-nonce`` () =
    button().hxPost("/save").hxNonce("abc123") { "Save" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/save" hx-nonce="abc123">Save</button>"""

// ─── hx-live ───

[<Fact>]
let ``hxLive renders hx-live`` () =
    output().hxLive("this.textContent = 'hello'")
    |> Render.toString
    |> shouldEqual """<output hx-live="this.textContent = &#39;hello&#39;"></output>"""

// ─── hx-download / hx-upsert swap helpers ───

[<Fact>]
let ``HxSwapMethod.download composes with hxSwap`` () =
    button().hxGet("/files/report.pdf").hxSwap(HxSwapMethod.download) { "Download" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/files/report.pdf" hx-swap="download">Download</button>"""

[<Fact>]
let ``HxUpsertModifier.sort composes with upsert swap`` () =
    div().hxGet("/items").hxSwap(HxSwapMethod.upsert + HxUpsertModifier.sort) { "list" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/items" hx-swap="upsert sort">list</div>"""

[<Fact>]
let ``HxUpsertModifier combined renders upsert sort:desc prepend`` () =
    div().hxGet("/items").hxSwap(HxSwapMethod.upsert + HxUpsertModifier.sortDesc + HxUpsertModifier.prepend) { "list" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/items" hx-swap="upsert sort:desc prepend">list</div>"""

[<Fact>]
let ``HxUpsertModifier.key renders key:attr`` () =
    div()
        .hxGet("/items")
        .hxSwap(
            HxSwapMethod.upsert
            + HxUpsertModifier.key "data-priority"
            + HxUpsertModifier.sort
        ) {
        "list"
    }
    |> Render.toString
    |> shouldEqual """<div hx-get="/items" hx-swap="upsert key:data-priority sort">list</div>"""

// ─── null suppression ───

[<Fact>]
let ``null hxSseConnect does not render attribute`` () =
    div().hxSseConnect(null) { "content" }
    |> Render.toString
    |> shouldEqual """<div>content</div>"""

// ─── extension headers ───

[<Fact>]
let ``extension request headers expose correct names`` () =
    HxRequestHeader.Preloaded |> shouldEqual "HX-Preloaded"
    HxRequestHeader.PTag |> shouldEqual "HX-PTag"

[<Fact>]
let ``extension response headers expose correct names`` () =
    HxResponseHeader.Download |> shouldEqual "HX-Download"
    HxResponseHeader.PTag |> shouldEqual "HX-PTag"
    HxResponseHeader.RequestId |> shouldEqual "HX-Request-ID"
