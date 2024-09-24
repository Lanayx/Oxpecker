module Tests

open System
open System.IO
open Xunit
open FsUnitTyped
open Fli

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
    let dir = $"{__SOURCE_DIRECTORY__}/Cases/HelloWorld"
    cli {
        Shell CMD
        WorkingDirectory dir
        Command "dotnet fable"
    }
    |> Command.execute
    |> Output.toExitCode
    |> shouldEqual 0

    let result = File.ReadAllText($"{dir}/HelloWorld.fs.js")
    let expected = File.ReadAllText($"{dir}/HelloWorld.expected")
    result |> shouldEqual expected
