module ContactApp.templates.index
open ContactApp.Tools
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Oxpecker.ViewEngine.Aria
open ContactApp.Models
open ContactApp.templates.shared

let archiveUi (archiver: Archiver) =
    div(id="archive-ui").attr(hxTarget "this", hxSwap "outerHTML") {
        if archiver.Status = "Waiting" then
            button().attr(hxPost "/contacts/archive"){
                "Download Contact Archive"
            }
    elif archiver.Status = "Running" then
        div().attr(hxGet "/contacts/archive", hxTrigger "load delay:500ms") {
            "Creating Archive..."
            div(class'="progress") {
                div(id="archive-progress", class'="progress-bar", role="progressbar",
                     ariaValueNow= $"{archiver.Progress * 100.}",
                     style= $"width:{archiver.Progress * 100.}%%")
            }
        }
    elif archiver.Status = "Complete" then
        a(href="/contacts/archive/file").attr(hxBoost false) {
            "Archive Ready!  Click here to download. "
            raw "&downarrow;"
        }
        button().attr(hxDelete "/contacts/archive"){ "Clear Download" }
    }

let rows page (contacts: Contact[]) =
    Fragment() {
        for contact in contacts do
            tr() {
                td() { input(type'="checkbox", name="selected_contact_ids", value= $"{contact.Id}") }
                td() { contact.First }
                td() { contact.Last }
                td() { contact.Phone }
                td() { contact.Email }
                td() {
                    a(href= $"/contacts/{contact.Id}/edit"){ "Edit" }
                    a(href= $"/contacts/{contact.Id}"){ "View" }
                    a(href= "#")
                        .attr(hxDelete $"/contacts/{contact.Id}",
                              hxSwap "outerHTML swap:1s",
                              hxConfirm "Are you sure you want to delete this contact?",
                              hxTarget "closest tr"){ "Delete" }
                }
            }
        if contacts.Length = 5 then
            tr() {
                td(colspan=5, style="text-align: center") {
                    span().attr(hxTarget "closest tr",
                                hxTrigger "revealed",
                                hxSwap "outerHTML",
                                hxSelect "tbody > tr",
                                hxGet $"/contacts?page={page + 1}"){
                      "Loading More..."
                    }
                }
            }
    }

let html q page (contacts: Contact[]) archiver =
    Fragment() {
        archiveUi archiver
        form(action="/contacts", method="get") {
            label(for'="search") { "Search Term" }
            input(id="search", type'="search", name="q", value=q, style="margin: 0 5px", autocomplete="off")
                .attr(hxGet "/contacts",
                      hxTrigger "search, keyup delay:200ms changed",
                      hxTarget "tbody",
                      hxPushUrl "true",
                      hxIndicator "#spinner")
            img(id="spinner", class'="spinner htmx-indicator", src="/spinning-circles.svg", alt="Request In Flight...")
            input(type'="submit", value="Search")
        }
        form() {
            table() {
                thead() {
                    tr() {
                        th(); th(){"First"}; th(){"Last"}; th(){"Phone"}; th(){"Email"}; th()
                    }
                }
                tbody() {
                    rows page contacts
                }
            }

            p() {
                span (style="float: left") {
                    button().attr(hxDelete "/contacts",
                                  hxConfirm "Are you sure you want to delete these contacts?",
                                  hxTarget "body",
                                  hxInclude "closest form") {
                        "Delete Selected Contacts"
                    }
                }

                span(style="float: right") {
                    a(href="/contacts/new") { "Add Contact" }
                    span().attr(hxGet "/contacts/count", hxTrigger "revealed"){
                        img(class'="spinner htmx-indicator", src="/spinning-circles.svg")
                    }
                }
            }
        }
    }
    |> layout.html
