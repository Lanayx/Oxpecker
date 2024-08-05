namespace Oxpecker.ViewEngine

open System.Net
open System.Runtime.CompilerServices
open System.Text

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

        member this.AddAttribute(attribute: HtmlAttribute) = attributes.Enqueue(attribute)
        member this.AddChild(element: HtmlElement) = children.Enqueue(element)

        member this.Children = children
        member this.Attributes = attributes
        member this.ElementType = elementType

    // builder methods
    type HtmlElement with
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

        member inline _.Yield(element: HtmlElement) : HtmlElementFun = _.AddChild(element)

        member inline _.Yield(text: string) : HtmlElementFun =
            _.AddChild(
                HtmlElement {
                    NodeType = NodeType.RegularTextNode
                    Value = text
                }
            )

        member inline _.Yield(text: RawText) : HtmlElementFun =
            _.AddChild(
                HtmlElement {
                    NodeType = NodeType.RawTextNode
                    Value = text.Text
                }
            )

    type HtmlElementExtensions =
        [<Extension>]
        static member inline Run<'T when 'T :> HtmlElement>(this: 'T, [<InlineIfLambda>] runExpr: HtmlElementFun) =
            runExpr this
            this
