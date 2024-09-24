module Oxpecker.Solid.Tests.Common

open Fli
open FsUnitTyped
open System.IO

let runCase caseName =
    let dir = $"{__SOURCE_DIRECTORY__}/Cases/{caseName}"
    cli {
        Shell CMD
        WorkingDirectory dir
        Command "dotnet fable --noCache --exclude Oxpecker.Solid.FablePlugin"
        WindowStyle Maximized
    }
    |> Command.execute
    |> Output.toExitCode
    |> shouldEqual 0

    let result = File.ReadAllText($"{dir}/{caseName}.fs.js")
    let expected = File.ReadAllText($"{dir}/{caseName}.expected")
    result |> shouldEqual expected
