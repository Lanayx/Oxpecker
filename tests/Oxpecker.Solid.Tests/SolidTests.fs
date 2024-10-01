module SolidTests

open Xunit
open Oxpecker.Solid.Tests.Common

[<Fact>]
let ``Signal`` () =
    runSolidCase "Signal"
