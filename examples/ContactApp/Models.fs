module ContactApp.Models
open System.ComponentModel.DataAnnotations


type Contact = {
    Id: int
    First: string
    Last: string
    Phone: string
    Email: string
}

[<CLIMutable>]
type ContactDTO = {
    id: int
    [<Required>]
    first: string
    [<Required>]
    last: string
    [<Required>]
    phone: string
    [<Required>]
    [<EmailAddress>]
    email: string
} with
    member this.ToDomain() =
        { Id = this.id; First = this.first; Last = this.last; Phone = this.phone; Email = this.email }
    static member FromDomain(contact: Contact) =
        { id = contact.Id; first = contact.First; last = contact.Last; phone = contact.Phone; email = contact.Email }
