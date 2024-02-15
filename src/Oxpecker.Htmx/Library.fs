namespace Oxpecker.Htmx

open Oxpecker.ViewEngine

[<AutoOpen>]
module CoreAttributes =
    type HtmlElement with
        member this.hxGet with set value =
            this.attr("hx-get", value) |> ignore
        member this.hxPost with set value =
            this.attr("hx-post", value) |> ignore
        member this.hxOn(event, value) =
            this.attr("hx-on:"+event, value)
        member this.hxPushUrl with set value =
            this.attr("hx-push-url", value) |> ignore
        member this.hxSelect with set value =
            this.attr("hx-select", value) |> ignore
        member this.hxSelectOob with set value =
            this.attr("hx-select-oob", value) |> ignore
        member this.hxSwap with set value =
            this.attr("hx-swap", value) |> ignore
        member this.hxSwapOob with set value =
            this.attr("hx-swap-oob", value) |> ignore
        member this.hxTarget with set value =
            this.attr("hx-target", value) |> ignore
        member this.hxTrigger with set value =
            this.attr("hx-trigger", value) |> ignore
        member this.hxVals with set value =
            this.attr("hx-vals", value) |> ignore


[<AutoOpen>]
module AdditionalAttributes =
    type HtmlElement with
        member this.hxBoost with set value =
            this.attr("hx-boost", if value then "true" else "false") |> ignore
        member this.hxConfirm with set value =
            this.attr("hx-confirm", value) |> ignore
        member this.hxDelete with set value =
            this.attr("hx-delete", value) |> ignore
        member this.hxDisable with set value =
            if value then
                this.attr("hx-disable", "true") |> ignore
        member this.hxDisabledElt with set value =
            this.attr("hx-disabled-elt", value) |> ignore
        member this.hxDisinherit with set value =
            this.attr("hx-disinherit", value) |> ignore
        member this.hxExt with set value =
            this.attr("hx-ext", value) |> ignore
        member this.hxHeaders with set value =
            this.attr("hx-headers", value) |> ignore
        member this.hxHistory with set value =
            this.attr("hx-history", if value then "true" else "false") |> ignore
        member this.hxHistoryElt with set value =
            if value then
                this.attr("hx-history-elt", "") |> ignore
        member this.hxInclude with set value =
            this.attr("hx-include", value) |> ignore
        member this.hxIndicator with set value =
            this.attr("hx-indicator", value) |> ignore
        member this.hxParams with set value =
            this.attr("hx-params", value) |> ignore
        member this.hxPatch with set value =
            this.attr("hx-patch", value) |> ignore
        member this.hxPreserve with set value =
            if value then
                this.attr("hx-preserve", "") |> ignore
        member this.hxPrompt with set value =
            this.attr("hx-prompt", value) |> ignore
        member this.hxPut with set value =
            this.attr("hx-put", value) |> ignore
        member this.hxReplaceUrl with set value =
            this.attr("hx-replace-url", value) |> ignore
        member this.hxRequest with set value =
            this.attr("hx-request", value) |> ignore
        member this.hxSync with set value =
            this.attr("hx-sync", value) |> ignore
        member this.hxValidate with set value =
            this.attr("hx-validate", if value then "true" else "false") |> ignore

module Test =

    let test () =

        let z =
            div(hxGet = "/", hxDisable = true, hxHistoryElt = true).hxOn("clicked", "alert!")
                {
                "sdf"
            }
        z
