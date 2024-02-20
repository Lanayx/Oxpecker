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

let postEditContact id: EndpointHandler =
    fun ctx ->
        task {
            let! contact = ctx.BindForm<ContactDTO>()
            let validatedContact = contact.Validate()
            if validatedContact.errors.Count > 0 then
                return! ctx.WriteHtmlView(edit.html { validatedContact with id = id })
            else
                let domainContact = validatedContact.ToDomain()
                ContactService.update({domainContact with Id = id})
                return ctx.Response.Redirect($"/contacts/{id}")
        }

let viewContact id: EndpointHandler =
    fun ctx ->
        let contact = ContactService.find id
        show.html contact |> ctx.WriteHtmlView

let getEditContact id: EndpointHandler =
    fun ctx ->
        let contact = ContactService.find id |> ContactDTO.FromDomain
        edit.html contact |> ctx.WriteHtmlView
