namespace Oxpecker.ViewEngine

open System.Runtime.CompilerServices
open System.Text
open Tools

[<AutoOpen>]
module Builder =

    [<Struct>]
    type HtmlAttribute = { Name: string; Value: string | null }

    type HtmlElement =
        abstract member Render: StringBuilder -> unit

    type HtmlTag =
        inherit HtmlElement
        abstract member AddAttribute: HtmlAttribute -> unit

    type HtmlContainer =
        inherit HtmlElement
        abstract member AddChild: HtmlElement -> unit

    module internal RenderHelpers =
        let inline renderStartTag (tagName: string) (sb: StringBuilder) (attributes: CustomQueue<HtmlAttribute>) =
            sb.Append('<').Append(tagName) |> ignore
            let mutable next = attributes.Head
            while isNotNull next do
                let attr = next.Value
                if isNull attr.Value then
                    sb.Append(' ').Append(attr.Name) |> ignore
                else
                    sb.Append(' ').Append(attr.Name).Append("=\"") |> ignore
                    sb |> CustomWebUtility.htmlEncode attr.Value
                    sb.Append('"') |> ignore
                next <- next.Next
            sb.Append('>') |> ignore

        let inline renderChildren (sb: StringBuilder) (children: CustomQueue<HtmlElement>) =
            let mutable next = children.Head
            while isNotNull next do
                let child = next.Value
                child.Render(sb)
                next <- next.Next
        let inline renderEndTag (tagName: string) (sb: StringBuilder) =
            sb.Append("</").Append(tagName).Append('>') |> ignore

    /// Node with children only (no attributes)
    type FragmentNode() =
        let mutable children: CustomQueue<HtmlElement> = Unchecked.defaultof<_>
        member this.Children = children.AsEnumerable()
        member this.AddChild(element: HtmlElement) = children.Enqueue(element)
        member this.Render(sb: StringBuilder) =
            RenderHelpers.renderChildren sb children
        interface HtmlContainer with
            member this.Render sb = this.Render sb
            member this.AddChild element = this.AddChild element

    /// Node with both children and attributes
    type RegularNode(tagName: string) =
        let mutable children: CustomQueue<HtmlElement> = Unchecked.defaultof<_>
        let mutable attributes: CustomQueue<HtmlAttribute> = Unchecked.defaultof<_>
        member this.Children = children.AsEnumerable()
        member this.Attributes = attributes.AsEnumerable()
        member this.TagName = tagName
        member this.Render(sb: StringBuilder) =
            RenderHelpers.renderStartTag tagName sb attributes
            RenderHelpers.renderChildren sb children
            RenderHelpers.renderEndTag tagName sb
        member this.AddAttribute(attribute: HtmlAttribute) = attributes.Enqueue(attribute)
        member this.AddChild(element: HtmlElement) = children.Enqueue(element)
        interface HtmlElement with
            member this.Render sb = this.Render sb
        interface HtmlTag with
            member this.AddAttribute attribute = this.AddAttribute attribute
        interface HtmlContainer with
            member this.AddChild element = this.AddChild element

    /// Node with attributes only (no children)
    type VoidNode(tagName: string) =
        let mutable attributes: CustomQueue<HtmlAttribute> = Unchecked.defaultof<_>
        member this.Attributes = attributes.AsEnumerable()
        member this.Render(sb: StringBuilder) =
            RenderHelpers.renderStartTag tagName sb attributes
        member this.AddAttribute(attribute: HtmlAttribute) = attributes.Enqueue(attribute)
        member this.TagName = tagName
        interface HtmlTag with
            member this.Render sb = this.Render sb
            member this.AddAttribute attribute = this.AddAttribute attribute

    /// Text node that will be HTML-escaped
    type RegularTextNode(text: string | null) =
        member this.Render(sb: StringBuilder) = sb |> CustomWebUtility.htmlEncode text
        interface HtmlElement with
            member this.Render sb = this.Render sb

    /// Text node that will NOT be HTML-escaped
    type RawTextNode(text: string | null) =
        member this.Render(sb: StringBuilder) = text |> sb.Append |> ignore
        interface HtmlElement with
            member this.Render sb = this.Render sb

    /// Integer node that will NOT be HTML-escaped
    type IntNode(value: int) =
        member this.Render(sb: StringBuilder) = value |> sb.Append |> ignore
        interface HtmlElement with
            member this.Render sb = this.Render sb

    /// Create text node that will NOT be HTML-escaped
    let inline raw text = RawTextNode text

    type HtmlContainerFun = HtmlContainer -> unit

    // builder methods
    type HtmlContainer with
        member inline _.Combine
            ([<InlineIfLambda>] first: HtmlContainerFun, [<InlineIfLambda>] second: HtmlContainerFun)
            : HtmlContainerFun =
            fun builder ->
                first builder
                second builder

        member inline _.Zero() : HtmlContainerFun = ignore

        member inline _.Delay([<InlineIfLambda>] delay: unit -> HtmlContainerFun) : HtmlContainerFun = delay()

        member inline _.For(values: #seq<'T>, [<InlineIfLambda>] body: 'T -> HtmlContainerFun) : HtmlContainerFun =
            fun builder ->
                for value in values do
                    body value builder

        member inline _.Yield(element: #HtmlElement) : HtmlContainerFun = _.AddChild(element)

        member inline _.YieldFrom(elements: #seq<#HtmlElement>) : HtmlContainerFun =
            fun builder ->
                for element in elements do
                    builder.AddChild(element)

        member inline _.Yield(text: string | null) : HtmlContainerFun = _.AddChild(RegularTextNode text)

        member inline _.Yield(value: int) : HtmlContainerFun = _.AddChild(IntNode value)

    type HtmlContainerExtensions =
        [<Extension>]
        static member inline Run(this: #HtmlContainer, [<InlineIfLambda>] runExpr: HtmlContainerFun) =
            runExpr this
            this
