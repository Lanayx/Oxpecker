module ContactApp.templates.index
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open ContactApp.Models
open ContactApp.templates.shared

let rows page (contacts: Contact[]) =
    __() {
        for contact in contacts do
            tr() {
                td() { contact.First }
                td() { contact.Last }
                td() { contact.Phone }
                td() { contact.Email }
                td() {
                    a(href= $"/contacts/{contact.Id}/edit"){ "Edit" }
                    a(href= $"/contacts/{contact.Id}"){ "View" }
                    a(href= "#", hxDelete= $"/contacts/{contact.Id}",
                      hxSwap="outerHTML",
                      hxConfirm="Are you sure you want to delete this contact?",
                      hxTarget="closest tr"){ "Delete" }
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


let html q page (contacts: Contact[]) =
    __() {
        form(action="/contacts", method="get") {
            label(for'="search") { "Search Term" }
            input(id="search", type'="search", name="q", value=q, style="margin: 0 5px", autocomplete="off",
                  hxGet="/contacts",
                  hxTrigger="search, keyup delay:200ms changed",
                  hxTarget="tbody",
                  hxPushUrl="true",
                  hxIndicator="#spinner")
            img(id="spinner", class'="spinner htmx-indicator", src="/spinning-circles.svg", alt="Request In Flight...")
            input(type'="submit", value="Search")
        }
        table() {
            thead() {
                tr() {
                    th(){"First"}; th(){"Last"}; th(){"Phone"}; th(){"Email"}; th()
                }
            }
            tbody() {
                rows page contacts
            }
        }

        p() {
            a(href="/contacts/new") { "Add Contact" }
            span(hxGet="/contacts/count", hxTrigger="revealed"){
                img(class'="spinner htmx-indicator", src="/spinning-circles.svg")
            }
        }
    }
    |> layout.html
