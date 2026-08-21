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

    /// Node with a prerendered prefix and suffix around its children
    type PrerenderedNode(prefix: string, suffix: string) =
        let mutable children: CustomQueue<HtmlElement> = Unchecked.defaultof<_>
        member this.Children = children.AsEnumerable()
        member this.AddChild(element: HtmlElement) = children.Enqueue(element)
        member this.Render(sb: StringBuilder) =
            sb.Append(prefix) |> ignore
            RenderHelpers.renderChildren sb children
            sb.Append(suffix) |> ignore
        interface HtmlContainer with
            member this.Render sb = this.Render sb
            member this.AddChild element = this.AddChild element

    /// Placeholder that records where the dynamic part of a template begins
    type internal HoleMarker() =
        let mutable position = -1
        let mutable count = 0
        member this.Position = position
        member this.Count = count
        member this.Render(sb: StringBuilder) =
            position <- sb.Length
            count <- count + 1
        interface HtmlElement with
            member this.Render sb = this.Render sb

    /// Create text node that will NOT be HTML-escaped
    let inline raw text = RawTextNode text

    /// <summary>
    /// Renders an element together with all its children into a static HTML snapshot.
    /// Use it to render static parts of a view once, instead of re-rendering them on every request.
    /// </summary>
    /// <remarks>
    /// The snapshot is taken eagerly, at the moment of the call. Children or attributes added
    /// to the original element afterwards will not be reflected in the returned node.
    /// </remarks>
    let prerender (view: #HtmlElement) =
        let sb = StringBuilderPool.Get()
        try
            view.Render sb
            RawTextNode(sb.ToString())
        finally
            StringBuilderPool.Return(sb)

    /// <summary>
    /// Renders the static part of a template with a hole in it once, and returns a factory
    /// creating nodes that fill the hole. Use it for layouts, where only a small part of the
    /// markup changes between renders.
    /// </summary>
    /// <param name="template">
    /// Function that receives the hole and places it inside the markup. It has to use the hole exactly once.
    /// </param>
    /// <remarks>
    /// The static part is rendered eagerly, at the moment of the call, exactly like <c>prerender</c> does.
    /// </remarks>
    let prerenderAround (template: HtmlElement -> #HtmlElement) =
        let hole = HoleMarker()
        let view = template hole
        let sb = StringBuilderPool.Get()
        let prefix, suffix =
            try
                view.Render sb
                if hole.Count <> 1 then
                    invalidArg (nameof template) "Template has to use the provided hole exactly once"
                sb.ToString(0, hole.Position), sb.ToString(hole.Position, sb.Length - hole.Position)
            finally
                StringBuilderPool.Return(sb)
        fun () -> PrerenderedNode(prefix, suffix)

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
