module Attributes.Tests

open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Xunit
open FsUnit.Light

// ─── Verb attributes ───

[<Fact>]
let ``hxGet renders hx-get`` () =
    div().attr(hxGet "/api") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/api">content</div>"""

[<Fact>]
let ``hxPost renders hx-post`` () =
    button().attr(hxPost "/submit") { "Submit" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/submit">Submit</button>"""

[<Fact>]
let ``hxPut renders hx-put`` () =
    button().attr(hxPut "/update") { "Update" }
    |> Render.toString
    |> shouldEqual """<button hx-put="/update">Update</button>"""

[<Fact>]
let ``hxPatch renders hx-patch`` () =
    button().attr(hxPatch "/patch") { "Patch" }
    |> Render.toString
    |> shouldEqual """<button hx-patch="/patch">Patch</button>"""

[<Fact>]
let ``hxDelete renders hx-delete`` () =
    button().attr(hxDelete "/item/1") { "Delete" }
    |> Render.toString
    |> shouldEqual """<button hx-delete="/item/1">Delete</button>"""

// ─── Core inheritable attributes ───

[<Fact>]
let ``hxSwap renders hx-swap`` () =
    div().attr(hxGet "/data", hxSwap "outerHTML") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/data" hx-swap="outerHTML">content</div>"""

[<Fact>]
let ``hxTarget renders hx-target`` () =
    button().attr(hxGet "/data", hxTarget "#result") { "Load" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/data" hx-target="#result">Load</button>"""

[<Fact>]
let ``hxTrigger renders hx-trigger`` () =
    input().attr(hxGet "/search", hxTrigger "keyup delay:200ms changed")
    |> Render.toString
    |> shouldEqual """<input hx-get="/search" hx-trigger="keyup delay:200ms changed">"""

[<Fact>]
let ``hxSelect renders hx-select`` () =
    div().attr(hxGet "/page", hxSelect ".content") { "Loading..." }
    |> Render.toString
    |> shouldEqual """<div hx-get="/page" hx-select=".content">Loading...</div>"""

[<Fact>]
let ``hxVals renders hx-vals`` () =
    button().attr(hxPost "/action", hxVals """{"key":"value"}""") { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/action" hx-vals="{&quot;key&quot;:&quot;value&quot;}">Go</button>"""

[<Fact>]
let ``hxOn renders hx-on:event`` () =
    input(type' = "submit", value = "Click").attr(hxOn("click", "alert('hi')"))
    |> Render.toString
    |> shouldEqual """<input type="submit" value="Click" hx-on:click="alert(&#39;hi&#39;)">"""

// ─── Additional inheritable attributes ───

[<Fact>]
let ``hxSwapOob renders hx-swap-oob`` () =
    div().attr(hxSwapOob "true") { "OOB" }
    |> Render.toString
    |> shouldEqual """<div hx-swap-oob="true">OOB</div>"""

[<Fact>]
let ``hxPushUrl renders hx-push-url`` () =
    a().attr(hxGet "/page", hxPushUrl "true") { "Link" }
    |> Render.toString
    |> shouldEqual """<a hx-get="/page" hx-push-url="true">Link</a>"""

[<Fact>]
let ``hxInclude renders hx-include`` () =
    button().attr(hxPost "/submit", hxInclude "closest form") { "Submit" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/submit" hx-include="closest form">Submit</button>"""

[<Fact>]
let ``hxSelectOob renders hx-select-oob`` () =
    div().attr(hxGet "/data", hxSelectOob "#sidebar") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/data" hx-select-oob="#sidebar">content</div>"""

[<Fact>]
let ``hxBoost true renders hx-boost=true`` () =
    body().attr(hxBoost true) { "content" }
    |> Render.toString
    |> shouldEqual """<body hx-boost="true">content</body>"""

[<Fact>]
let ``hxBoost false renders hx-boost=false`` () =
    a(href = "/file").attr(hxBoost false) { "Download" }
    |> Render.toString
    |> shouldEqual """<a href="/file" hx-boost="false">Download</a>"""

[<Fact>]
let ``hxReplaceUrl renders hx-replace-url`` () =
    div().attr(hxGet "/data", hxReplaceUrl "true") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/data" hx-replace-url="true">content</div>"""

[<Fact>]
let ``hxConfirm renders hx-confirm`` () =
    button().attr(hxDelete "/item", hxConfirm "Are you sure?") { "Delete" }
    |> Render.toString
    |> shouldEqual """<button hx-delete="/item" hx-confirm="Are you sure?">Delete</button>"""

[<Fact>]
let ``hxDisable renders hx-disable with CSS selector`` () =
    button().attr(hxPost "/submit", hxDisable "this") { "Submit" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/submit" hx-disable="this">Submit</button>"""

[<Fact>]
let ``hxDisable with merge renders hx-disable:merge`` () =
    main().attr(hxDisable("find button", merge = true)) { "content" }
    |> Render.toString
    |> shouldEqual """<main hx-disable:merge="find button">content</main>"""

[<Fact>]
let ``hxPreserve true renders hx-preserve`` () =
    div().attr(hxPreserve true) { "Preserved" }
    |> Render.toString
    |> shouldEqual """<div hx-preserve="">Preserved</div>"""

[<Fact>]
let ``hxPreserve false does not render`` () =
    div().attr(hxPreserve false) { "Not preserved" }
    |> Render.toString
    |> shouldEqual """<div>Not preserved</div>"""

[<Fact>]
let ``hxHeaders renders hx-headers`` () =
    button().attr(hxGet "/api", hxHeaders """{"X-Custom":"val"}""") { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-get="/api" hx-headers="{&quot;X-Custom&quot;:&quot;val&quot;}">Go</button>"""

[<Fact>]
let ``hxIndicator renders hx-indicator`` () =
    input().attr(hxGet "/search", hxIndicator "#spinner")
    |> Render.toString
    |> shouldEqual """<input hx-get="/search" hx-indicator="#spinner">"""

[<Fact>]
let ``hxSync renders hx-sync`` () =
    button().attr(hxPost "/action", hxSync "closest form:abort") { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/action" hx-sync="closest form:abort">Go</button>"""

[<Fact>]
let ``hxPreload renders hx-preload`` () =
    a(href = "/page").attr(hxPreload "mouseover") { "Hover me" }
    |> Render.toString
    |> shouldEqual """<a href="/page" hx-preload="mouseover">Hover me</a>"""

[<Fact>]
let ``hxValidate renders hx-validate`` () =
    form().attr(hxPost "/submit", hxValidate true) { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-post="/submit" hx-validate="true">form</form>"""

[<Fact>]
let ``hxEncoding renders hx-encoding`` () =
    form().attr(hxPost "/upload", hxEncoding "multipart/form-data") { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-post="/upload" hx-encoding="multipart/form-data">form</form>"""

// ─── Non-inheritable additional attributes ───

[<Fact>]
let ``hxAction renders hx-action`` () =
    form().attr(hxAction "/api/users") { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-action="/api/users">form</form>"""

[<Fact>]
let ``hxMethod renders hx-method`` () =
    form().attr(hxAction "/api", hxMethod "PUT") { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-action="/api" hx-method="PUT">form</form>"""

[<Fact>]
let ``hxConfig renders hx-config`` () =
    button().attr(hxPost "/slow", hxConfig """{"timeout":30000}""") { "Go" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/slow" hx-config="{&quot;timeout&quot;:30000}">Go</button>"""

[<Fact>]
let ``hxIgnore true renders hx-ignore`` () =
    div().attr(hxIgnore true) { "Ignored" }
    |> Render.toString
    |> shouldEqual """<div hx-ignore="">Ignored</div>"""

[<Fact>]
let ``hxIgnore false does not render`` () =
    div().attr(hxIgnore false) { "Not ignored" }
    |> Render.toString
    |> shouldEqual """<div>Not ignored</div>"""

[<Fact>]
let ``hxOptimistic renders hx-optimistic`` () =
    button().attr(hxPost "/save", hxOptimistic "#template-id") { "Save" }
    |> Render.toString
    |> shouldEqual """<button hx-post="/save" hx-optimistic="#template-id">Save</button>"""

// ─── Inheritance modifiers ───

[<Fact>]
let ``hxBoost with HxInherited.Replace renders hx-boost:inherited`` () =
    body().attr(hxBoost(true, HxInherited.Replace)) { "content" }
    |> Render.toString
    |> shouldEqual """<body hx-boost:inherited="true">content</body>"""

[<Fact>]
let ``hxInclude with HxInherited.Append renders hx-include:inherited:append`` () =
    form().attr(hxInclude(".extra", HxInherited.Append)) { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-include:inherited:append=".extra">form</form>"""

[<Fact>]
let ``hxDisable with merge and inherited combines suffixes`` () =
    main().attr(hxDisable("find button", merge = true, inherited = HxInherited.Replace)) { "content" }
    |> Render.toString
    |> shouldEqual """<main hx-disable:merge:inherited="find button">content</main>"""

[<Fact>]
let ``hxTarget with HxInherited.Replace renders hx-target:inherited`` () =
    body().attr(hxTarget("#main", HxInherited.Replace)) { "content" }
    |> Render.toString
    |> shouldEqual """<body hx-target:inherited="#main">content</body>"""

// ─── hxStatus ───

[<Fact>]
let ``hxStatus renders hx-status:code`` () =
    form().attr(hxPost "/save", hxStatus("422", "swap:innerHTML target:#errors")) { "form" }
    |> Render.toString
    |> shouldEqual """<form hx-post="/save" hx-status:422="swap:innerHTML target:#errors">form</form>"""

[<Fact>]
let ``hxStatus wildcard renders hx-status:5xx`` () =
    div().attr(hxGet "/data", hxStatus("5xx", "swap:none")) { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/data" hx-status:5xx="swap:none">content</div>"""

// ─── Multiple typed attributes in one .attr(...) call ───

[<Fact>]
let ``multiple typed attributes in one attr call`` () =
    div().attr(hxGet "/api", hxTarget "#out", hxSwap "outerHTML", hxTrigger "click") { "content" }
    |> Render.toString
    |> shouldEqual """<div hx-get="/api" hx-target="#out" hx-swap="outerHTML" hx-trigger="click">content</div>"""

// ─── null suppression ───

[<Fact>]
let ``null hxGet does not render attribute`` () =
    div().attr(hxGet null) { "content" }
    |> Render.toString
    |> shouldEqual """<div>content</div>"""

// ─── combination with other attributes ───

[<Fact>]
let ``hx attribute get combined with normal attributes`` () =
    div(id="1").attr(hxGet "/api") { "content" }
    |> Render.toString
    |> shouldEqual """<div id="1" hx-get="/api">content</div>"""
