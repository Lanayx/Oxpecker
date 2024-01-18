namespace PerfTest

open System.IO
open BenchmarkDotNet.Attributes
open Microsoft.AspNetCore.Http

module OxpeckerView =
    open Oxpecker.ViewEngine

    let get() =
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

module GiraffeView =
    open Giraffe.ViewEngine

    let get() =
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
type ViewEngine() =

// BenchmarkDotNet v0.13.12, Windows 10
// AMD Ryzen 7 2700X, 1 CPU, 16 logical and 8 physical cores
// .NET SDK 8.0.101
//   [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2 DEBUG
//   DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJI

// | Method             | Mean     | Error     | StdDev    | Gen0   | Allocated |
// |------------------- |---------:|----------:|----------:|-------:|----------:|
// | RenderOxpeckerView | 3.763 us | 0.0481 us | 0.0427 us | 3.1967 |  13.07 KB |
// | RenderGiraffeView  | 3.510 us | 0.0411 us | 0.0364 us | 3.9940 |  16.35 KB |


    [<Benchmark>]
    member this.RenderOxpeckerView () =
        let ctx = DefaultHttpContext()
        ctx.Response.Body <- new MemoryStream()
        OxpeckerView.get() |> Oxpecker.ViewEngine.Render.toResponseStream ctx

    [<Benchmark>]
    member this.RenderGiraffeView () =
        let ctx = DefaultHttpContext()
        ctx.Response.Body <- new MemoryStream()
        let bytes = GiraffeView.get() |> Giraffe.ViewEngine.RenderView.AsBytes.htmlDocument
        ctx.Response.ContentType <- "text/html; charset=utf-8"
        ctx.Response.ContentLength <- bytes.LongLength
        ctx.Response.Body.WriteAsync(bytes).AsTask()