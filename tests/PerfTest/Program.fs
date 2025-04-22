open BenchmarkDotNet.Running
open PerfTest

[<EntryPoint>]
let main args =
    let summary = BenchmarkRunner.Run<ModelParsing>()
    //ModelBinding().OxpeckerPost().Wait()

    0
