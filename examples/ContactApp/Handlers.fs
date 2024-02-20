module ContactApp.Handlers
open ContactApp.templates
open ContactApp.Models
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

let getNewContact: EndpointHandler =
    let newContact = {
        id = 0
        first = ""
        last = ""
        email = ""
        phone = ""
        errors = dict []
    }
    new'.html newContact |> htmlView

let postNewContact: EndpointHandler =
    fun ctx ->
        task {
            let! contact = ctx.BindForm<ContactDTO>()
            let validatedContact = contact.Validate()
            if validatedContact.errors.Count > 0 then
                return! ctx.WriteHtmlView(new'.html validatedContact)
            else
                validatedContact.ToDomain()
                |> ContactService.add
                |> ignore
                return ctx.Response.Redirect("/contacts")
        }

