module Oxpecker.Solid.Tests.Cases.Svg

open Oxpecker.Solid
open Oxpecker.Solid.Svg


[<SolidComponent>]
let SvgTest () =
    circle(fillOpacity = "fillOpacity", ``clip-rule`` = "clipRule")
