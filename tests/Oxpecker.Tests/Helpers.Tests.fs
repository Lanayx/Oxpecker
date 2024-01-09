module Oxpecker.Tests.Helpers

open System
open Xunit
open FsUnitTyped
open Oxpecker.Helpers


[<Fact>]
let ``Test isNotNull`` () =
    "sdf" |> isNotNull |> shouldEqual true
    null |> isNotNull |> shouldEqual false

[<Fact>]
let ``Test composition plus``() =
    let f x = x + 1
    let g x y = x * y
    let h x y = x / y
    (f <<+ g) 1 2 |> shouldEqual 3
    (f <<+ h) 4 2 |> shouldEqual 3

[<Fact>]
let ``Test composition plus plus``() =
    let f x = x + 1
    let g x y z = x * y * z
    let h x y z = x / y / z
    (f <<++ g) 1 2 3 |> shouldEqual 7
    (f <<++ h) 4 2 2 |> shouldEqual 2

[<Fact>]
let ``Test is1xxStatusCode`` () =
    100 |> is1xxStatusCode |> shouldEqual true
    200 |> is1xxStatusCode |> shouldEqual false

[<Fact>]
let ``Test is2xxStatusCode`` () =
    200 |> is2xxStatusCode |> shouldEqual true
    300 |> is2xxStatusCode |> shouldEqual false

[<Fact>]
let ``Test is3xxStatusCode`` () =
    300 |> is3xxStatusCode |> shouldEqual true
    400 |> is3xxStatusCode |> shouldEqual false

[<Fact>]
let ``Test is4xxStatusCode`` () =
    400 |> is4xxStatusCode |> shouldEqual true
    500 |> is4xxStatusCode |> shouldEqual false

[<Fact>]
let ``Test is5xxStatusCode`` () =
    100 |> is5xxStatusCode |> shouldEqual false
    500 |> is5xxStatusCode |> shouldEqual true
