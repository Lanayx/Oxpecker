module Tests

open System
open System.IO
open Xunit
open FsUnitTyped
open Fli
open Oxpecker.Solid.Tests.Common

[<Fact>]
let ``Fable version`` () =
    cli {
        Shell CMD
        Command "dotnet fable --version"
    }
    |> Command.execute
    |> Output.toText
    |> shouldEqual "4.21.0"

[<Fact>]
let ``Hello world`` () =
    runCase "HelloWorld"

[<Fact>]
let ``Nested tags`` () =
    runCase "NestedTags"
