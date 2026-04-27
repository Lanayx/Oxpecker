module Attributes.Tests

open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Xunit
open FsUnit.Light

// ─── Verb attributes ───

[<Fact>]
let ``hxGet renders hx-get`` () =
    div().hxGet("/api") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/api">content</div>"""

[<Fact>]
let ``hxPost renders hx-post`` () =
    button().hxPost("/submit") { "Submit" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/submit">Submit</button>"""

[<Fact>]
let ``hxPut renders hx-put`` () =
    button().hxPut("/update") { "Update" }
    |> Render.toString
    |> shouldEqual """<button hx-put="/update">Update</button>"""

[<Fact>]
let ``hxPatch renders hx-patch`` () =
    button().hxPatch("/patch") { "Patch" }
    |> Render.toString
    |> shouldEqual """<button hx-patch="/patch">Patch</button>"""

[<Fact>]
let ``hxDelete renders hx-delete`` () =
    button().hxDelete("/item/1") { "Delete" }
    |> Render.toString
    |> shouldEqual """<button hx-delete="/item/1">Delete</button>"""

// ─── Core inheritable attributes ───

[<Fact>]
let ``hxSwap renders hx-swap`` () =
    div().hxGet("/data").hxSwap("outerHTML") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/data" hx-swap="outerHTML">content</div>"""

[<Fact>]
let ``hxTarget renders hx-target`` () =
    button().hxGet("/data").hxTarget("#result") { "Load" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/data" hx-target="#result">Load</button>"""

[<Fact>]
let ``hxTrigger renders hx-trigger`` () =
    input().hxGet("/search").hxTrigger("keyup delay:200ms changed")
    |> Render.toString
    |> shouldEqual """<input hx-get="/search" hx-trigger="keyup delay:200ms changed">"""

[<Fact>]
let ``hxSelect renders hx-select`` () =
    div().hxGet("/page").hxSelect(".content") { "Loading..." }
    |> Render.toString
    |> shouldEqual """<div hx-get="/page" hx-select=".content">Loading...</div>"""

[<Fact>]
let ``hxVals renders hx-vals`` () =
    button().hxPost("/action").hxVals("""{"key":"value"}""") { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/action" hx-vals="{&quot;key&quot;:&quot;value&quot;}">Go</button>"""

[<Fact>]
let ``hxOn renders hx-on:event`` () =
    input(type' = "submit", value = "Click").hxOn("click", "alert('hi')")
    |> Render.toString
    |> shouldEqual """<input type="submit" value="Click" hx-on:click="alert(&#39;hi&#39;)">"""

// ─── Additional inheritable attributes ───

[<Fact>]
let ``hxSwapOob renders hx-swap-oob`` () =
    div().hxSwapOob("true") { "OOB" }
    |> Render.toString
    |> shouldEqual """<div hx-swap-oob="true">OOB</div>"""

[<Fact>]
let ``hxPushUrl renders hx-push-url`` () =
    a().hxGet("/page").hxPushUrl("true") { "Link" }
    |> Render.toString
    |> shouldEqual """<a hx-get="/page" hx-push-url="true">Link</a>"""

[<Fact>]
let ``hxInclude renders hx-include`` () =
    button().hxPost("/submit").hxInclude(HxSelector.closest "form") { "Submit" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/submit" hx-include="closest form">Submit</button>"""

[<Fact>]
let ``hxSelectOob renders hx-select-oob`` () =
    div().hxGet("/data").hxSelectOob("#sidebar") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/data" hx-select-oob="#sidebar">content</div>"""

[<Fact>]
let ``hxBoost true renders hx-boost=true`` () =
    body().hxBoost("true") { "content" }
    |> Render.toString
    |> shouldEqual """<body hx-boost="true">content</body>"""

[<Fact>]
let ``hxBoost false renders hx-boost=false`` () =
    a(href = "/file").hxBoost("false") { "Download" }
    |> Render.toString
    |> shouldEqual """<a href="/file" hx-boost="false">Download</a>"""

[<Fact>]
let ``hxReplaceUrl renders hx-replace-url`` () =
    div().hxGet("/data").hxReplaceUrl("true") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/data" hx-replace-url="true">content</div>"""

[<Fact>]
let ``hxConfirm renders hx-confirm`` () =
    button().hxDelete("/item").hxConfirm("Are you sure?") { "Delete" }
    |> Render.toString
    |> shouldEqual """<button hx-delete="/item" hx-confirm="Are you sure?">Delete</button>"""

[<Fact>]
let ``hxDisable renders hx-disable with CSS selector`` () =
    button().hxPost("/submit").hxDisable(HxSelector.this') { "Submit" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/submit" hx-disable="this">Submit</button>"""

[<Fact>]
let ``hxDisable with merge renders hx-disable:merge`` () =
    main().hxDisable(HxSelector.find "button", HxModifier.merge) { "content" }
    |> Render.toString
    |> shouldEqual """<main hx-disable:merge="find button">content</main>"""

[<Fact>]
let ``hxPreserve true renders hx-preserve`` () =
    div().hxPreserve(true) { "Preserved" }
    |> Render.toString
    |> shouldEqual """<div hx-preserve>Preserved</div>"""

[<Fact>]
let ``hxPreserve false does not render`` () =
    div().hxPreserve(false) { "Not preserved" }
    |> Render.toString
    |> shouldEqual """<div>Not preserved</div>"""

[<Fact>]
let ``hxHeaders renders hx-headers`` () =
    button().hxGet("/api").hxHeaders("""{"X-Custom":"val"}""") { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/api" hx-headers="{&quot;X-Custom&quot;:&quot;val&quot;}">Go</button>"""

[<Fact>]
let ``hxIndicator renders hx-indicator`` () =
    input().hxGet("/search").hxIndicator("#spinner")
    |> Render.toString
    |> shouldEqual """<input hx-get="/search" hx-indicator="#spinner">"""

[<Fact>]
let ``hxSync renders hx-sync`` () =
    button().hxPost("/action").hxSync("closest form:abort") { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/action" hx-sync="closest form:abort">Go</button>"""

[<Fact>]
let ``hxPreload renders hx-preload`` () =
    a(href = "/page").hxPreload("mouseover") { "Hover me" }
    |> Render.toString
    |> shouldEqual """<a href="/page" hx-preload="mouseover">Hover me</a>"""

[<Fact>]
let ``hxValidate renders hx-validate`` () =
    form().hxPost("/submit").hxValidate(true) { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-post="/submit" hx-validate>form</form>"""

[<Fact>]
let ``hxEncoding renders hx-encoding`` () =
    form().hxPost("/upload").hxEncoding("multipart/form-data") { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-post="/upload" hx-encoding="multipart/form-data">form</form>"""

// ─── Non-inheritable additional attributes ───

[<Fact>]
let ``hxAction renders hx-action`` () =
    form().hxAction("/api/users") { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-action="/api/users">form</form>"""

[<Fact>]
let ``hxMethod renders hx-method`` () =
    form().hxAction("/api").hxMethod("PUT") { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-action="/api" hx-method="PUT">form</form>"""

[<Fact>]
let ``hxConfig renders hx-config`` () =
    button().hxPost("/slow").hxConfig("""{"timeout":30000}""") { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/slow" hx-config="{&quot;timeout&quot;:30000}">Go</button>"""

[<Fact>]
let ``hxIgnore true renders hx-ignore`` () =
    div().hxIgnore(true) { "Ignored" }
    |> Render.toString
    |> shouldEqual """<div hx-ignore>Ignored</div>"""

[<Fact>]
let ``hxIgnore false does not render`` () =
    div().hxIgnore(false) { "Not ignored" }
    |> Render.toString
    |> shouldEqual """<div>Not ignored</div>"""

[<Fact>]
let ``hxOptimistic renders hx-optimistic`` () =
    button().hxPost("/save").hxOptimistic("#template-id") { "Save" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/save" hx-optimistic="#template-id">Save</button>"""

// ─── Inheritance modifiers ───

[<Fact>]
let ``hxBoost with "inherited" modifier renders hx-boost:inherited`` () =
    body().hxBoost("true", HxModifier.inherited) { "content" }
    |> Render.toString
    |> shouldEqual """<body hx-boost:inherited="true">content</body>"""

[<Fact>]
let ``hxInclude with "inherited:append" modifier renders hx-include:inherited:append`` () =
    form().hxInclude(".extra", HxModifier.inheritedAppend) { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-include:inherited:append=".extra">form</form>"""

[<Fact>]
let ``hxDisable with "merge" renders :merge`` () =
    main().hxDisable(HxSelector.find "button", HxModifier.merge) { "content" }
    |> Render.toString
    |> shouldEqual """<main hx-disable:merge="find button">content</main>"""

[<Fact>]
let ``hxDisable with combined ":merge:inherited" suffix`` () =
    main().hxDisable(HxSelector.find "button", ":merge:inherited") { "content" }
    |> Render.toString
    |> shouldEqual """<main hx-disable:merge:inherited="find button">content</main>"""

[<Fact>]
let ``hxTarget with "inherited" modifier renders hx-target:inherited`` () =
    body().hxTarget("#main", HxModifier.inherited) { "content" }
    |> Render.toString
    |> shouldEqual """<body hx-target:inherited="#main">content</body>"""

// ─── hxStatus ───

[<Fact>]
let ``hxStatus renders hx-status:code`` () =
    form().hxPost("/save").hxStatus("422", "swap:innerHTML target:#errors") { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-post="/save" hx-status:422="swap:innerHTML target:#errors">form</form>"""

[<Fact>]
let ``hxStatus wildcard renders hx-status:5xx`` () =
    div().hxGet("/data").hxStatus("5xx", "swap:none") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/data" hx-status:5xx="swap:none">content</div>"""

// ─── Multiple chained extension methods ───

[<Fact>]
let ``multiple chained htmx extensions`` () =
    div().hxGet("/api").hxTarget("#out").hxSwap("outerHTML").hxTrigger("click") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/api" hx-target="#out" hx-swap="outerHTML" hx-trigger="click">content</div>"""

// ─── null suppression ───

[<Fact>]
let ``null hxGet does not render attribute`` () =
    div().hxGet(null) { "content" }
    |> Render.toString
    |> shouldEqual """<div>content</div>"""

// ─── combination with other attributes ───

[<Fact>]
let ``hx attribute get combined with normal attributes`` () =
    div(id = "1").hxGet("/api") { "content" }
    |> Render.toString
    |> shouldEqual """<div id="1" hx-get="/api">content</div>"""

// ─── HxSelector helpers ───

[<Fact>]
let ``HxSelector.this' renders this`` () =
    button().hxPost("/status").hxTarget(HxSelector.this') { "Check" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/status" hx-target="this">Check</button>"""

[<Fact>]
let ``HxSelector.body renders body`` () =
    a().hxGet("/page").hxTarget(HxSelector.body) { "Load" }
    |> Render.toString
    |> shouldEqual """<a hx-get="/page" hx-target="body">Load</a>"""

[<Fact>]
let ``HxSelector.nextSibling renders next`` () =
    button().hxGet("/more").hxTarget(HxSelector.nextSibling) { "Load More" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/more" hx-target="next">Load More</button>"""

[<Fact>]
let ``HxSelector.previousSibling renders previous`` () =
    button().hxGet("/more").hxTarget(HxSelector.previousSibling) { "Load More" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/more" hx-target="previous">Load More</button>"""

[<Fact>]
let ``HxSelector.closest renders closest`` () =
    button().hxDelete("/item/1").hxTarget(HxSelector.closest ".card") { "Delete" }
    |> Render.toString
    |> shouldEqual """<button hx-delete="/item/1" hx-target="closest .card">Delete</button>"""

[<Fact>]
let ``HxSelector.find renders find`` () =
    div().hxGet("/user").hxTarget(HxSelector.find ".username") { "Loading" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/user" hx-target="find .username">Loading</div>"""

[<Fact>]
let ``HxSelector.findAll renders findAll`` () =
    div().hxGet("/items").hxTarget(HxSelector.findAll ".item") { "items" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/items" hx-target="findAll .item">items</div>"""

[<Fact>]
let ``HxSelector.next renders next selector`` () =
    button().hxGet("/data").hxTarget(HxSelector.next ".results") { "Load" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/data" hx-target="next .results">Load</button>"""

[<Fact>]
let ``HxSelector.previous renders previous selector`` () =
    button().hxGet("/data").hxTarget(HxSelector.previous ".results") { "Load" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/data" hx-target="previous .results">Load</button>"""

[<Fact>]
let ``HxSelector.global' renders global selector`` () =
    button().hxGet("/data").hxTarget(HxSelector.global' "#results") { "Load" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/data" hx-target="global #results">Load</button>"""

[<Fact>]
let ``HxSelector helpers compose with hxInclude`` () =
    button().hxPost("/save").hxInclude(HxSelector.closest "form") { "Save" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/save" hx-include="closest form">Save</button>"""

[<Fact>]
let ``HxSelector helpers compose with hxDisable`` () =
    form().hxPost("/x").hxDisable(HxSelector.find "button") { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-post="/x" hx-disable="find button">form</form>"""
