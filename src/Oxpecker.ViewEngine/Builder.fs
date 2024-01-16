namespace Oxpecker.ViewEngine

[<AutoOpen>]
module Builder =

    open System.Text

    [<Struct>]
    type HtmlAttribute =
        {
            Name: string
            Value: string
        }

    type HtmlElementFun = HtmlElement -> unit

    and HtmlElement(tagName: string, singleTag: bool) =
        let childElements = ResizeArray<HtmlElement>()
        let attributes = ResizeArray<HtmlAttribute>()

        new(tagName: string) = HtmlElement(tagName, false)

        // general attributes
        member this.id with set value = this.AddAttribute("id", value) |> ignore
        member this.class' with set value = this.AddAttribute("class", value) |> ignore
        member this.style with set value = this.AddAttribute("style", value) |> ignore
        member this.lang with set value = this.AddAttribute("lang", value) |> ignore
        member this.dir with set value = this.AddAttribute("dir", value) |> ignore

        abstract member Render : unit -> StringBuilder
        default this.Render() =
            let sb = StringBuilder().Append('<').Append(tagName)
            for attribute in attributes do
                sb.Append(' ').Append(attribute.Name).Append("=\"").Append(attribute.Value).Append("\"") |> ignore
            sb.Append('>') |> ignore
            if not singleTag then
                for child in childElements do
                    sb.Append(child.Render()) |> ignore
                sb.Append("</").Append(tagName).Append('>')
            else
                sb

        member this.AddChild(element: HtmlElement) =
            childElements.Add(element)
            this

        member this.AddAttribute(name: string, value: string) =
            if not (isNull value) then
                attributes.Add({ Name = name; Value = value })
            this

        member this.Children = childElements
        member this.Attributes = attributes

        // builder methods
        member inline  _.Combine([<InlineIfLambda>]first: HtmlElementFun, [<InlineIfLambda>]second: HtmlElementFun) : HtmlElementFun =
            fun builder ->
                first builder
                second builder

        member inline _.Zero() : HtmlElementFun = ignore

        member inline _.Delay([<InlineIfLambda>]delay: unit -> HtmlElementFun) : HtmlElementFun =
            delay()

        member inline _.For(values: 'T seq, [<InlineIfLambda>]body: 'T -> HtmlElementFun) : HtmlElementFun =
            fun builder ->
                for value in values do
                    body value builder

        member inline _.Yield(element: HtmlElement) : HtmlElementFun =
            fun builder ->
                builder.AddChild(element) |> ignore

        member inline this.Run([<InlineIfLambda>]runExpr: HtmlElementFun) =
            runExpr this
            this