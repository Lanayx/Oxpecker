module Rocket.RenderingCases

open Oxpecker.ViewEngine
open Oxpecker.Datastar
open Oxpecker.Datastar.Pro

// This exact fixture is compiled against both target assemblies.
let cases () = [
    "text escaping", Render.toString(div() { "<&>\"'" }), "<div>&lt;&amp;&gt;&quot;&#39;</div>"
    "attribute escaping", Render.toString(div(title = "<&>\"'") { }), "<div title=\"&lt;&amp;&gt;&quot;&#39;\"></div>"
    "unicode",
    Render.toString(
        div() {
            "\u00A0\u00FF\U0001F680"
            + System.String([| 0xD800; 120; 0xDC00 |] |> Array.map char)
        }
    ),
    "<div>&#160;&#255;&#128640;\uFFFDx\uFFFD</div>"
    "null attribute", Render.toString(div().dataText(null) { }), "<div></div>"
    "boolean omission", Render.toString(input(disabled = false)), "<input>"
    "boolean presence", Render.toString(input(disabled = true)), "<input disabled>"
    "signals and modifiers",
    Render.toString(button().dataOn("click", "$$count++", DsModifier.once + DsModifier.debounceMs 100) { "+" }),
    "<button data-on:click__once__debounce.100ms=\"$$count++\">+</button>"
    "value form", Render.toString(input().dataBindValue("name")), "<input data-bind=\"name\">"
    "threshold",
    Render.toString(div().dataOnIntersect("$seen = true", DsModifier.threshold 50) { }),
    "<div data-on-intersect__threshold.50=\"$seen = true\"></div>"
    "Pro attribute",
    Render.toString(div().dataPersistKey("cart", DsProModifier.session) { }),
    "<div data-persist:cart__session></div>"
    "loop",
    Render.toString(
        div() {
            for n in [ 1; 2 ] do
                span() { n }
        }
    ),
    "<div><span>1</span><span>2</span></div>"
    "prerender", Render.toString(prerender(span() { "<&" })), "<span>&lt;&amp;</span>"
    "prerenderAround",
    (let layout = prerenderAround(fun hole -> div() { hole })
     Render.toString(layout() { "<&" })),
    "<div>&lt;&amp;</div>"
]
