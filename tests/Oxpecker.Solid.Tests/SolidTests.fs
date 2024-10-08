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
let ``Iterators`` () =
    runSolidCase "Iterators"

[<Fact>]
let ``Children`` () =
    runSolidCase "Children"
