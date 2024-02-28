module ContactApp.ContactService

open System
open System.Threading
open ContactApp.Models

let private contactDb = ResizeArray([
    { Id = 1; First =  "John"; Last = "Smith"; Email = "john@example.com"; Phone = "123-456-7890" }
    { Id = 2; First =  "Dana"; Last = "Crandith"; Email = "dcran@example.com"; Phone = "123-456-7890" }
    { Id = 3; First =  "Edith"; Last = "Neutvaar"; Email = "en@example.com"; Phone = "123-456-7890" }
    { Id = 4; First =  "John2"; Last = "Smith"; Email = "john2@example.com"; Phone = "123-456-7890" }
    { Id = 5; First =  "Dana2"; Last = "Crandith"; Email = "dcran2@example.com"; Phone = "123-456-7890" }
    { Id = 6; First =  "Edith2"; Last = "Neutvaar"; Email = "en2@example.com"; Phone = "123-456-7890" }
    { Id = 7; First =  "John3"; Last = "Smith"; Email = "john3@example.com"; Phone = "123-456-7890" }
    { Id = 8; First =  "Dana3"; Last = "Crandith"; Email = "dcran3@example.com"; Phone = "123-456-7890" }
    { Id = 9; First =  "Edith3"; Last = "Neutvaar"; Email = "en3@example.com"; Phone = "123-456-7890" }
    { Id = 10; First =  "John4"; Last = "Smith"; Email = "john4@example.com"; Phone = "123-456-7890" }
    { Id = 11; First =  "Dana4"; Last = "Crandith"; Email = "dcran4@example.com"; Phone = "123-456-7890" }
    { Id = 12; First =  "Edith4"; Last = "Neutvaar"; Email = "en4@example.com"; Phone = "123-456-7890" }
])

let count() =
    Thread.Sleep 2000
    contactDb.Count

let searchContact (search: string) =
    contactDb
    |> Seq.filter(fun c -> c.First.Contains(search, StringComparison.OrdinalIgnoreCase)
                                     || c.Last.Contains(search, StringComparison.OrdinalIgnoreCase))

let all page =
    contactDb |> Seq.skip ((page-1)*5) |> Seq.truncate 5

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

let delete id =
    contactDb.RemoveAll(fun c -> c.Id = id)

let validateEmail (contact: Contact) =
    let existingContact = contactDb |> Seq.tryFind(fun c -> c.Email = contact.Email)
    match existingContact with
    | Some c when c.Id <> contact.Id -> false
    | _ -> true
