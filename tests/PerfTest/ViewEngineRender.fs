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
                        li [] [ Elem.span [] [ Text.enc "Test" ] ]
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
                        li [] [ span [] [ str "Test" ] ]
                ]
            ]
        ]

[<MemoryDiagnoser>]
type ViewEngineRender() =

    // BenchmarkDotNet v0.14.0, Windows 10
    // AMD Ryzen 7 2700X, 1 CPU, 16 logical and 8 physical cores
    // .NET SDK 8.0.401
    //   [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2 DEBUG
    //   DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
    //
    //
    // | Method             | Mean       | Error   | StdDev  | Gen0   | Gen1   | Allocated |
    // |------------------- |-----------:|--------:|--------:|-------:|-------:|----------:|
    // | RenderOxpeckerView |   729.8 ns | 2.66 ns | 2.36 ns | 0.2213 |      - |     928 B |
    // | RenderGiraffeView  | 1,098.6 ns | 5.94 ns | 5.55 ns | 2.6302 | 0.0019 |   11000 B |
    // | RenderFalcoView    | 1,324.7 ns | 4.38 ns | 3.66 ns | 0.5798 |      - |    2432 B |



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
