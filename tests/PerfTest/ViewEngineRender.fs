namespace PerfTest

open System.Net
open System.Text
open BenchmarkDotNet.Attributes
open Oxpecker.ViewEngine.Tools

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

    let prerenderedHtml = prerender staticHtml

    /// Same document, but with the one paragraph that would change per request left as a hole
    let template =
        prerenderAround(fun content ->
            html() {
                body(style = "width: 800px; margin: 0 auto") {
                    h1(style = "text-align: center; color: red") { "Error" }
                    content
                    p() { raw "<h2>Raw HTML</h2>" }
                    ul() {
                        for _ in 1..10 do
                            li() { span() { "Hellö 𝓦orld!" } }
                    }
                }
            })

    let dynamicPart = p() { "Some long error text" }

    let renderTemplate () = template() { dynamicPart }

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

    // BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
    // AMD Ryzen AI 9 HX PRO 370 w/ Radeon 890M 2.00GHz, 1 CPU, 24 logical and 12 physical cores
    // .NET SDK 10.0.400
    //   [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 DEBUG
    //   DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
    //
    //
    // | Method                        | Mean      | Error     | StdDev    | Gen0   | Gen1   | Allocated |
    // |------------------------------ |----------:|----------:|----------:|-------:|-------:|----------:|
    // | RenderOxpeckerView            | 550.75 ns |  9.861 ns | 12.471 ns | 0.1612 |      - |   1.32 KB |
    // | RenderPrerenderedOxpeckerView |  69.19 ns |  2.367 ns |  6.904 ns | 0.1615 |      - |   1.32 KB |
    // | RenderTemplateOxpeckerView    | 116.56 ns | 10.984 ns | 32.042 ns | 0.1712 |      - |    1.4 KB |
    // | RenderGiraffeView             | 586.42 ns | 11.716 ns | 32.659 ns | 1.3647 | 0.0753 |  11.15 KB |
    // | RenderFalcoView               | 976.39 ns | 18.912 ns | 23.226 ns | 0.4730 | 0.0019 |   3.87 KB |

    [<Benchmark>]
    member this.RenderOxpeckerView() =
        OxpeckerViewRender.staticHtml |> Oxpecker.ViewEngine.Render.toHtmlDocString

    [<Benchmark>]
    member this.RenderPrerenderedOxpeckerView() =
        OxpeckerViewRender.prerenderedHtml |> Oxpecker.ViewEngine.Render.toHtmlDocString

    [<Benchmark>]
    member this.RenderTemplateOxpeckerView() =
        OxpeckerViewRender.renderTemplate()
        |> Oxpecker.ViewEngine.Render.toHtmlDocString

    [<Benchmark>]
    member this.RenderGiraffeView() =
        GiraffeViewRender.staticHtml
        |> Giraffe.ViewEngine.RenderView.AsString.htmlDocument

    [<Benchmark>]
    member this.RenderFalcoView() =
        FalcoViewRender.staticHtml |> Falco.Markup.XmlNodeRenderer.renderHtml

// let unencodedFortunes =
// """<!doctype html><html>
//   <head><title>Fortunes</title></head>
//   <body><table>
//   <tr><th>id</th><th>message</th></tr>
//   <tr><td>11</td><td><script>alert("This should not be displayed in a browser alert box.");</script></td></tr>
//   <tr><td>4</td><td>A bad random number generator: 1, 1, 1, 1, 1, 4.33e+67, 1, 1, 1</td></tr>
//   <tr><td>5</td><td>A computer program does what you tell it to do, not what you want it to do.</td></tr>
//   <tr><td>2</td><td>`@#$%^&*()_+=-,/|\}{[]  ~.</td></tr>
//   <tr><td>8</td><td>A list is only as strong as its weakest link. — Donald Knuth</td></tr>
//   <tr><td>0</td><td>Съешь ещё этих мягких французских булок, да выпей чаю.</td></tr>
//   <tr><td>3</td><td>"Hellö 𝓦orld!"</td></tr>
//   <tr><td>7</td><td>😀😄😵‍💫</td></tr>
//   <tr><td>10</td><td>联合国中文日</td></tr>
//   <tr><td>6</td><td>Emacs is a nice operating system, but I prefer UNIX. — Tom Christaensen</td></tr>
//   <tr><td>9</td><td>Feature: A bug with seniority.</td></tr>
//   <tr><td>1</td><td>fortune: No such file or directory</td></tr>
//   <tr><td>12</td><td>フレームワークのベンチマーク</td></tr>
//   </table></body></html>"""

// [<Benchmark>]
// member this.CustomHtmlEncode() =
//     let sb = StringBuilder()
//     CustomWebUtility.htmlEncode unencodedFortunes sb
//
// [<Benchmark>]
// member this.HtmlEncode() =
//     let sb = StringBuilder()
//     let s = WebUtility.HtmlEncode unencodedFortunes
//     sb.Append(s) |> ignore
