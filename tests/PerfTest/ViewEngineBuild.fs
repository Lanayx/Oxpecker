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
    // .NET SDK 8.0.200
    //   [Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2 DEBUG
    //   DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2
    //
    //
    // | Method            | Mean     | Error     | StdDev    | Gen0   | Allocated |
    // |------------------ |---------:|----------:|----------:|-------:|----------:|
    // | BuildOxpeckerView | 1.443 us | 0.0284 us | 0.0292 us | 1.3638 |   5.57 KB |
    // | BuildGiraffeView  | 1.661 us | 0.0332 us | 0.0341 us | 1.0338 |   4.23 KB |



    [<Benchmark>]
    member this.BuildOxpeckerView() = OxpeckerViewBuild.get()

    [<Benchmark>]
    member this.BuildGiraffeView() = GiraffeViewBuild.get()
