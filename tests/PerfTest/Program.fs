﻿open BenchmarkDotNet.Running
open PerfTest

[<EntryPoint>]
let main args =
    let summary = BenchmarkRunner.Run<Routing>()

    0
