module ContactApp.Models
open System
open System.Collections.Generic


type Contact = {
    id: int
    first: string
    last: string
    phone: string
    email: string
}

[<CLIMutable>]
type ContactDTO = {
    id: int
    first: string
    last: string
    phone: string
    email: string
    errors: IDictionary<string, string>
} with
    member this.GetError key =
        match this.errors.TryGetValue key with
        | true, value -> value
        | _ -> ""
    member this.Validate() =
        let errors = Dictionary<string, string>()
        if String.IsNullOrEmpty(this.first) then errors.Add("first", "First name is required")
        if String.IsNullOrEmpty(this.last) then errors.Add("last", "Last name is required")
        if String.IsNullOrEmpty(this.phone) then errors.Add("phone", "Phone is required")
        if String.IsNullOrEmpty(this.email) then errors.Add("email", "Email is required")
        { this with errors = errors }
    member this.ToDomain() =
        { id = this.id; first = this.first; last = this.last; phone = this.phone; email = this.email }
