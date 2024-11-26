module ContactApp.Handlers
open System.Threading.Tasks
open ContactApp.templates
open ContactApp.Models
open ContactApp.Tools
open Microsoft.AspNetCore.Http
open Oxpecker
open Oxpecker.Htmx
open Oxpecker.ModelValidation

let mutable archiver = Archiver(ResizeArray())

let getContacts: EndpointHandler =
    fun ctx ->
        let page = ctx.TryGetQueryValue "page" |> Option.map int |> Option.defaultValue 1
        match ctx.TryGetQueryValue "q" with
        | Some search ->
            let result =
                ContactService.searchContact search
                |> Seq.toArray
            match ctx.TryGetHeaderValue HxRequestHeader.Trigger with
            | Some "search" ->
                ctx.WriteHtmlView (index.rows page result)
            | _ ->
                ctx |> writeHtml (index.html search page result archiver)
        | None ->
            let result =
                ContactService.all page
                |> Seq.toArray
            ctx |> writeHtml (index.html "" page result archiver)

let getContactsCount: EndpointHandler =
    fun ctx ->
        let count = ContactService.count()
        ctx.WriteText $"({count} total Contacts)"

let getNewContact: EndpointHandler =
    writeHtml (new'.html ModelState.Empty)

let insertContact: EndpointHandler =
    fun ctx ->
        task {
            match! ctx.BindAndValidateForm<ContactDTO>() with
            | ValidationResult.Valid validatedContact ->
                validatedContact.ToDomain()
                |> ContactService.add
                |> ignore
                flash "Created new Contact!" ctx
                return ctx.Response.Redirect("/contacts")
            | ValidationResult.Invalid invalidModel ->
                return!
                    invalidModel
                    |> ModelState.Invalid
                    |> new'.html
                    |> writeHtml
                    <| ctx
        }

let updateContact id: EndpointHandler =
    fun ctx ->
        task {
            match! ctx.BindAndValidateForm<ContactDTO>() with
            | ValidationResult.Valid validatedContact ->
                let domainContact = validatedContact.ToDomain()
                ContactService.update({domainContact with Id = id})
                flash "Updated Contact!" ctx
                return ctx.Response.Redirect($"/contacts/{id}")
            | ValidationResult.Invalid (contactDto, errors) ->
                return!
                    ({ contactDto with id = id }, errors)
                    |> ModelState.Invalid
                    |> edit.html
                    |> writeHtml
                    <| ctx
        }

let viewContact id: EndpointHandler =
    let contact = ContactService.find id
    writeHtml <| show.html contact

let getEditContact id: EndpointHandler =
    id
    |> ContactService.find
    |> ContactDTO.FromDomain
    |> ModelState.Valid
    |> edit.html
    |> writeHtml

let deleteContact id: EndpointHandler =
    fun ctx ->
        task {
            ContactService.delete id |> ignore
            match ctx.TryGetHeaderValue HxRequestHeader.Trigger with
            | Some "delete-btn" ->
                flash "Deleted Contact!" ctx
                ctx.Response.Redirect("/contacts")
                ctx.SetStatusCode(303)
            | _ ->
                ()
        }

let deleteContacts (ctx: HttpContext) =
    match ctx.TryGetFormValues "selected_contact_ids" with
    | Some ids ->
        for id in ids do
            id |> int |> ContactService.delete |> ignore
        flash "Deleted Contacts!" ctx
    | None ->
        ()
    let page = 1
    let result =
        ContactService.all page
        |> Seq.toArray
    ctx |> writeHtml (index.html "" page result archiver)

let validateEmail id: EndpointHandler =
    fun ctx ->
        match ctx.TryGetQueryValue("email") with
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

let startArchive: EndpointHandler =
    fun ctx ->
        archiver <- Archiver(ContactService.contactDb)
        archiver.Run() |> ignore
        ctx.WriteHtmlView (index.archiveUi archiver)

let getArchiveStatus: EndpointHandler =
    fun ctx ->
        ctx.WriteHtmlView (index.archiveUi archiver)

let deleteArchive: EndpointHandler =
    fun ctx ->
        archiver.Reset()
        ctx.WriteHtmlView (index.archiveUi archiver)
