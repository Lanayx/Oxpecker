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
    |> shouldEqual "4.23.0"

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

[<Fact>]
let ``Custom attributes`` () =
    runGeneralCase "CustomAttributes"

[<Fact>]
let ``Custom tags`` () =
    runGeneralCase "CustomTags"

[<Fact>]
let ``Let bindings`` () =
    let dir = $"{__SOURCE_DIRECTORY__}/Cases/LetBindings"
    let output =
        cli {
            Shell CMD
            WorkingDirectory dir
            Command "dotnet fable --noCache --exclude Oxpecker.Solid.FablePlugin"
        }
        |> Command.execute
    output |> Output.toExitCode |> shouldEqual 1
    output |> Output.toText |> shouldContainText """`let` binding inside HTML CE can't be converted to JSX:line 9"""

