namespace ContactApp.templates.shared

open ContactApp.templates.shared.errors
open Oxpecker.Htmx
open Oxpecker.ModelValidation

module contactFields =

    open ContactApp.Models
    open Oxpecker.ViewEngine

    let html (contact: ModelState<ContactDTO>) =
        let x = Unchecked.defaultof<ContactDTO>
        let showErrors = showErrors contact
        div() {
            p() {
                label(for'="email") { "Email" }
                input(name=nameof x.email, id="email", type'="email", placeholder="Email", value=contact.Value(_.email),
                      hxTrigger="change, keyup delay:200ms changed",
                      hxGet= $"/contacts/{contact.Value(_.id >> string)}/email", hxTarget="next .error")
                showErrors <| nameof x.email
            }
            p() {
                label(for'="first") { "First Name" }
                input(name=nameof x.first, id="first", type'="text", placeholder="First Name", value=contact.Value(_.first))
                showErrors <| nameof x.first
            }
            p() {
                label(for'="last")  { "Last Name" }
                input(name=nameof x.last, id="last", type'="text", placeholder="Last Name", value=contact.Value(_.last))
                showErrors <| nameof x.last
            }
            p() {
                label(for'="phone") { "Phone" }
                input(name=nameof x.phone, id="phone", type'="text", placeholder="Phone", value=contact.Value(_.phone))
                showErrors <| nameof x.phone
            }
        }
