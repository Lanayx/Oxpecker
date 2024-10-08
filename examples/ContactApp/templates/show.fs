module ContactApp.templates.show

open Oxpecker.ViewEngine
open ContactApp.Models
open ContactApp.templates.shared

let html (contact: Contact) =
    Fragment() {
        h1() { $"{contact.First} {contact.Last}" }

        div() {
            div() { $"Phone: {contact.Phone}" }
            div() { $"Email: {contact.Email}" }
        }

        p() {
            a(href= $"/contacts/{contact.Id}/edit") { "Edit" }
            a(href="/contacts") { "Back" }
        }
    }
    |> layout.html
