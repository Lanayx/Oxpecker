namespace Oxpecker.Htmx

open System.Diagnostics.CodeAnalysis
open Oxpecker.ViewEngine

[<AutoOpen>]
module CoreAttributes =
    type HtmlTag with
        /// Issues a GET to the specified URL
        member this.hxGet
            with set (value: string | null) = this.attr("hx-get", value) |> ignore
        /// Issues a POST to the specified URL
        member this.hxPost
            with set (value: string | null) = this.attr("hx-post", value) |> ignore
        /// Handle events with inline scripts on elements
        member this.hxOn(event, [<StringSyntax("js")>] value) = this.attr("hx-on:" + event, value)
        /// Push a URL into the browser location bar to create history
        member this.hxPushUrl
            with set (value: string | null) = this.attr("hx-push-url", value) |> ignore
        /// Select content to swap in from a response
        [<StringSyntax("css")>]
        member this.hxSelect
            with set (value: string | null) = this.attr("hx-select", value) |> ignore
        /// Select content to swap in from a response, somewhere other than the target (out of band)
        [<StringSyntax("css")>]
        member this.hxSelectOob
            with set (value: string | null) = this.attr("hx-select-oob", value) |> ignore
        /// Controls how content will swap in (outerHTML, beforeend, afterend, …)
        member this.hxSwap
            with set (value: string | null) = this.attr("hx-swap", value) |> ignore
        /// Mark element to swap in from a response (out of band)
        member this.hxSwapOob
            with set (value: string | null) = this.attr("hx-swap-oob", value) |> ignore
        /// Specifies the target element to be swapped
        [<StringSyntax("css")>]
        member this.hxTarget
            with set (value: string | null) = this.attr("hx-target", value) |> ignore
        /// Specifies the event that triggers the request
        member this.hxTrigger
            with set (value: string | null) = this.attr("hx-trigger", value) |> ignore
        /// Add values to submit with the request (JSON format)
        member this.hxVals
            with set (value: string | null) = this.attr("hx-vals", value) |> ignore


[<AutoOpen>]
module AdditionalAttributes =
    type HtmlTag with
        /// Add progressive enhancement for links and forms
        member this.hxBoost
            with set (value: bool) = this.attr("hx-boost", (if value then "true" else "false")) |> ignore
        /// Shows a confirm() dialog before issuing a request
        member this.hxConfirm
            with set (value: string | null) = this.attr("hx-confirm", value) |> ignore
        /// Issues a DELETE to the specified URL
        member this.hxDelete
            with set (value: string | null) = this.attr("hx-delete", value) |> ignore
        /// Disables htmx processing for the given node and any children nodes
        member this.hxDisable
            with set (value: bool) =
                if value then
                    this.attr("hx-disable", "true") |> ignore
        /// Adds the disabled attribute to the specified elements while a request is in flight
        member this.hxDisabledElt
            with set (value: string | null) = this.attr("hx-disabled-elt", value) |> ignore
        /// Control and disable automatic attribute inheritance for child nodes
        member this.hxDisinherit
            with set (value: string | null) = this.attr("hx-disinherit", value) |> ignore
        /// Changes the request encoding type
        member this.hxEncoding
            with set (value: string | null) = this.attr("hx-encoding", value) |> ignore
        /// Extensions to use for this element
        member this.hxExt
            with set (value: string | null) = this.attr("hx-ext", value) |> ignore
        /// Adds to the headers that will be submitted with the request
        member this.hxHeaders
            with set (value: string | null) = this.attr("hx-headers", value) |> ignore
        /// Prevent sensitive data being saved to the history cache
        member this.hxHistory
            with set (value: bool) = this.attr("hx-history", (if value then "true" else "false")) |> ignore
        /// The element to snapshot and restore during history navigation
        member this.hxHistoryElt
            with set (value: bool) =
                if value then
                    this.attr("hx-history-elt", "") |> ignore
        /// Include additional data in requests
        member this.hxInclude
            with set (value: string | null) = this.attr("hx-include", value) |> ignore
        /// The element to put the htmx-request class on during the request
        [<StringSyntax("css")>]
        member this.hxIndicator
            with set (value: string | null) = this.attr("hx-indicator", value) |> ignore
        /// Filters the parameters that will be submitted with a request
        member this.hxParams
            with set (value: string | null) = this.attr("hx-params", value) |> ignore
        /// Issues a PATCH to the specified URL
        member this.hxPatch
            with set (value: string | null) = this.attr("hx-patch", value) |> ignore
        /// Specifies elements to keep unchanged between requests
        member this.hxPreserve
            with set (value: bool) =
                if value then
                    this.attr("hx-preserve", "") |> ignore
        /// Shows a prompt() before submitting a request
        member this.hxPrompt
            with set (value: string | null) = this.attr("hx-prompt", value) |> ignore
        /// Issues a PUT to the specified URL
        member this.hxPut
            with set (value: string | null) = this.attr("hx-put", value) |> ignore
        /// Replace the URL in the browser location bar
        member this.hxReplaceUrl
            with set (value: string | null) = this.attr("hx-replace-url", value) |> ignore
        /// Configures various aspects of the request
        member this.hxRequest
            with set (value: string | null) = this.attr("hx-request", value) |> ignore
        /// Control how requests made by different elements are synchronized
        member this.hxSync
            with set (value: string | null) = this.attr("hx-sync", value) |> ignore
        /// Force elements to validate themselves before a request
        member this.hxValidate
            with set (value: bool) = this.attr("hx-validate", (if value then "true" else "false")) |> ignore
