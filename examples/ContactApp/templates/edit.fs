module ContactApp.templates.edit

open Oxpecker.ModelValidation
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open ContactApp.Models
open ContactApp.templates.shared

let html (contact: ModelState<ContactDTO>) =
    let contactId = contact.Value(_.id >> string)
    Fragment() {
        form(action= $"/contacts/{contactId}/edit", method="post") {
            fieldset() {
                legend() { "Contact Values" }
                contactFields.html contact
                button() { "Save" }
            }
        }

        button(id="delete-btn",
               hxDelete= $"/contacts/{contactId}",
               hxPushUrl="true",
               hxConfirm="Are you sure you want to delete this contact?",
               hxTarget="body") { "Delete Contact" }

        p() {
            a(href="/contacts") { "Back" }
        }
    }
    |> layout.html

