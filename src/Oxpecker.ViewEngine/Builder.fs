namespace Oxpecker.ViewEngine

[<AutoOpen>]
module Builder =

    open System.Text

    type HtmlElementFun = HtmlElement -> unit

    and HtmlElement(tagName: string) =
        let childElements = ResizeArray<HtmlElement>()
        let properties = ResizeArray<struct(string*string)>()

        // general attributes
        member this.id with set value =
            properties.Add("id", value)

        abstract member Render : unit -> StringBuilder
        default this.Render() =
            let sb = StringBuilder().Append('<').Append(tagName)
            for name, value in properties do
                sb.Append(' ').Append(name).Append("=\"").Append(value).Append("\"") |> ignore
            sb.Append('>') |> ignore
            for child in childElements do
                sb.Append(child.Render()) |> ignore
            sb.Append("</").Append(tagName).Append('>')

        member this.AddChild(element: HtmlElement) =
            childElements.Add(element)

        // builder methods
        member inline  _.Combine([<InlineIfLambda>]first: HtmlElementFun, [<InlineIfLambda>]second: HtmlElementFun) : HtmlElementFun =
            fun builder ->
                first builder
                second builder

        member inline _.Zero() : HtmlElementFun = ignore

        member inline _.Delay([<InlineIfLambda>]delay: unit -> HtmlElementFun) =
            fun (builder) -> (delay())(builder)
            // Note, not "f()()" - the F# compiler optimizer likes arguments to match lambdas in order to preserve
            // argument evaluation order, so for "(f())()" the optimizer reduces one lambda then another, while "f()()" doesn't

        member inline _.For(values: 'T seq, [<InlineIfLambda>]body: 'T -> HtmlElementFun) : HtmlElementFun =
            fun builder ->
                for value in values do
                    body value builder

        member inline _.Yield(element: HtmlElement) : HtmlElementFun =
            _.AddChild(element)

        member inline this.Run([<InlineIfLambda>]runExpr: HtmlElementFun) =
            runExpr this
            this