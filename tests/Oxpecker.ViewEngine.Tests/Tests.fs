module Tests

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
            role="checkbox",
            id="checkBoxInput",
            ariaChecked="false",
            tabindex=0,
            ariaLabelledBy="chk15-label"
        )
    result
    |> Render.toString
    |> shouldEqual """<span role="checkbox" id="checkBoxInput" aria-checked="false" tabindex="0" aria-labelledby="chk15-label"></span>"""
