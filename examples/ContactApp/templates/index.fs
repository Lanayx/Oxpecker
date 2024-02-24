module ContactApp.templates.index
open Oxpecker.ViewEngine
open ContactApp.Models
open ContactApp.templates.shared

let html q page (contacts: Contact[]) =
    __() {
        form(action="/contacts", method="get") {
            label(for'="search") { "Search Term" }
            input(id="search", type'="search", name="q", value=q, style="margin: 0 5px")
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
                        td(colspan="5", style="text-align: center") {
                            button(hxTarget="closest tr",
                                    hxSwap="outerHTML",
                                    hxSelect="tbody > tr",
                                    hxGet= $"/contacts?page={page + 1}"){
                              "Load More"
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
