namespace Oxpecker.Solid

open System.Runtime.CompilerServices
open Browser.Types

[<AutoOpen>]
module Tags =

    /// Fragment (or template) node, only renders children, not itself
    type __() =
        inherit FragmentNode()

    /// Set of html extensions that keep original type
    [<Extension>]
    type HtmlElementExtensions =

        /// Add event handler to the element through the corresponding attribute
        [<Extension>]
        static member on(this: #HtmlTag, eventName: string, eventHandler: Event -> unit) = this

    type HtmlTag with
        member this.id  with set (_: string) = ()
        member this.class' with set (_: string) = ()

    type html() =
        inherit RegularNode()
        member this.xmlns with set (_: string) = ()

    type head() = inherit RegularNode()
    type body() = inherit RegularNode()
    type div() = inherit RegularNode()
    type h1() = inherit RegularNode()
    type h2() = inherit RegularNode()
    type h3() = inherit RegularNode()

    type br() = inherit VoidNode()
