module ContactApp.templates.index
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open ContactApp.Models
open ContactApp.templates.shared

let html q page (contacts: Contact[]) =
    __() {
        form(action="/contacts", method="get") {
            label(for'="search") { "Search Term" }
            input(id="search", type'="search", name="q", value=q, style="margin: 0 5px",
                  hxGet="/contacts",
                  hxTrigger="search, keyup delay:200ms changed",
                  hxTarget="tbody",
                  hxSelect="tbody tr")
            input(type'="submit", value="Search")
        }
        table() {
            thead() {
                tr() {
                    th(){"First"}; th(){"Last"}; th(){"Phone"}; th(){"Email"}; th()
                }
            }
            tbody() {
                for contact in contacts do
                    tr() {
                        td() { contact.First }
                        td() { contact.Last }
                        td() { contact.Phone }
                        td() { contact.Email }
                        td() {
                            a(href= $"/contacts/{contact.Id}/edit"){ "Edit" }
                            a(href= $"/contacts/{contact.Id}"){ "View" }
                        }
                    }
                if contacts.Length = 5 then
                    tr() {
                        td(colspan=5, style="text-align: center") {
                            span(hxTarget="closest tr",
                                    hxTrigger="revealed",
                                    hxSwap="outerHTML",
                                    hxSelect="tbody > tr",
                                    hxGet= $"/contacts?page={page + 1}"){
                              "Loading More..."
                            }
                        }
                    }
            }
        }

        p() {
            a(href="/contacts/new") { "Add Contact" }
        }
    }
    |> layout.html
