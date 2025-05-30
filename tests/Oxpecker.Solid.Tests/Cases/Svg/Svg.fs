module Oxpecker.Solid.Tests.Cases.Svg

open Oxpecker.Solid
open Oxpecker.Solid.Svg


[<SolidComponent>]
let SvgTest () =
    svg() {
        defs() {
            linearGradient(id = "gradient", x1 = "0%", y1 = "0%", x2 = "100%", y2 = "0%") {
                stop(offset = "0%", style = "stop-color:rgb(255,255,0);stop-opacity:1")
                stop(offset = "100%", style = "stop-color:rgb(255,0,0);stop-opacity:1")
            }
        }
        circle(id="circle", ``fill-opacity`` = "fillOpacity", ``clip-rule`` = "clipRule")
        "Sorry but this browser does not support inline SVG."
    }
