module GeneralTests

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
    |> shouldEqual "4.22.0"

[<Fact>]
let ``Hello world`` () =
    runGeneralCase "HelloWorld"

[<Fact>]
let ``Parameters`` () =
    runGeneralCase "Parameters"

[<Fact>]
let ``Deep nested tags`` () =
    runGeneralCase "DeepNestedTags"

[<Fact>]
let ``Event handler`` () =
    runGeneralCase "EventHandler"

[<Fact>]
let ``Flat nested tags`` () =
    runGeneralCase "FlatNestedTags"
