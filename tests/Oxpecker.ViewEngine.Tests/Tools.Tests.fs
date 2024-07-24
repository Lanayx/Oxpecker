module Tools.Tests

open Oxpecker.ViewEngine.Tools
open Xunit
open FsUnitTyped

[<Fact>]
let ``Custom queue works well`` () =
    let mutable customQueue = CustomQueue()
    customQueue.Enqueue(1)
    customQueue.Enqueue(2)
    customQueue.Enqueue(3)
    let result = customQueue.AsEnumerable() |> Seq.toArray
    result |> shouldEqual [| 1; 2; 3 |]

[<Fact>]
let ``Empty custom queue works well`` () =
    let customQueue = CustomQueue()
    let result = customQueue.AsEnumerable() |> Seq.toArray
    result |> shouldEqual [||]
