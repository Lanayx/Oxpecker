open BenchmarkDotNet.Running
open PerfTest

[<EntryPoint>]
let main args =
    let summary = BenchmarkRunner.Run<ModelBinding>()
    //Form().OxpeckerPost().Wait()

    0
