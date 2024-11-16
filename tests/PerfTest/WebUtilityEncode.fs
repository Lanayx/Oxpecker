namespace PerfTest

open System.Net
open System.Text
open BenchmarkDotNet.Attributes
open Oxpecker.ViewEngine.Tools

[<MemoryDiagnoser>]
type WebUtilityEncode() =

    let testText = (Seq.init 10000 (fun _ -> "Some long error text")) |> String.concat "<script/>"

    [<Benchmark>]
    member this.WebUtility() =
        let sb = StringBuilder()
        WebUtility.HtmlEncode testText
        |> sb.Append
        |> ignore

    [<Benchmark>]
    member this.Tools() =
        let sb = StringBuilder()
        CustomWebUtility.htmlEncode testText sb

// with hack:
// BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4391/23H2/2023Update/SunValley3)
// AMD Ryzen 5 5600H with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
// .NET SDK 9.0.100
//   [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2 DEBUG
//   DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
//
//
// | Method     | Mean     | Error   | StdDev  | Gen0     | Gen1     | Gen2     | Allocated |
// |----------- |---------:|--------:|--------:|---------:|---------:|---------:|----------:|
// | WebUtility | 696.0 us | 2.37 us | 2.10 us | 332.0313 | 332.0313 | 332.0313 |   1370 KB |
// | Tools      | 498.0 us | 3.03 us | 2.83 us |  83.9844 |  68.3594 |        - |  691.6 KB |

// without hack:
// BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4391/23H2/2023Update/SunValley3)
// AMD Ryzen 5 5600H with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
// .NET SDK 9.0.100
//   [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2 DEBUG
//   DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
//
//
// | Method     | Mean     | Error   | StdDev  | Gen0     | Gen1     | Gen2     | Allocated |
// |----------- |---------:|--------:|--------:|---------:|---------:|---------:|----------:|
// | WebUtility | 695.5 us | 4.14 us | 3.87 us | 332.0313 | 332.0313 | 332.0313 |   1370 KB |
// | Tools      | 668.1 us | 2.86 us | 2.39 us |  83.9844 |  68.3594 |        - |  691.6 KB |

