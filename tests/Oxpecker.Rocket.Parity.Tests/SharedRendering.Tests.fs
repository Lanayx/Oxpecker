module Rocket.ParityTests

open Xunit

[<Fact>]
let ``shared views match cross-target rendering expectations`` () =
    Assert.All(Rocket.RenderingCases.cases(), fun (_, actual, expected) -> Assert.Equal(expected, actual))
