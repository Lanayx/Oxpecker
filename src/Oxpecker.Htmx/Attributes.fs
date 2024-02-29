namespace Oxpecker.Htmx

open System.Diagnostics.CodeAnalysis
open Oxpecker.ViewEngine

[<AutoOpen>]
module CoreAttributes =
    type HtmlElement with
        /// issues a GET to the specified URL
        member this.hxGet
            with set value = this.attr("hx-get", value) |> ignore
        /// issues a POST to the specified URL
        member this.hxPost
            with set value = this.attr("hx-post", value) |> ignore
        /// handle events with inline scripts on elements
        member this.hxOn(event, [<StringSyntax("js")>] value) = this.attr("hx-on:" + event, value)
        /// push a URL into the browser location bar to create history
        member this.hxPushUrl
            with set value = this.attr("hx-push-url", value) |> ignore
        /// select content to swap in from a response
        [<StringSyntax("css")>]
        member this.hxSelect
            with set value = this.attr("hx-select", value) |> ignore
        /// select content to swap in from a response, somewhere other than the target (out of band)
        [<StringSyntax("css")>]
        member this.hxSelectOob
            with set value = this.attr("hx-select-oob", value) |> ignore
        /// controls how content will swap in (outerHTML, beforeend, afterend, …)
        member this.hxSwap
            with set value = this.attr("hx-swap", value) |> ignore
        /// mark element to swap in from a response (out of band)
        member this.hxSwapOob
            with set value = this.attr("hx-swap-oob", value) |> ignore
        /// specifies the target element to be swapped
        [<StringSyntax("css")>]
        member this.hxTarget
            with set value = this.attr("hx-target", value) |> ignore
        /// specifies the event that triggers the request
        member this.hxTrigger
            with set value = this.attr("hx-trigger", value) |> ignore
        /// add values to submit with the request (JSON format)
        member this.hxVals
            with set value = this.attr("hx-vals", value) |> ignore


[<AutoOpen>]
module AdditionalAttributes =
    type HtmlElement with
        /// add progressive enhancement for links and forms
        member this.hxBoost
            with set value = this.attr("hx-boost", (if value then "true" else "false")) |> ignore
        /// shows a confirm() dialog before issuing a request
        member this.hxConfirm
            with set value = this.attr("hx-confirm", value) |> ignore
        /// issues a DELETE to the specified URL
        member this.hxDelete
            with set value = this.attr("hx-delete", value) |> ignore
        /// disables htmx processing for the given node and any children nodes
        member this.hxDisable
            with set value =
                if value then
                    this.attr("hx-disable", "true") |> ignore
        /// adds the disabled attribute to the specified elements while a request is in flight
        member this.hxDisabledElt
            with set value = this.attr("hx-disabled-elt", value) |> ignore
        /// control and disable automatic attribute inheritance for child nodes
        member this.hxDisinherit
            with set value = this.attr("hx-disinherit", value) |> ignore
        /// changes the request encoding type
        member this.hxEncoding
            with set value = this.attr("hx-encoding", value) |> ignore
        /// extensions to use for this element
        member this.hxExt
            with set value = this.attr("hx-ext", value) |> ignore
        /// adds to the headers that will be submitted with the request
        member this.hxHeaders
            with set value = this.attr("hx-headers", value) |> ignore
        /// prevent sensitive data being saved to the history cache
        member this.hxHistory
            with set value = this.attr("hx-history", (if value then "true" else "false")) |> ignore
        /// the element to snapshot and restore during history navigation
        member this.hxHistoryElt
            with set value =
                if value then
                    this.attr("hx-history-elt", "") |> ignore
        /// include additional data in requests
        member this.hxInclude
            with set value = this.attr("hx-include", value) |> ignore
        /// the element to put the htmx-request class on during the request
        [<StringSyntax("css")>]
        member this.hxIndicator
            with set value = this.attr("hx-indicator", value) |> ignore
        /// filters the parameters that will be submitted with a request
        member this.hxParams
            with set value = this.attr("hx-params", value) |> ignore
        /// issues a PATCH to the specified URL
        member this.hxPatch
            with set value = this.attr("hx-patch", value) |> ignore
        /// specifies elements to keep unchanged between requests
        member this.hxPreserve
            with set value =
                if value then
                    this.attr("hx-preserve", "") |> ignore
        /// shows a prompt() before submitting a request
        member this.hxPrompt
            with set value = this.attr("hx-prompt", value) |> ignore
        /// issues a PUT to the specified URL
        member this.hxPut
            with set value = this.attr("hx-put", value) |> ignore
        /// replace the URL in the browser location bar
        member this.hxReplaceUrl
            with set value = this.attr("hx-replace-url", value) |> ignore
        /// configures various aspects of the request
        member this.hxRequest
            with set value = this.attr("hx-request", value) |> ignore
        /// control how requests made by different elements are synchronized
        member this.hxSync
            with set value = this.attr("hx-sync", value) |> ignore
        /// force elements to validate themselves before a request
        member this.hxValidate
            with set value = this.attr("hx-validate", (if value then "true" else "false")) |> ignore
