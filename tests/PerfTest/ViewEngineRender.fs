namespace PerfTest

open BenchmarkDotNet.Attributes

module OxpeckerViewRender =
    open Oxpecker.ViewEngine

    let staticHtml =
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

module GiraffeViewRender =
    open Giraffe.ViewEngine

    let staticHtml =
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
type ViewEngineRender() =

    // BenchmarkDotNet v0.13.12, Windows 10
    // AMD Ryzen 7 2700X, 1 CPU, 16 logical and 8 physical cores
    // .NET SDK 8.0.101
    //   [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2 DEBUG
    //   DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJI

    // | Method             | Mean       | Error    | StdDev   | Gen0   | Allocated |
    // |------------------- |-----------:|---------:|---------:|-------:|----------:|
    // | RenderOxpeckerView |   140.4 ns |  2.67 ns |  2.62 ns | 0.0629 |     264 B |
    // | RenderGiraffeView  | 1,224.8 ns | 21.66 ns | 19.20 ns | 2.5234 |   10552 B |


    [<Benchmark>]
    member this.RenderOxpeckerView() =
        OxpeckerViewRender.staticHtml |> Oxpecker.ViewEngine.Render.toHtmlDocBytes

    [<Benchmark>]
    member this.RenderGiraffeView() =
        GiraffeViewRender.staticHtml
        |> Giraffe.ViewEngine.RenderView.AsBytes.htmlDocument
