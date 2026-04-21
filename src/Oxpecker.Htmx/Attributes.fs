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
        /// Controls how content will swap in (outerHTML, beforeend, afterend, …)
        member this.hxSwap
            with set (value: string | null) = this.attr("hx-swap", value) |> ignore
        /// Specifies the target element to be swapped
        [<StringSyntax("css")>]
        member this.hxTarget
            with set (value: string | null) = this.attr("hx-target", value) |> ignore
        /// Specifies the event that triggers the request
        member this.hxTrigger
            with set (value: string | null) = this.attr("hx-trigger", value) |> ignore
        /// Select content to swap in from a response
        [<StringSyntax("css")>]
        member this.hxSelect
            with set (value: string | null) = this.attr("hx-select", value) |> ignore
        /// Issues a PUT to the specified URL
        member this.hxPut
            with set (value: string | null) = this.attr("hx-put", value) |> ignore
        /// Issues a PATCH to the specified URL
        member this.hxPatch
            with set (value: string | null) = this.attr("hx-patch", value) |> ignore
        /// Issues a DELETE to the specified URL
        member this.hxDelete
            with set (value: string | null) = this.attr("hx-delete", value) |> ignore
        /// Add values to submit with the request (JSON format)
        member this.hxVals
            with set (value: string | null) = this.attr("hx-vals", value) |> ignore


[<AutoOpen>]
module AdditionalAttributes =
    type HtmlTag with
        /// Mark element to swap in from a response (out of band)
        member this.hxSwapOob
            with set (value: string | null) = this.attr("hx-swap-oob", value) |> ignore
        /// Push a URL into the browser location bar to create history
        member this.hxPushUrl
            with set (value: string | null) = this.attr("hx-push-url", value) |> ignore
        /// Include additional data in requests
        member this.hxInclude
            with set (value: string | null) = this.attr("hx-include", value) |> ignore
        /// Select content to swap in from a response, somewhere other than the target (out of band)
        [<StringSyntax("css")>]
        member this.hxSelectOob
            with set (value: string | null) = this.attr("hx-select-oob", value) |> ignore
        /// Add progressive enhancement for links and forms
        member this.hxBoost
            with set (value: bool) = this.attr("hx-boost", (if value then "true" else "false")) |> ignore
        /// Replace the URL in the browser location bar
        member this.hxReplaceUrl
            with set (value: string | null) = this.attr("hx-replace-url", value) |> ignore
        /// Shows a confirm() dialog before issuing a request
        member this.hxConfirm
            with set (value: string | null) = this.attr("hx-confirm", value) |> ignore
        /// Adds the disabled attribute to specified elements while a request is in flight
        member this.hxDisable
            with set (value: string | null) = this.attr("hx-disable", value) |> ignore
        /// Specifies elements to keep unchanged between requests
        member this.hxPreserve
            with set (value: bool) =
                if value then
                    this.attr("hx-preserve", "") |> ignore
        /// Adds to the headers that will be submitted with the request
        member this.hxHeaders
            with set (value: string | null) = this.attr("hx-headers", value) |> ignore
        /// The element to put the htmx-request class on during the request
        [<StringSyntax("css")>]
        member this.hxIndicator
            with set (value: string | null) = this.attr("hx-indicator", value) |> ignore
        /// Control how requests made by different elements are synchronized
        member this.hxSync
            with set (value: string | null) = this.attr("hx-sync", value) |> ignore
        /// Preload content before user triggers request
        member this.hxPreload
            with set (value: string | null) = this.attr("hx-preload", value) |> ignore
        /// Force elements to validate themselves before a request
        member this.hxValidate
            with set (value: bool) = this.attr("hx-validate", (if value then "true" else "false")) |> ignore
        /// Changes the request encoding type
        member this.hxEncoding
            with set (value: string | null) = this.attr("hx-encoding", value) |> ignore
        /// Specify URL to receive request (use with hxMethod)
        member this.hxAction
            with set (value: string | null) = this.attr("hx-action", value) |> ignore
        /// Specify HTTP method for request (use with hxAction)
        member this.hxMethod
            with set (value: string | null) = this.attr("hx-method", value) |> ignore
        /// Configure request behavior with JSON
        member this.hxConfig
            with set (value: string | null) = this.attr("hx-config", value) |> ignore
        /// Disable htmx processing for the given node and any children nodes
        member this.hxIgnore
            with set (value: bool) =
                if value then
                    this.attr("hx-ignore", "") |> ignore
        /// Show optimistic content during request
        member this.hxOptimistic
            with set (value: string | null) = this.attr("hx-optimistic", value) |> ignore


[<AutoOpen>]
module ModifierAttributes =
    open System.Runtime.CompilerServices

    type HtmlTagHtmx4Extensions =
        /// Set an attribute with the :inherited modifier for explicit inheritance in htmx 4
        /// e.g. hxInherited "hx-boost" "true" renders hx-boost:inherited="true"
        [<Extension>]
        static member hxInherited(this: #HtmlTag, attr: string, value: string) = this.attr(attr + ":inherited", value)
        /// Set an attribute with the :inherited:append modifier to append to an inherited value
        /// e.g. hxInheritedAppend "hx-include" ".extra" renders hx-include:inherited:append=".extra"
        [<Extension>]
        static member hxInheritedAppend(this: #HtmlTag, attr: string, value: string) =
            this.attr(attr + ":inherited:append", value)
        /// Set an attribute with the :merge modifier to merge with parent values
        /// e.g. hxMerge "hx-disable" "find button" renders hx-disable:merge="find button"
        [<Extension>]
        static member hxMerge(this: #HtmlTag, attr: string, value: string) = this.attr(attr + ":merge", value)
        /// Handle responses differently by status code
        /// e.g. hxStatus 422 "swap:innerHTML target:#errors" renders hx-status:422="swap:innerHTML target:#errors"
        [<Extension>]
        static member hxStatus(this: #HtmlTag, code: string, value: string) = this.attr("hx-status:" + code, value)
