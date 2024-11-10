module Tools.Tests

open System.Net
open System.Text
open Oxpecker.ViewEngine.Tools
open Xunit
open FsUnitTyped

[<Fact>]
let ``Custom queue works well`` () =
    let mutable customQueue = CustomQueue()
    customQueue.Enqueue(1)
    customQueue.Enqueue(2)
    customQueue.Enqueue(3)
    let result = customQueue.AsEnumerable() |> Seq.toArray
    result |> shouldEqual [| 1; 2; 3 |]

[<Fact>]
let ``Empty custom queue works well`` () =
    let customQueue = CustomQueue()
    let result = customQueue.AsEnumerable() |> Seq.toArray
    result |> shouldEqual [||]

[<Fact>]
let ``Tools.HtmlEncode and WebUtility.HtmlEncode are exactly the same`` () =
    let unencodedFortunes =
        """<!doctype html><html>
          <head><title>Fortunes</title></head>
          <body><table>
          <tr><th>id</th><th>message</th></tr>
          <tr><td>11</td><td><script>alert("This should not be displayed in a browser alert box.");</script></td></tr>
          <tr><td>4</td><td>A bad random number generator: 1, 1, 1, 1, 1, 4.33e+67, 1, 1, 1</td></tr>
          <tr><td>5</td><td>A computer program does what you tell it to do, not what you want it to do.</td></tr>
          <tr><td>2</td><td>`@#$%^&*()_+=-,/|\}{[]  ~.</td></tr>
          <tr><td>8</td><td>A list is only as strong as its weakest link. — Donald Knuth</td></tr>
          <tr><td>0</td><td>Съешь ещё этих мягких французских булок, да выпей чаю.</td></tr>
          <tr><td>3</td><td>"Hellö 𝓦orld!"</td></tr>
          <tr><td>7</td><td>😀😄😵‍💫</td></tr>
          <tr><td>10</td><td>联合国中文日</td></tr>
          <tr><td>6</td><td>Emacs is a nice operating system, but I prefer UNIX. — Tom Christaensen</td></tr>
          <tr><td>9</td><td>Feature: A bug with seniority.</td></tr>
          <tr><td>1</td><td>fortune: No such file or directory</td></tr>
          <tr><td>12</td><td>フレームワークのベンチマーク</td></tr>
          </table></body></html>"""

    let encodedFortunes = WebUtility.HtmlEncode unencodedFortunes

    let sb = StringBuilder()
    HtmlEncode sb unencodedFortunes

    encodedFortunes |> shouldEqual(sb.ToString())
