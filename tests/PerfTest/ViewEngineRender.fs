namespace PerfTest

open System.IO
open BenchmarkDotNet.Attributes
open Microsoft.AspNetCore.Http

module OxpeckerViewRender =
    open Oxpecker.ViewEngine

    let staticHtml =
        html(){
            body(style = "width: 800px; margin: 0 auto") {
                h1(style="text-align: center; color: red") { "Error" }
                p() { "Some long error text" }
                ul() {
                    for i in 1..10 do
                        li() {
                            span() { $"Test %d{i}" }
                        }
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
                        li [] [
                            span [] [ str $"Test %d{i}" ]
                        ]
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

// | Method             | Mean     | Error     | StdDev    | Gen0   | Allocated |
// |------------------- |---------:|----------:|----------:|-------:|----------:|
// | RenderOxpeckerView | 1.484 us | 0.0231 us | 0.0205 us | 1.1292 |   4.62 KB |
// | RenderGiraffeView  | 1.861 us | 0.0344 us | 0.0305 us | 2.9659 |  12.13 KB |


    [<Benchmark>]
    member this.RenderOxpeckerView () =
        let ctx = DefaultHttpContext()
        ctx.Response.Body <- new MemoryStream()
        OxpeckerViewRender.staticHtml |> Oxpecker.ViewEngine.Render.toResponseStream ctx

    [<Benchmark>]
    member this.RenderGiraffeView () =
        let ctx = DefaultHttpContext()
        ctx.Response.Body <- new MemoryStream()
        let bytes = GiraffeViewRender.staticHtml |> Giraffe.ViewEngine.RenderView.AsBytes.htmlDocument
        ctx.Response.ContentType <- "text/html; charset=utf-8"
        ctx.Response.ContentLength <- bytes.LongLength
        ctx.Response.Body.WriteAsync(bytes).AsTask()