module ContactApp.ContactService

open System
open ContactApp.Models

let private contactDb = ResizeArray([
    { Id = 1; First =  "John"; Last = "Smith"; Email = "john@example.com"; Phone = "123-456-7890" }
    { Id = 2; First =  "Dana"; Last = "Crandith"; Email = "dcran@example.com"; Phone = "123-456-7890" }
    { Id = 3; First =  "Edith"; Last = "Neutvaar"; Email = "en@example.com"; Phone = "123-456-7890" }
])

let searchContact (search: string) =
    contactDb |> Seq.filter(fun c -> c.First.Contains(search, StringComparison.OrdinalIgnoreCase)
                                     || c.Last.Contains(search, StringComparison.OrdinalIgnoreCase))

let all (): Contact seq =
    contactDb

let add (contact: Contact) =
    let newId = contactDb |> Seq.maxBy(fun c -> c.Id) |> fun c -> c.Id + 1
    let newContact = { contact with Id = newId }
    contactDb.Add(newContact)
    newContact

let find id =
    contactDb.Find(fun c -> c.Id = id)

let update (contact: Contact) =
    let index = contactDb.FindIndex(fun c -> c.Id = contact.Id)
    contactDb[index] <- contact
