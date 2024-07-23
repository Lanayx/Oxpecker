namespace Oxpecker.ViewEngine

open System.Diagnostics.CodeAnalysis
open System.Net
open System.Text
open JetBrains.Annotations

module NodeType =
    [<Literal>]
    let NormalNode = 0uy
    [<Literal>]
    let VoidNode = 1uy
    [<Literal>]
    let RegularTextNode = 2uy
    [<Literal>]
    let RawTextNode = 3uy

[<AutoOpen>]
module Builder =

    open Tools

    [<Struct>]
    type RawText = { Text: string }

    let raw text = { Text = text }

    [<Struct>]
    type HtmlAttribute = { Name: string; Value: string }

    [<Struct>]
    type HtmlElementType = { NodeType: byte; Value: string }

    type HtmlElementFun = HtmlElement -> unit

    and HtmlElement(elementType: HtmlElementType) =
        let mutable children: CustomQueue<HtmlElement> = Unchecked.defaultof<_>
        let mutable attributes: CustomQueue<HtmlAttribute> = Unchecked.defaultof<_>

        new(tagName: string) =
            HtmlElement(
                {
                    NodeType = NodeType.NormalNode
                    Value = tagName
                }
            )

        // global attributes
        member this.id
            with set value = this.attr("id", value) |> ignore
        member this.class'
            with set value = this.attr("class", value) |> ignore
        [<LanguageInjection(InjectedLanguage.CSS, Prefix = ".x{", Suffix = ";}")>]
        member this.style
            with set value = this.attr("style", value) |> ignore
        member this.lang
            with set value = this.attr("lang", value) |> ignore
        member this.dir
            with set value = this.attr("dir", value) |> ignore
        member this.tabindex
            with set (value: int) = this.attr("tabindex", string value) |> ignore
        member this.title
            with set value = this.attr("title", value) |> ignore
        member this.accesskey
            with set (value: char) = this.attr("accesskey", string value) |> ignore
        member this.contenteditable
            with set (value: bool) = this.attr("contenteditable", (if value then "true" else "false")) |> ignore
        member this.draggable
            with set value = this.attr("draggable", value) |> ignore
        member this.enterkeyhint
            with set value = this.attr("enterkeyhint", value) |> ignore
        member this.hidden
            with set (value: bool) =
                if value then
                    this.attr("hidden", "") |> ignore
        member this.inert
            with set (value: bool) =
                if value then
                    this.attr("inert", "") |> ignore
        member this.inputmode
            with set value = this.attr("inputmode", value) |> ignore
        member this.popover
            with set (value: bool) =
                if value then
                    this.attr("popover", "") |> ignore
        member this.spellcheck
            with set (value: bool) = this.attr("spellcheck", (if value then "true" else "false")) |> ignore
        member this.translate
            with set (value: bool) = this.attr("translate", (if value then "yes" else "no")) |> ignore

        /// Add event handler to the element through the corresponding attribute
        member this.on(eventName: string, [<StringSyntax("js")>] eventHandler: string) =
            this.attr($"on{eventName}", eventHandler)
        /// Add data attribute to the element
        member this.data(name: string, value: string) = this.attr($"data-{name}", value)

        member this.Render(sb: StringBuilder) : unit =
            let inline renderStartTag (tagName: string) =
                sb.Append('<').Append(tagName) |> ignore
                let mutable next = attributes.Head
                while isNotNull next do
                    let attr = next.Value
                    sb
                        .Append(' ')
                        .Append(attr.Name)
                        .Append("=\"")
                        .Append(WebUtility.HtmlEncode(attr.Value))
                        .Append('"')
                    |> ignore
                    next <- next.Next
                sb.Append('>') |> ignore
            let inline renderChildren () =
                let mutable next = children.Head
                while isNotNull next do
                    let child = next.Value
                    child.Render(sb)
                    next <- next.Next
            let inline renderEndTag (tagName: string) =
                sb.Append("</").Append(tagName).Append('>') |> ignore

            match elementType.NodeType with
            | NodeType.RawTextNode -> elementType.Value |> sb.Append |> ignore
            | NodeType.RegularTextNode -> elementType.Value |> WebUtility.HtmlEncode |> sb.Append |> ignore
            | NodeType.VoidNode -> renderStartTag elementType.Value
            | NodeType.NormalNode ->
                if isNull elementType.Value then
                    renderChildren()
                else
                    renderStartTag elementType.Value
                    renderChildren()
                    renderEndTag elementType.Value
            | _ -> failwith "Invalid node type"


        member this.AddChild(element: HtmlElement) =
            children.Enqueue(element)
            this

        /// Add an attribute to the element
        member this.attr(name: string, value: string) =
            if isNotNull value then
                attributes.Enqueue({ Name = name; Value = value })
            this

        member this.Children = children
        member this.Attributes = attributes
        member this.ElementType = elementType

        // builder methods
        member inline _.Combine
            ([<InlineIfLambda>] first: HtmlElementFun, [<InlineIfLambda>] second: HtmlElementFun)
            : HtmlElementFun =
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
            fun builder ->
                builder.AddChild(
                    HtmlElement {
                        NodeType = NodeType.RegularTextNode
                        Value = text
                    }
                )
                |> ignore

        member inline _.Yield(text: RawText) : HtmlElementFun =
            fun builder ->
                builder.AddChild(
                    HtmlElement {
                        NodeType = NodeType.RawTextNode
                        Value = text.Text
                    }
                )
                |> ignore

        member inline this.Run([<InlineIfLambda>] runExpr: HtmlElementFun) =
            runExpr this
            this
