namespace Oxpecker.ViewEngine

[<AutoOpen>]
module Builder =

    open System.Text
    open Tools

    [<Struct>]
    type HtmlAttribute = { Name: string; Value: string }

    [<Struct>]
    type HtmlElementType =
        | DoubleTag of dt: string
        | SingleTag of st: string
        | TextNode of text: string

    type HtmlElementFun = HtmlElement -> unit

    and HtmlElement(elemType: HtmlElementType) =
        let mutable children: CustomQueue<HtmlElement> = CustomQueue(null, null)
        let mutable attributes: CustomQueue<HtmlAttribute> = CustomQueue(null, null)

        new(tagName: string) = HtmlElement(HtmlElementType.DoubleTag(tagName))

        // global attributes
        member this.id
            with set value = this.attr("id", value) |> ignore
        member this.class'
            with set value = this.attr("class", value) |> ignore
        member this.style
            with set value = this.attr("style", value) |> ignore
        member this.lang
            with set value = this.attr("lang", value) |> ignore
        member this.dir
            with set value = this.attr("dir", value) |> ignore

        member this.Render(sb: StringBuilder) : unit =
            let inline handleSingleTag (tagName: string) =
                sb.Append('<').Append(tagName) |> ignore
                while isNotNull attributes.Head do
                    let attr = attributes.Dequeue()
                    sb.Append(' ').Append(attr.Name).Append("=\"").Append(attr.Value).Append('"')
                    |> ignore
                sb.Append('>') |> ignore

            match elemType with
            | HtmlElementType.TextNode content -> sb.Append(content) |> ignore
            | HtmlElementType.SingleTag tagName -> handleSingleTag(tagName)
            | HtmlElementType.DoubleTag tagName ->
                handleSingleTag(tagName)
                while isNotNull children.Head do
                    let child = children.Dequeue()
                    child.Render(sb)
                sb.Append("</").Append(tagName).Append('>') |> ignore


        member this.AddChild(element: HtmlElement) =
            children.Enqueue(element)
            this

        member this.attr(name: string, value: string) =
            if isNotNull value then
                attributes.Enqueue({ Name = name; Value = value })
            this

        member this.Children = children
        member this.Attributes = attributes

        // builder methods
        member inline _.Combine
            (
                [<InlineIfLambda>] first: HtmlElementFun,
                [<InlineIfLambda>] second: HtmlElementFun
            ) : HtmlElementFun =
            fun builder ->
                first builder
                second builder

        member inline _.Zero() : HtmlElementFun = ignore

        member inline _.Delay([<InlineIfLambda>] delay: unit -> HtmlElementFun) : HtmlElementFun = delay()

        member inline _.For(values: 'T seq, [<InlineIfLambda>] body: 'T -> HtmlElementFun) : HtmlElementFun =
            fun builder ->
                for value in values do
                    body value builder

        member inline _.Yield(element: HtmlElement) : HtmlElementFun =
            fun builder -> builder.AddChild(element) |> ignore

        member inline _.Yield(text: string) : HtmlElementFun =
            fun builder -> builder.AddChild(TextNode text) |> ignore

        member inline this.Run([<InlineIfLambda>] runExpr: HtmlElementFun) =
            runExpr this
            this

    and TextNode(text: string) =
        inherit HtmlElement(HtmlElementType.TextNode text)
