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

    type HtmlElementType =
        | DoubleTag = 0uy
        | SingleTag = 1uy
        | TextNode = 2uy

    type HtmlElementFun = HtmlElement -> unit

    and HtmlElement(tagName: string, elemType: HtmlElementType) =
        let children = ResizeArray<HtmlElement>()
        let attributes = ResizeArray<HtmlAttribute>()

        new(tagName: string) = HtmlElement(tagName, HtmlElementType.DoubleTag)

        // global attributes
        member this.id with set value = this.attr("id", value) |> ignore
        member this.class' with set value = this.attr("class", value) |> ignore
        member this.style with set value = this.attr("style", value) |> ignore
        member this.lang with set value = this.attr("lang", value) |> ignore
        member this.dir with set value = this.attr("dir", value) |> ignore

        member this.Render(sb: StringBuilder): unit =
            let inline handleSingleTag() =
                sb.Append('<').Append(tagName) |> ignore
                for attribute in attributes do
                    sb.Append(' ').Append(attribute.Name).Append("=\"").Append(attribute.Value).Append('"') |> ignore
                sb.Append('>') |> ignore

            match elemType with
            | HtmlElementType.TextNode ->
                sb.Append(tagName) |> ignore
            | HtmlElementType.SingleTag ->
                handleSingleTag()
            | HtmlElementType.DoubleTag ->
                handleSingleTag()
                for child in children do
                    child.Render(sb)
                sb.Append("</").Append(tagName).Append('>') |> ignore
            | _ ->
                failwith "Unknown element type"

        member this.AddChild(element: HtmlElement) =
            children.Add(element)
            this

        member this.attr(name: string, value: string) =
            if not (isNull value) then
                attributes.Add({ Name = name; Value = value })
            this

        member this.Children = children
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

        member inline _.Yield(text: string) : HtmlElementFun =
            fun builder ->
                builder.AddChild(TextNode text) |> ignore

        member inline this.Run([<InlineIfLambda>]runExpr: HtmlElementFun) =
            runExpr this
            this

    and TextNode(text: string) = inherit HtmlElement(text, HtmlElementType.TextNode)