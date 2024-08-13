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
                    for _ in 1..10 do
                        li() { span() { "Test" } }
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
                    for _ in 1..10 do
                        li [] [ span [] [ str "Test" ] ]
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
    // | Method             | Mean       | Error   | StdDev  | Gen0   | Gen1   | Allocated |
    // |------------------- |-----------:|--------:|--------:|-------:|-------:|----------:|
    // | RenderOxpeckerView |   775.7 ns | 7.86 ns | 6.97 ns | 0.1144 |      - |     480 B |
    // | RenderGiraffeView  | 1,159.4 ns | 3.27 ns | 2.73 ns | 2.5234 | 0.0019 |   10552 B |



    [<Benchmark>]
    member this.RenderOxpeckerView() =
        OxpeckerViewRender.staticHtml |> Oxpecker.ViewEngine.Render.toHtmlDocBytes

    [<Benchmark>]
    member this.RenderGiraffeView() =
        GiraffeViewRender.staticHtml
        |> Giraffe.ViewEngine.RenderView.AsBytes.htmlDocument
