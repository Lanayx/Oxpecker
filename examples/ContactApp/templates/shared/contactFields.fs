namespace ContactApp.templates.shared

module contactFields =

    open ContactApp.Models
    open Oxpecker.ViewEngine

    let html (contact: ContactDTO) =
        div() {
            p() {
                label(for'="email") { "Email" }
                input(name="email", id="email", type'="email", placeholder="Email", value=contact.email)
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
