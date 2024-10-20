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

[<Fact>]
let ``Conditional`` () =
    runSolidCase "Conditional"

[<Fact>]
let ``Suspense`` () =
    runSolidCase "Suspense"

[<Fact>]
let ``Refs`` () =
    runSolidCase "Refs"

[<Fact>]
let ``Router`` () =
    runSolidCase "Router"
