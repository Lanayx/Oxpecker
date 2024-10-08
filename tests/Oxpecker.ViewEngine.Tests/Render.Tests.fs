module Render.Tests

open System
open System.IO
open System.Text
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria
open Xunit
open FsUnitTyped

[<Fact>]
let ``Basic test`` () =
    let result =
        html() {
            div(id = "1")
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
        """<html><div id="1"></div><div id="2"><div id="3" class="test"></div><br><br><div id="4"></div></div></html>"""


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
