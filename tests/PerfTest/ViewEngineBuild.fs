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
                    for _ in 1..10 do
                        li() { span() { "Test" } }
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
                    for _ in 1..10 do
                        li [] [ span [] [ str "Test" ] ]
                ]
            ]
        ]

[<MemoryDiagnoser>]
type ViewEngineBuild() =

    // BenchmarkDotNet v0.13.12, Windows 10
    // AMD Ryzen 7 2700X, 1 CPU, 16 logical and 8 physical cores
    // .NET SDK 8.0.200
    //   [Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2 DEBUG
    //   DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2
    //
    //
    // | Method            | Mean     | Error    | StdDev   | Gen0   | Allocated |
    // |------------------ |---------:|---------:|---------:|-------:|----------:|
    // | BuildOxpeckerView | 590.8 ns | 11.51 ns | 17.58 ns | 0.7992 |   3.27 KB |
    // | BuildGiraffeView  | 788.9 ns | 14.91 ns | 13.95 ns | 0.6323 |   2.59 KB |



    [<Benchmark>]
    member this.BuildOxpeckerView() = OxpeckerViewBuild.get()

    [<Benchmark>]
    member this.BuildGiraffeView() = GiraffeViewBuild.get()
