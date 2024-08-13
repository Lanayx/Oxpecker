namespace Oxpecker.ViewEngine

open System.Net
open System.Runtime.CompilerServices
open System.Text
open Tools

[<AutoOpen>]
module Builder =

    [<Struct>]
    type RawText = { Text: string }

    let raw text = { Text = text }

    [<Struct>]
    type HtmlAttribute = { Name: string; Value: string }

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
                sb
                    .Append(' ')
                    .Append(attr.Name)
                    .Append("=\"")
                    .Append(WebUtility.HtmlEncode(attr.Value))
                    .Append('"')
                |> ignore
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

    type FragmentNode() =
        let mutable children: CustomQueue<HtmlElement> = Unchecked.defaultof<_>
        member this.AddChild(element: HtmlElement) = children.Enqueue(element)
        member this.Children = children.AsEnumerable()
        interface HtmlContainer with
            member this.Render(sb: StringBuilder) =
                RenderHelpers.renderChildren sb children
            member this.AddChild(element: HtmlElement) = children.Enqueue(element)

    type RegularNode(tagName: string) =
        let mutable children: CustomQueue<HtmlElement> = Unchecked.defaultof<_>
        let mutable attributes: CustomQueue<HtmlAttribute> = Unchecked.defaultof<_>
        member this.Children = children.AsEnumerable()
        member this.Attributes = attributes.AsEnumerable()
        member this.TagName = tagName
        interface HtmlElement with
            member this.Render(sb: StringBuilder) =
                RenderHelpers.renderStartTag tagName sb attributes
                RenderHelpers.renderChildren sb children
                RenderHelpers.renderEndTag tagName sb
        interface HtmlTag with
            member this.AddAttribute(attribute: HtmlAttribute) = attributes.Enqueue(attribute)
        interface HtmlContainer with
            member this.AddChild(element: HtmlElement) = children.Enqueue(element)

    type VoidNode(tagName: string) =
        let mutable attributes: CustomQueue<HtmlAttribute> = Unchecked.defaultof<_>
        member this.Attributes = attributes.AsEnumerable()
        member this.TagName = tagName
        interface HtmlTag with
            member this.Render(sb: StringBuilder) =
                RenderHelpers.renderStartTag tagName sb attributes
            member this.AddAttribute(attribute: HtmlAttribute) = attributes.Enqueue(attribute)

    type RegularTextNode(text: string) =
        interface HtmlElement with
            member this.Render(sb: StringBuilder) =
                text |> WebUtility.HtmlEncode |> sb.Append |> ignore

    type RawTextNode(text: string) =
        interface HtmlElement with
            member this.Render(sb: StringBuilder) = text |> sb.Append |> ignore

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

        member inline _.For(values: 'T seq, [<InlineIfLambda>] body: 'T -> HtmlContainerFun) : HtmlContainerFun =
            fun builder ->
                for value in values do
                    body value builder

        member inline _.Yield(element: HtmlElement) : HtmlContainerFun = _.AddChild(element)

        member inline _.Yield(text: string) : HtmlContainerFun = _.AddChild(RegularTextNode text)

        member inline _.Yield(text: RawText) : HtmlContainerFun = _.AddChild(RawTextNode text.Text)

    type HtmlContainerExtensions =
        [<Extension>]
        static member inline Run(this: #HtmlContainer, [<InlineIfLambda>] runExpr: HtmlContainerFun) =
            runExpr this
            this
