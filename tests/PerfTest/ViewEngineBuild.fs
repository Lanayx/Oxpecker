namespace PerfTest

open BenchmarkDotNet.Attributes

module OxpeckerViewBuild =
    open Oxpecker.ViewEngine

    let get () =
        html() {
            body(style = "width: 800px; margin: 0 auto") {
                h1(style = "text-align: center; color: red") { "Error" }
                p() { "Some long error text" }
                ul() {
                    for i in 1..10 do
                        li() { span() { $"Test %d{i}" } }
                }
            }
        }

module GiraffeViewBuild =
    open Giraffe.ViewEngine

    let get () =
        html [] [
            body [ _style "width: 800px; margin: 0 auto" ] [
                h1 [ _style "text-align: center; color: red" ] [ str "Error" ]
                p [] [ str "Some long error text" ]
                ul [] [
                    for i in 1..10 do
                        li [] [ span [] [ str $"Test %d{i}" ] ]
                ]
            ]
        ]

[<MemoryDiagnoser>]
type ViewEngineBuild() =

    // BenchmarkDotNet v0.13.12, Windows 10
    // AMD Ryzen 7 2700X, 1 CPU, 16 logical and 8 physical cores
    // .NET SDK 8.0.101
    //   [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2 DEBUG
    //   DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJI

    // | Method            | Mean     | Error     | StdDev    | Gen0   | Allocated |
    // |------------------ |---------:|----------:|----------:|-------:|----------:|
    // | BuildOxpeckerView | 1.512 us | 0.0254 us | 0.0225 us | 1.5049 |   6.15 KB |
    // | BuildGiraffeView  | 1.651 us | 0.0146 us | 0.0137 us | 1.0338 |   4.23 KB |



    [<Benchmark>]
    member this.BuildOxpeckerView() = OxpeckerViewBuild.get()

    [<Benchmark>]
    member this.BuildGiraffeView() = GiraffeViewBuild.get()
