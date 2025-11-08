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
                        li() { span() { "Hellö 𝓦orld!" } }
                }
            }
        }

module FalcoViewRender =
    open Falco.Markup
    open Falco.Markup.Elem
    open Falco.Markup.Attr

    let staticHtml =
        html [] [
            body [ style "width: 800px; margin: 0 auto" ] [
                h1 [ style "text-align: center; color: red" ] [ Text.enc "Error" ]
                p [] [ Text.enc "Some long raw text" ]
                p [] [ Text.raw "<h2>Raw HTML</h2>" ]
                ul [] [
                    for _ in 1..10 do
                        li [] [ Elem.span [] [ Text.enc "Hellö 𝓦orld!" ] ]
                ]
            ]
        ]

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
                        li [] [ span [] [ str "Hellö 𝓦orld!" ] ]
                ]
            ]
        ]

[<MemoryDiagnoser>]
type ViewEngineRender() =

    // BenchmarkDotNet v0.15.5, Windows 11 (10.0.26200.6899)
    // AMD Ryzen 5 5600H with Radeon Graphics 3.30GHz, 1 CPU, 12 logical and 6 physical cores
    // .NET SDK 10.0.100-rc.2.25502.107
    //   [Host]     : .NET 10.0.0 (10.0.0-rc.2.25502.107, 10.0.25.50307), X64 RyuJIT x86-64-v3 DEBUG
    //   DefaultJob : .NET 10.0.0 (10.0.0-rc.2.25502.107, 10.0.25.50307), X64 RyuJIT x86-64-v3
    //
    //
    // | Method             | Mean       | Error    | StdDev   | Median     | Gen0   | Gen1   | Allocated |
    // |------------------- |-----------:|---------:|---------:|-----------:|-------:|-------:|----------:|
    // | RenderOxpeckerView |   917.4 ns |  9.49 ns |  7.92 ns |   918.2 ns | 0.1612 |      - |   1.32 KB |
    // | RenderGiraffeView  |   918.9 ns | 20.12 ns | 59.33 ns |   891.8 ns | 1.3647 | 0.0753 |  11.15 KB |
    // | RenderFalcoView    | 1,274.0 ns | 22.94 ns | 22.53 ns | 1,279.5 ns | 0.4730 | 0.0019 |   3.87 KB |



    [<Benchmark>]
    member this.RenderOxpeckerView() =
        OxpeckerViewRender.staticHtml |> Oxpecker.ViewEngine.Render.toHtmlDocString

    [<Benchmark>]
    member this.RenderGiraffeView() =
        GiraffeViewRender.staticHtml
        |> Giraffe.ViewEngine.RenderView.AsString.htmlDocument

    [<Benchmark>]
    member this.RenderFalcoView() =
        FalcoViewRender.staticHtml |> Falco.Markup.XmlNodeRenderer.renderHtml
