module ContactApp.Handlers
open System.Threading.Tasks
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
    fun ctx ->
        let newContact = {
            id = 0
            first = ""
            last = ""
            email = ""
            phone = ""
            errors = dict []
        }
        new'.html newContact |> ctx.WriteHtmlView

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
            if validatedContact.errors.Count = 0 then
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

let postDeleteContact id: EndpointHandler =
    fun ctx ->
        task {
            ContactService.delete id |> ignore
            ctx.Response.Redirect("/contacts")
            ctx.SetStatusCode(303)
        }

let validateEmail id: EndpointHandler =
    fun ctx ->
        match ctx.TryGetQueryStringValue("email") with
        | Some email ->
            let contact =
                if id = 0 then
                    { Id = 0; First = ""; Last = ""; Phone = ""; Email = email }
                else
                    let contact = ContactService.find id
                    { contact with Email = email  }
            if ContactService.validateEmail { contact with Email = email  } then
                Task.CompletedTask
            else
                ctx.WriteText "Invalid email"
        | None ->
                Task.CompletedTask


