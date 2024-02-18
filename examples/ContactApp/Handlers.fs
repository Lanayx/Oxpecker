module ContactApp.Handlers
open ContactApp.templates
open Oxpecker


let getContacts: EndpointHandler =
    fun ctx ->
        match ctx.TryGetQueryStringValue "q" with
        | Some search ->
            let result = ContactService.searchContact search
            index.html search result |> ctx.WriteHtmlView
        | None ->
            let result = ContactService.all()
            index.html "" result |> ctx.WriteHtmlView
