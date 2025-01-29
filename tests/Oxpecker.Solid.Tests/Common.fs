module Oxpecker.Solid.Tests.Common

open Fli
open FsUnitTyped
open System.IO

let private runCase folderName caseName =
    let dir = $"{__SOURCE_DIRECTORY__}/{folderName}/{caseName}"
    cli {
        Shell CMD
        WorkingDirectory dir
        Command "dotnet fable --extension .jsx"
    }
    |> Command.execute
    |> Output.toExitCode
    |> shouldEqual 0

    let result = File.ReadAllLines($"{dir}/{caseName}.jsx")
    let expected = File.ReadAllLines($"{dir}/{caseName}.expected")
    result |> shouldEqual expected

let runGeneralCase caseName =
    let folderName = "Cases"
    runCase folderName caseName

let runSolidCase caseName =
    let folderName = "SolidCases"
    runCase folderName caseName
