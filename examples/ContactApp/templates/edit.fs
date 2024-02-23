module ContactApp.templates.edit

open Oxpecker.ViewEngine
open Oxpecker.Htmx
open ContactApp.Models
open ContactApp.templates.shared

let html (contact: ContactDTO) =
    __() {
        form(action= $"/contacts/{contact.id}/edit", method="post") {
            fieldset() {
                legend() { "Contact Values" }
                contactFields.html contact
                button() { "Save" }
            }
        }

        button(hxDelete= $"/contacts/{contact.id}",
               hxTarget="body",
               hxPushUrl="true",
               hxConfirm="Are you sure you want to delete this contact?") { "Delete Contact" }

        p() {
            a(href="/contacts") { "Back" }
        }
    }
    |> layout.html

