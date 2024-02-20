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
    // .NET SDK 8.0.200
    //   [Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2 DEBUG
    //   DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2
    //
    //
    // | Method             | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
    // |------------------- |---------:|----------:|----------:|-------:|-------:|----------:|
    // | RenderOxpeckerView | 1.904 us | 0.0360 us | 0.0353 us | 0.4807 |      - |   1.97 KB |
    // | RenderGiraffeView  | 1.275 us | 0.0246 us | 0.0242 us | 2.5234 | 0.0019 |  10.32 KB |


    [<Benchmark>]
    member this.RenderOxpeckerView() =
        OxpeckerViewRender.staticHtml |> Oxpecker.ViewEngine.Render.toHtmlDocBytes

    [<Benchmark>]
    member this.RenderGiraffeView() =
        GiraffeViewRender.staticHtml
        |> Giraffe.ViewEngine.RenderView.AsBytes.htmlDocument
