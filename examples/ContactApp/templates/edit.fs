﻿module ContactApp.templates.edit
open ContactApp.Models
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let html (contact: ContactDTO) =
    __() {
        form(action= $"/contacts/{contact.id}/edit", method="post") {
            fieldset() {
                legend() { "Contact Values" }
                div() {
                    p() {
                        label(for'="email") { "Email" }
                        input(name="email", id="email", type'="text", placeholder="Email", value=contact.email)
                        span(class'="error") { contact.GetError("email") }
                    }
                    p() {
                        label(for'="first") { "First Name" }
                        input(name="first", id="firs", type'="text", placeholder="First Name", value=contact.first)
                        span(class'="error") { contact.GetError("first") }
                    }
                    p() {
                        label(for'="last")  { "Last Name" }
                        input(name="last", id="last", type'="text", placeholder="Last Name", value=contact.last)
                        span(class'="error") { contact.GetError("last") }
                    }
                    p() {
                        label(for'="phone") { "Phone" }
                        input(name="phone", id="phone", type'="text", placeholder="Phone", value=contact.phone)
                        span(class'="error") { contact.GetError("phone") }
                    }
                }
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

