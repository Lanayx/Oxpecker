module ContactApp.templates.edit

open Oxpecker.ViewEngine
open Oxpecker.Htmx
open ContactApp.Models
open ContactApp.templates.shared

let html (contact: ContactDTO) =
    Fragment() {
        form(action= $"/contacts/{contact.id}/edit", method="post") {
            fieldset() {
                legend() { "Contact Values" }
                contactFields.html contact
                button() { "Save" }
            }
        }

        button(id="delete-btn",
               hxDelete= $"/contacts/{contact.id}",
               hxPushUrl="true",
               hxConfirm="Are you sure you want to delete this contact?",
               hxTarget="body") { "Delete Contact" }

        p() {
            a(href="/contacts") { "Back" }
        }
    }
    |> layout.html

