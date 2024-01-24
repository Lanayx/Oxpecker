namespace PerfTest

open BenchmarkDotNet.Attributes

module OxpeckerViewRender =
    open Oxpecker.ViewEngine

    let staticHtml =
        html() {
            body(style = "width: 800px; margin: 0 auto") {
                h1(style = "text-align: center; color: red") { "Error" }
                p() { "Some long error text" }
                p() { raw "<h2>Raw HTML</h2>" }
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
                p [] [ str "Some long raw text" ]
                p [] [ rawText "<h2>Raw HTML</h2>" ]
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

    // | Method             | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
    // |------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
    // | RenderOxpeckerView |   141.9 ns |  0.98 ns |  0.87 ns | 0.0763 |      - |     320 B |
    // | RenderGiraffeView  | 1,217.4 ns | 14.39 ns | 12.76 ns | 2.5234 | 0.0019 |   10568 B |



    [<Benchmark>]
    member this.RenderOxpeckerView() =
        OxpeckerViewRender.staticHtml |> Oxpecker.ViewEngine.Render.toHtmlDocBytes

    [<Benchmark>]
    member this.RenderGiraffeView() =
        GiraffeViewRender.staticHtml
        |> Giraffe.ViewEngine.RenderView.AsBytes.htmlDocument
