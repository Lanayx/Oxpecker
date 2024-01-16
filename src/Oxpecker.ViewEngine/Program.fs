module Oxpecker.ViewEngine

open System.Text

type HtmlElementFun = HtmlElement -> unit

and HtmlElement() =
    let elements = StringBuilder()

    member val id = "" with get, set

    abstract member Render : unit -> string
    default this.Render() = elements.ToString()

    member this.Add(element: HtmlElement) =
        elements.Append(element.Render()) |> ignore
    member this.Elements = elements

    // builder methods
    member _.Combine(first: HtmlElementFun, second: HtmlElementFun) : HtmlElementFun =
        fun(builder) ->
            first(builder)
            second(builder)

    member _.Zero() : HtmlElementFun = ignore

    member _.Delay(delay: unit -> HtmlElementFun) =
        fun (builder) -> (delay())(builder)
        // Note, not "f()()" - the F# compiler optimizer likes arguments to match lambdas in order to preserve
        // argument evaluation order, so for "(f())()" the optimizer reduces one lambda then another, while "f()()" doesn't

    member _.For(values: 'T seq, body: 'T -> HtmlElementFun) : HtmlElementFun =
        fun builder ->
            for value in values do
                body value builder

    member _.Yield(element: HtmlElement) : HtmlElementFun =
        fun builder ->
            builder.Add(element)
    member this.Run(runExpr: HtmlElementFun) =
        runExpr this
        this

type html() =
    inherit HtmlElement()
    override this.Render() = $"<html>{this.Elements}</html>"

type div() =
    inherit HtmlElement()
    override this.Render() = $"<div id={this.id}>{this.Elements}</div>"

type br() =
    inherit HtmlElement()
    override this.Render() = $"<br/>"