module SolidTests

open Xunit
open Oxpecker.Solid.Tests.Common

[<Fact>]
let ``Signal`` () =
    runSolidCase "Signal"

[<Fact>]
let ``Components`` () =
    runSolidCase "Components"

[<Fact>]
let ``ForLoop`` () =
    runSolidCase "ForLoop"
