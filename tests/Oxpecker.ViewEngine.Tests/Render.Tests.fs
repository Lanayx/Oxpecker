module Render.Tests

open System
open System.IO
open System.Text
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria
open Xunit
open FsUnit.Light

[<Fact>]
let ``Basic test`` () =
    let result =
        html() {
            div(id = "1") { 1 }
            div(id = "2") {
                let x = 2
                div(id = "3", class' = "test")
                for _ in 1..x do
                    br()
                div(id = "4")
            }
        }
    result
    |> Render.toString
    |> shouldEqual
        """<html><div id="1">1</div><div id="2"><div id="3" class="test"></div><br><br><div id="4"></div></div></html>"""

[<Fact>]
let ``Basic test to bytes`` () =
    let result =
        html() {
            div(id = "1") { 1 }
            div(id = "2") {
                let x = 2
                div(id = "3", class' = "test")
                for _ in 1..x do
                    br()
                div(id = "4")
            }
        }
    result
    |> Render.toBytes
    |> Encoding.UTF8.GetString
    |> shouldEqual
        """<html><div id="1">1</div><div id="2"><div id="3" class="test"></div><br><br><div id="4"></div></div></html>"""


[<Fact>]
let ``Optional attribute`` () =
    let value = true
    let result =
        div(id = if value then null else "abc") { div(id = if value then "myid" else null) { br() } }
    result
    |> Render.toString
    |> shouldEqual """<div><div id="myid"><br></div></div>"""


[<Fact>]
let ``Data attribute`` () =
    let result =
        div().attr("data-my-value", "sdf").attr("data-my-value2", "abc") { "Some text" }
    result
    |> Render.toString
    |> shouldEqual """<div data-my-value="sdf" data-my-value2="abc">Some text</div>"""

[<Fact>]
let ``Encode test`` () =
    let result =
        p(id = "<br>") {
            raw "<hr>"
            span() { "<hr>" }
        }
    result
    |> Render.toString
    |> shouldEqual """<p id="&lt;br&gt;"><hr><span>&lt;hr&gt;</span></p>"""

[<Fact>]
let ``Aria test`` () =
    let result =
        span(
            role = "checkbox",
            id = "checkBoxInput",
            ariaChecked = "false",
            tabindex = 0,
            ariaLabelledBy = "chk15-label"
        )
    result
    |> Render.toString
    |> shouldEqual
        """<span role="checkbox" id="checkBoxInput" aria-checked="false" tabindex="0" aria-labelledby="chk15-label"></span>"""

[<Fact>]
let ``Only children test`` () =
    let result =
        Fragment() {
            div(id = "1") { "Hello" }
            Fragment() {
                div(id = "2") { "World" }
                div(id = "3") {
                    Fragment()
                    "!"
                }
            }
        }
    result
    |> Render.toString
    |> shouldEqual """<div id="1">Hello</div><div id="2">World</div><div id="3">!</div>"""

[<Fact>]
let ``yield! test`` () =
    let elements = [ li() { "one" }; li() { "two" }; li() { "three" } ]

    let result = ul() { yield! elements }
    result
    |> Render.toString
    |> shouldEqual """<ul><li>one</li><li>two</li><li>three</li></ul>"""

[<Fact>]
let ``Double render works`` () =
    let test = span(id = "test1") { "test2" }
    let result1 = test |> Render.toString
    let result2 = test |> Render.toString
    result1 |> shouldEqual """<span id="test1">test2</span>"""
    result2 |> shouldEqual """<span id="test1">test2</span>"""

[<Fact>]
let ``Boolean attribute method`` () =
    let result =
        button().bool("required", true).bool("disabled", true).bool("novalidate", false) { "Test" }
    result
    |> Render.toString
    |> shouldEqual """<button required disabled>Test</button>"""

[<Fact>]
let ``Boolean property helpers`` () =
    button(autofocus = true, disabled = false) { "Test" }
    |> Render.toString
    |> shouldEqual """<button autofocus>Test</button>"""

[<Fact>]
let ``Basic chunked test`` () =
    task {
        let view = html() { div(id = "1") }
        use stream = new MemoryStream()
        do! Render.toStreamAsync stream view
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        stream.ToArray()
        |> Encoding.UTF8.GetString
        |> shouldEqual """<html><div id="1"></div></html>"""
    }

[<Fact>]
let ``Render to text writer`` () =
    task {
        let view = html() { div(id = "1") }
        let stream = new MemoryStream()
        let textWriter = new StreamWriter(stream, leaveOpen = true)
        do! Render.toHtmlDocTextWriterAsync textWriter view
        do! textWriter.DisposeAsync()
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        stream.ToArray()
        |> Encoding.UTF8.GetString
        |> shouldEqual $"""<!DOCTYPE html>{Environment.NewLine}<html><div id="1"></div></html>"""
    }

[<Fact>]
let ``Prerender renders the same as the original element`` () =
    let view =
        div(id = "1") {
            span(class' = "a") { "Hello" }
            br()
        }
    let expected = view |> Render.toString
    prerender view
    |> Render.toString
    |> shouldEqual expected

[<Fact>]
let ``Prerender includes all children`` () =
    let result =
        html() {
            div(id = "1") { 1 }
            div(id = "2") {
                div(id = "3", class' = "test")
                br()
                ul() { yield! [ li() { "one" }; li() { "two" } ] }
            }
        }
        |> prerender
    result
    |> Render.toString
    |> shouldEqual
        """<html><div id="1">1</div><div id="2"><div id="3" class="test"></div><br><ul><li>one</li><li>two</li></ul></div></html>"""

[<Fact>]
let ``Prerendered node is not escaped again when embedded`` () =
    let prerendered =
        p(id = "<br>") {
            raw "<hr>"
            span() { "<hr>" }
        }
        |> prerender
    let result = div() { prerendered }
    result
    |> Render.toString
    |> shouldEqual """<div><p id="&lt;br&gt;"><hr><span>&lt;hr&gt;</span></p></div>"""

[<Fact>]
let ``Prerendered node is embedded without a wrapper`` () =
    let header = h1() { "My site" } |> prerender
    let result = html() { body() { header } }
    result
    |> Render.toString
    |> shouldEqual """<html><body><h1>My site</h1></body></html>"""

[<Fact>]
let ``Prerender of a fragment renders children only`` () =
    let result =
        Fragment() {
            span() { "one" }
            span() { "two" }
        }
        |> prerender
    result |> Render.toString |> shouldEqual """<span>one</span><span>two</span>"""

[<Fact>]
let ``Prerender takes an eager snapshot`` () =
    let view = div() { span() { "early" } }
    let prerendered = prerender view
    view.AddChild(span() { "late" })
    prerendered
    |> Render.toString
    |> shouldEqual """<div><span>early</span></div>"""
    view
    |> Render.toString
    |> shouldEqual """<div><span>early</span><span>late</span></div>"""

[<Fact>]
let ``Double render of a prerendered node works`` () =
    let prerendered = prerender(span(id = "test1") { "test2" })
    let result1 = prerendered |> Render.toString
    let result2 = prerendered |> Render.toString
    result1 |> shouldEqual """<span id="test1">test2</span>"""
    result2 |> shouldEqual """<span id="test1">test2</span>"""

[<Fact>]
let ``Prerendered template renders the same as the original view`` () =
    let layout =
        prerenderAround(fun content ->
            html() {
                body() {
                    h1() { "My site" }
                    main() { content }
                }
            })
    let expected =
        html() {
            body() {
                h1() { "My site" }
                main() { p() { "Dynamic" } }
            }
        }
        |> Render.toString
    layout() { p() { "Dynamic" } } |> Render.toString |> shouldEqual expected

[<Fact>]
let ``Prerendered template accepts several children`` () =
    let layout = prerenderAround(fun content -> div(id = "wrap") { content })
    layout() {
        span() { "one" }
        span() { "two" }
    }
    |> Render.toString
    |> shouldEqual """<div id="wrap"><span>one</span><span>two</span></div>"""

[<Fact>]
let ``Prerendered template can be filled more than once`` () =
    let layout = prerenderAround(fun content -> div() { content })
    let first = layout() { "one" }
    let second = layout() { "two" }
    first |> Render.toString |> shouldEqual """<div>one</div>"""
    second |> Render.toString |> shouldEqual """<div>two</div>"""

[<Fact>]
let ``Prerendered template with an unfilled hole`` () =
    let layout = prerenderAround(fun content -> div() { content })
    layout() |> Render.toString |> shouldEqual """<div></div>"""

[<Fact>]
let ``Prerendered template still escapes the hole content`` () =
    let layout = prerenderAround(fun content -> p(id = "<br>") { content })
    layout() { "<hr>" }
    |> Render.toString
    |> shouldEqual """<p id="&lt;br&gt;">&lt;hr&gt;</p>"""

[<Fact>]
let ``Prerendered template with an empty prefix and suffix`` () =
    let layout = prerenderAround(fun content -> Fragment() { content })
    layout() { span() { "only" } }
    |> Render.toString
    |> shouldEqual """<span>only</span>"""

[<Fact>]
let ``Prerendered template hole accepts a for loop`` () =
    let layout = prerenderAround(fun content -> ul() { content })
    layout() {
        for i in 1..3 do
            li() { i }
    }
    |> Render.toString
    |> shouldEqual """<ul><li>1</li><li>2</li><li>3</li></ul>"""

[<Fact>]
let ``Prerendered templates can be nested`` () =
    let outer = prerenderAround(fun content -> html() { body() { content } })
    let inner = prerenderAround(fun content -> main(class' = "c") { content })
    outer() { inner() { p() { "text" } } }
    |> Render.toString
    |> shouldEqual """<html><body><main class="c"><p>text</p></main></body></html>"""

[<Fact>]
let ``Prerendered template requires the hole to be used`` () =
    Assert.Throws<ArgumentException>(fun () -> prerenderAround(fun _ -> div() { "no hole" }) |> ignore)
    |> ignore

[<Fact>]
let ``Prerendered template rejects a hole used twice`` () =
    Assert.Throws<ArgumentException>(fun () ->
        prerenderAround(fun content ->
            div() {
                content
                content
            })
        |> ignore)
    |> ignore
