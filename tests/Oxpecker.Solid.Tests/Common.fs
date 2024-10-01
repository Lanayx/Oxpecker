module Oxpecker.Solid.Tests.Common

open Fli
open FsUnitTyped
open System.IO

let private runCase folderName caseName =
    let dir = $"{__SOURCE_DIRECTORY__}/{folderName}/{caseName}"
    cli {
        Shell CMD
        WorkingDirectory dir
        Command "dotnet fable --noCache --exclude Oxpecker.Solid.FablePlugin"
    }
    |> Command.execute
    |> Output.toExitCode
    |> shouldEqual 0

    let result = File.ReadAllText($"{dir}/{caseName}.fs.js")
    let expected = File.ReadAllText($"{dir}/{caseName}.expected")
    result |> shouldEqual expected

let runGeneralCase caseName =
    let folderName = "Cases"
    runCase folderName caseName

let runSolidCase caseName =
    let folderName = "SolidCases"
    runCase folderName caseName
