module ContactApp.ContactService

open System
open ContactApp.Models

let private contactDb = ResizeArray([
    { id = 1; first =  "John"; last = "Smith"; email = "john@example.com"; phone = "123-456-7890" }
    { id = 2; first =  "Dana"; last = "Crandith"; email = "dcran@example.com"; phone = "123-456-7890" }
    { id = 3; first =  "Edith"; last = "Neutvaar"; email = "en@example.com"; phone = "123-456-7890" }
])

let searchContact (search: string) =
    contactDb |> Seq.filter(fun c -> c.first.Contains(search, StringComparison.OrdinalIgnoreCase)
                                     || c.last.Contains(search, StringComparison.OrdinalIgnoreCase))

let all (): Contact seq =
    contactDb

let add (contact: Contact) =
    let newId = contactDb |> Seq.maxBy(fun c -> c.id) |> fun c -> c.id + 1
    let newContact = { contact with id = newId }
    contactDb.Add(newContact)
    newContact
