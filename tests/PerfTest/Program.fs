﻿open BenchmarkDotNet.Running
open PerfTest

[<EntryPoint>]
let main args =
    let summary = BenchmarkRunner.Run<ViewEngineBuild>()

    0
