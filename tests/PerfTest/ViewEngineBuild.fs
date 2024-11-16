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

module FalcoViewBuild =
    open Falco.Markup
    open Falco.Markup.Elem
    open Falco.Markup.Attr

    let get () =
        html [] [
            body [ style "width: 800px; margin: 0 auto" ] [
                h1 [ style "text-align: center; color: red" ] [ Text.enc "Error" ]
                p [] [ Text.enc "Some long error text" ]
                ul [] [
                    for _ in 1..10 do
                        li [] [ Elem.span [] [ Text.enc "Test" ] ]
                ]
            ]
        ]

[<MemoryDiagnoser>]
type ViewEngineBuild() =

    // BenchmarkDotNet v0.14.0, Windows 10
    // AMD Ryzen 7 2700X, 1 CPU, 16 logical and 8 physical cores
    // .NET SDK 8.0.401
    //   [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2 DEBUG
    //   DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
    //
    //
    // | Method            | Mean     | Error   | StdDev  | Gen0   | Allocated |
    // |------------------ |---------:|--------:|--------:|-------:|----------:|
    // | BuildOxpeckerView | 578.0 ns | 9.00 ns | 8.42 ns | 0.7992 |   3.27 KB |
    // | BuildGiraffeView  | 789.8 ns | 1.90 ns | 1.68 ns | 0.6323 |   2.59 KB |
    // | BuildFalcoView    | 836.7 ns | 6.20 ns | 5.49 ns | 0.7763 |   3.17 KB |


    [<Benchmark>]
    member this.BuildOxpeckerView() = OxpeckerViewBuild.get()

    [<Benchmark>]
    member this.BuildGiraffeView() = GiraffeViewBuild.get()

    [<Benchmark>]
    member this.BuildFalcoView() = FalcoViewBuild.get()
