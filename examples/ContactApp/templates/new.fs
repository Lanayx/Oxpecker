module ContactApp.templates.new'
open ContactApp.Models
open Oxpecker.ViewEngine
open ContactApp.templates.shared

let html (contact: ContactDTO) =
    Fragment() {
        form(action="/contacts/new", method="post") {
            fieldset() {
                legend() { "Contact Values" }
                contactFields.html contact
                button() { "Save" }
            }
        }

        p() {
            a(href="/contacts") { "Back" }
        }
    }
    |> layout.html

