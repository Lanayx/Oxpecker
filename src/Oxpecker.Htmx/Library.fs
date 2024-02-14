namespace Oxpecker.Htmx

open System.Runtime.CompilerServices
open Oxpecker.ViewEngine

module Extension =

    [<Extension>]
    type CoreHtmxExtensions =
        [<Extension>]
        static member hxBoost(el: HtmlElement, value: bool) =
            el.attr("hx-boost", if value then "true" else "false")
        [<Extension>]
        static member hxGet(el: HtmlElement, value) =
            el.attr("hx-get", value)
        [<Extension>]
        static member hxPost(el: HtmlElement, value) =
            el.attr("hx-post", value)
        [<Extension>]
        static member hxOn(el: HtmlElement, event, value) =
            el.attr("hx-on:"+event, value)
        [<Extension>]
        static member hxPushUrl(el: HtmlElement, value) =
            el.attr("hx-push-url", value)
        [<Extension>]
        static member hxSelect(el: HtmlElement, value) =
            el.attr("hx-select", value)
        [<Extension>]
        static member hxSelectOob(el: HtmlElement, value) =
            el.attr("hx-select-oob", value)
        [<Extension>]
        static member hxSwap(el: HtmlElement, value) =
            el.attr("hx-swap", value)
        [<Extension>]
        static member hxSwapOob(el: HtmlElement, value) =
            el.attr("hx-swap-oob", value)
        [<Extension>]
        static member hxTarget(el: HtmlElement, value) =
            el.attr("hx-target", value)
        [<Extension>]
        static member hxTrigger(el: HtmlElement, value) =
            el.attr("hx-trigger", value)
        [<Extension>]
        static member hxVals(el: HtmlElement, value) =
            el.attr("hx-vals", value)



module Test =

    open Extension

    let test () =

        let z =
            div().hxGet("").hxOn("clicked", "alert!")
                .hxTrigger("click[ctrlKey&&shiftKey]"){
                "sdf"
            }
        z
