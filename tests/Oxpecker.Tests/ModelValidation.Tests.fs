module Oxpecker.Tests.ModelValidation

open System
open System.Collections.Generic
open System.ComponentModel
open System.Globalization
open System.IO
open System.Text
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Oxpecker
open System.ComponentModel.DataAnnotations
open Xunit
open FsUnit.Light

type Model = {
    Name: string | null
    [<Range(0, 100)>]
    Age: int
    [<Required>]
    [<MinLength(4)>]
    [<EmailAddress>]
    Email: string
    Active: bool
}

[<Fact>]
let ``Valid model doesn't raise any issues`` () =
    let model = {
        Name = "John"
        Age = 30
        Email = "my@email.com"
        Active = true
    }
    let result = validateModel model
    result.IsInvalid |> shouldEqual false
    match result with
    | ModelValidationResult.Valid x ->
        x |> shouldEqual model
        let modelState = ModelState.Valid model
        modelState.Value(_.Name) |> shouldEqual "John"
        modelState.BoolValue(_.Active) |> shouldEqual true
        modelState.Value(_.Age >> string) |> shouldEqual "30"
        modelState.Value(_.Email) |> shouldEqual "my@email.com"
    | ModelValidationResult.Invalid _ -> failwith "Expected valid model"


[<Fact>]
let ``Invalid model raises issues`` () =
    let model = {
        Name = null
        Age = 200
        Email = "abc"
        Active = false
    }
    let result = validateModel model
    result.IsValid |> shouldEqual false
    match result with
    | ModelValidationResult.Invalid(x, errors) ->
        x |> shouldEqual model
        errors.All |> shouldHaveLength 3
        errors.ErrorMessagesFor(nameof x.Age)
        |> Seq.head
        |> shouldEqual "The field Age must be between 0 and 100."
        errors.ErrorMessagesFor(nameof x.Email)
        |> Seq.toList
        |> shouldEqual [
            "The field Email must be a string or array type with a minimum length of '4'."
            "The Email field is not a valid e-mail address."
        ]
        errors.ErrorMessagesFor(nameof x.Name) |> shouldBeEmpty
    | _ -> failwith "Expected invalid model"

[<Fact>]
let ``Empty model returns default values`` () =
    let model = ModelState.Empty
    model.Value(_.Name) |> shouldEqual null
    model.BoolValue(_.Active) |> shouldEqual false


type FooService(value: int) =
    member val Value = value

type ValidatableModel = {
    Count: int
} with
    interface IValidatableObject with
        member this.Validate(ctx: ValidationContext) =
            let foo = ctx.GetRequiredService<FooService>().Value

            seq {
                if this.Count > foo then
                    yield ValidationResult($"Count is above {foo}", [ nameof this.Count ])
            }

[<Fact>]
let ``Unresolved services from ValidationContext throws exception`` () =
    let model = { Count = 100 }

    let svc = ServiceCollection()
    let sp = svc.BuildServiceProvider()
    let validationContext = ValidationContext(model, sp, null)
    (fun () -> validateModelWith validationContext model |> ignore)
    |> shouldFail<InvalidOperationException>

[<Fact>]
let ``Services must be resolved from ValidationContext`` () =
    let model = { Count = 999 }

    let value = 200
    let svc = ServiceCollection()
    svc.AddSingleton<FooService>(FooService(value)) |> ignore
    let sp = svc.BuildServiceProvider()
    let validationContext = ValidationContext(model, sp, null)

    let result = validateModelWith validationContext model
    result.IsValid |> shouldEqual false
    match result with
    | ModelValidationResult.Invalid(x, errors) ->
        x |> shouldEqual model
        errors.All |> shouldHaveLength 1
        errors.ErrorMessagesFor(nameof x.Count)
        |> Seq.head
        |> shouldEqual $"Count is above {value}"
    | _ -> failwith "Expected invalid model"


// ---------------------------------
// Recursive validation
// ---------------------------------

type Address = {
    [<Required>]
    [<MinLength(3)>]
    Street: string
    [<Range(1, 99999)>]
    [<Display(Name = "Postal code")>]
    Zip: int
}

let private streetError = "The field Street must be a string or array type with a minimum length of '3'."
let private zipError = "The field Postal code must be between 1 and 99999."

let private validAddress = { Street = "Main street"; Zip = 12345 }

/// Different arguments give structurally different invalid addresses.
let private invalidAddress (seed: int) = { Street = "x"; Zip = -seed }

let private errorsOf (result: ModelValidationResult<'T>) =
    match result with
    | ModelValidationResult.Invalid(_, errors) -> errors
    | ModelValidationResult.Valid _ -> failwith "Expected invalid model"

let private memberNames (errors: ValidationErrors) =
    errors.All |> Seq.collect _.MemberNames |> Seq.toList

let private messagesFor (name: string) (errors: ValidationErrors) =
    errors.ErrorMessagesFor name |> Seq.toList

type Customer = {
    [<Required>]
    Name: string | null
    Address: Address
}

[<Fact>]
let ``Nested object is validated`` () =
    let errors =
        validateModel {
            Name = "John"
            Address = invalidAddress 1
        }
        |> errorsOf
    memberNames errors |> shouldEqual [ "Address.Street"; "Address.Zip" ]
    errors |> messagesFor "Address.Street" |> shouldEqual [ streetError ]
    errors |> messagesFor "Address.Zip" |> shouldEqual [ zipError ]
    errors |> messagesFor "Street" |> shouldBeEmpty

[<Fact>]
let ``Valid nested object doesn't raise any issues`` () =
    let result =
        validateModel {
            Name = "John"
            Address = validAddress
        }
    result.IsValid |> shouldEqual true

[<Fact>]
let ``Errors of an object and of its nested objects are reported together`` () =
    let errors =
        validateModel {
            Name = null
            Address = invalidAddress 1
        }
        |> errorsOf
    memberNames errors |> shouldEqual [ "Name"; "Address.Street"; "Address.Zip" ]

[<Fact>]
let ``Anonymous record is validated`` () =
    validateModel {|
        Sender = validAddress
        Receiver = invalidAddress 1
    |}
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Receiver.Street"; "Receiver.Zip" ]

type Category = {
    [<Required>]
    Title: string | null
    Subcategories: Category list
}

[<Fact>]
let ``Recursive type is validated on every level`` () =
    let errors =
        validateModel {
            Title = "Books"
            Subcategories = [
                {
                    Title = "Fiction"
                    Subcategories = [ { Title = null; Subcategories = [] } ]
                }
                { Title = null; Subcategories = [] }
            ]
        }
        |> errorsOf
    memberNames errors
    |> shouldEqual [ "Subcategories[0].Subcategories[0].Title"; "Subcategories[1].Title" ]

/// Every level is another type: Nest<int> holds Nest<int list>, which holds Nest<int list list>, and so on without an end.
type Nest<'T> = {
    [<Required>]
    Label: string | null
    Inner: Nest<'T list> option
}

[<Fact>]
let ``Generic type nesting ever growing instances of itself is validated`` () =
    let model: Nest<int> = {
        Label = "outer"
        Inner =
            Some {
                Label = null
                Inner = Some { Label = null; Inner = None }
            }
    }
    validateModel model
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Inner.Label"; "Inner.Inner.Label" ]

type Box<'T> = { Content: 'T }

[<Fact>]
let ``Generic type holding another instance of itself is validated`` () =
    validateModel {
        Content = { Content = invalidAddress 1 }
    }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Content.Content.Street"; "Content.Content.Zip" ]

    (validateModel { Content = { Content = validAddress } }).IsValid
    |> shouldEqual true
    (validateModel { Content = { Content = 1 } }).IsValid |> shouldEqual true

type Shipment = {
    Stops: Address list
    Depots: Address[]
    Returns: ResizeArray<Address>
    Detours: Address seq
}

let private emptyShipment = {
    Stops = []
    Depots = [||]
    Returns = ResizeArray()
    Detours = Seq.empty
}

[<Fact>]
let ``Collection items are validated`` () =
    let errors =
        validateModel {
            Stops = [ validAddress; invalidAddress 1 ]
            Depots = [| invalidAddress 2; validAddress |]
            Returns = ResizeArray [ validAddress; validAddress; invalidAddress 3 ]
            Detours =
                seq {
                    validAddress
                    invalidAddress 4
                }
        }
        |> errorsOf
    memberNames errors
    |> shouldEqual [
        "Stops[1].Street"
        "Stops[1].Zip"
        "Depots[0].Street"
        "Depots[0].Zip"
        "Returns[2].Street"
        "Returns[2].Zip"
        "Detours[1].Street"
        "Detours[1].Zip"
    ]
    errors |> messagesFor "Returns[2].Zip" |> shouldEqual [ zipError ]

[<Fact>]
let ``Empty collections are valid`` () =
    (validateModel emptyShipment).IsValid |> shouldEqual true

[<Fact>]
let ``Null collection items are skipped and keep the index of the following ones`` () =
    let errors =
        validateModel {
            emptyShipment with
                Depots = [| Unchecked.defaultof<Address>; invalidAddress 1 |]
        }
        |> errorsOf
    memberNames errors |> shouldEqual [ "Depots[1].Street"; "Depots[1].Zip" ]

[<Fact>]
let ``Structurally equal items are all reported`` () =
    let errors =
        validateModel {
            emptyShipment with
                Stops = [ { Street = "x"; Zip = 0 }; validAddress; { Street = "x"; Zip = 0 } ]
        }
        |> errorsOf
    memberNames errors
    |> shouldEqual [ "Stops[0].Street"; "Stops[0].Zip"; "Stops[2].Street"; "Stops[2].Zip" ]

[<Fact>]
let ``Object referenced from several places is validated once`` () =
    let shared = invalidAddress 1
    let errors =
        validateModel {
            emptyShipment with
                Stops = [ shared ]
                Depots = [| shared |]
        }
        |> errorsOf
    memberNames errors |> shouldEqual [ "Stops[0].Street"; "Stops[0].Zip" ]

type Delivery = {
    Billing: Address option
    Pickup: Address voption
}

[<Fact>]
let ``Option values are validated without extra path segments`` () =
    let errors =
        validateModel {
            Billing = Some(invalidAddress 1)
            Pickup = ValueSome(invalidAddress 2)
        }
        |> errorsOf
    memberNames errors
    |> shouldEqual [ "Billing.Street"; "Billing.Zip"; "Pickup.Street"; "Pickup.Zip" ]

[<Fact>]
let ``Missing option values are valid`` () =
    let result = validateModel { Billing = None; Pickup = ValueNone }
    result.IsValid |> shouldEqual true

type Network = {
    Warehouses: Map<string, Address>
    Shops: Dictionary<int, Address>
}

[<Fact>]
let ``Dictionary values are validated and addressed by key`` () =
    let shops = Dictionary<int, Address>()
    shops[3] <- validAddress
    shops[7] <- invalidAddress 2
    let errors =
        validateModel {
            Warehouses = Map [ "north", validAddress; "south", invalidAddress 1 ]
            Shops = shops
        }
        |> errorsOf
    memberNames errors
    |> shouldEqual [
        "Warehouses[south].Street"
        "Warehouses[south].Zip"
        "Shops[7].Street"
        "Shops[7].Zip"
    ]

type Atlas = { Places: Collections.Hashtable }

[<Fact>]
let ``Values of non-generic dictionaries are validated and addressed by key`` () =
    let places = Collections.Hashtable()
    places["office"] <- invalidAddress 1
    validateModel { Places = places }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Places[office].Street"; "Places[office].Zip" ]

type Contact =
    | Postal of Address
    | Courier of company: string * depot: Address
    | Nobody

type Subscriber = { Contact: Contact }

[<Fact>]
let ``Union case fields are validated`` () =
    validateModel { Contact = Postal(invalidAddress 1) }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Contact.Item.Street"; "Contact.Item.Zip" ]

    validateModel {
        Contact = Courier("Fast", invalidAddress 2)
    }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Contact.depot.Street"; "Contact.depot.Zip" ]

    (validateModel { Contact = Nobody }).IsValid |> shouldEqual true
    (validateModel { Contact = Postal validAddress }).IsValid |> shouldEqual true

[<Struct>]
type Gauge = {
    [<Range(1, 10)>]
    Level: int
}

/// A struct union has the fields of all its cases, those of the other cases hold default values.
[<Struct>]
type Reading =
    | Missing
    | Measured of gauge: Gauge
    | Estimated of guess: Gauge * note: string

type Sensor = { Reading: Reading }

[<Fact>]
let ``Struct union is validated through the fields of its case only`` () =
    // the default gauge is invalid, but no gauge belongs to these values
    (validateModel Missing).IsValid |> shouldEqual true
    (validateModel { Reading = Missing }).IsValid |> shouldEqual true
    (validateModel(Measured { Level = 5 })).IsValid |> shouldEqual true

    validateModel(Measured { Level = 50 })
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "gauge.Level" ]

    validateModel {
        Reading = Estimated({ Level = 0 }, "low")
    }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Reading.guess.Level" ]

/// Cases of a struct union may share a field when its name and type are the same.
[<Struct>]
type Dial =
    | Idle
    | Manual of gauge: Gauge
    | Automatic of gauge: Gauge * program: string

[<Fact>]
let ``Field shared by the cases of a struct union is validated for each of them`` () =
    (validateModel Idle).IsValid |> shouldEqual true
    (validateModel(Manual { Level = 5 })).IsValid |> shouldEqual true

    validateModel(Manual { Level = 50 })
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "gauge.Level" ]

    validateModel(Automatic({ Level = 0 }, "eco"))
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "gauge.Level" ]

type Transfer = { Legs: Address * Address }

[<Fact>]
let ``Tuple items are validated`` () =
    validateModel {
        Legs = (validAddress, invalidAddress 1)
    }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Legs.Item2.Street"; "Legs.Item2.Zip" ]

[<Fact>]
let ``Collection model is validated`` () =
    validateModel [ validAddress; invalidAddress 1 ]
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "[1].Street"; "[1].Zip" ]

    validateModel [ [ invalidAddress 2 ]; [] ]
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "[0][0].Street"; "[0][0].Zip" ]

    (validateModel([]: Address list)).IsValid |> shouldEqual true

[<Struct>]
type Point = {
    [<Range(0, 10)>]
    X: int
}

type Marker = {
    Position: Point
    Target: Nullable<Point>
}

[<Fact>]
let ``Struct models are validated`` () =
    validateModel { X = 50 } |> errorsOf |> memberNames |> shouldEqual [ "X" ]

    validateModel {
        Position = { X = 50 }
        Target = Nullable { X = 60 }
    }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Position.X"; "Target.X" ]

    let result =
        validateModel {
            Position = { X = 5 }
            Target = Nullable()
        }
    result.IsValid |> shouldEqual true

[<AbstractClass>]
type Shape() = class end

type Circle(radius: int) =
    inherit Shape()
    [<Range(1, 10)>]
    member _.Radius = radius

type Drawing = { Figure: Shape; Attachment: obj }

[<Fact>]
let ``Runtime type of a value is validated`` () =
    validateModel {
        Figure = Circle 50
        Attachment = invalidAddress 1
    }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Figure.Radius"; "Attachment.Street"; "Attachment.Zip" ]

type TreeNode(name: string | null) =
    [<Required>]
    member val Name = name with get, set
    member val Parent: TreeNode | null = null with get, set
    member val Children = ResizeArray<TreeNode>() with get

[<Fact>]
let ``Cyclic object graph is validated once`` () =
    let root = TreeNode("root")
    let child = TreeNode(null)
    let grandchild = TreeNode(null)
    root.Parent <- root
    root.Children.Add child
    child.Parent <- root
    child.Children.Add grandchild
    grandchild.Parent <- child

    validateModel root
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Children[0].Name"; "Children[0].Children[0].Name" ]

type Fork(name: string | null) =
    [<Required>]
    member val Name = name with get, set
    member val Left: Fork | null = null with get, set
    member val Right: Fork | null = null with get, set

[<Fact>]
let ``Objects sharing their children are validated in linear time`` () =
    // 64 objects, but 2^63 paths lead to the last one: validating it for each path would never end
    let mutable top = Fork(null)
    for _ in 1..63 do
        let parent = Fork("fork")
        parent.Left <- top
        parent.Right <- top
        top <- parent

    validateModel top
    |> errorsOf
    |> memberNames
    |> shouldEqual [ String.replicate 63 "Left." + "Name" ]

[<Fact>]
let ``Too deeply nested model fails with an exception instead of a stack overflow`` () =
    let root = Fork("fork")
    let mutable last = root
    for _ in 1..1_000_000 do
        let next = Fork("fork")
        last.Left <- next
        last <- next

    (fun () -> validateModel root |> ignore)
    |> shouldFail<InsufficientExecutionStackException>

type Envelope(payload: Address) =
    member _.Payload = payload
    member _.Kind = typeof<Address>
    member _.Link = Uri("https://example.org")
    member _.Culture = CultureInfo.InvariantCulture
    member _.Raw = [| 1uy; 2uy |]
    member _.Tags = [ "a"; "b" ]
    member _.Failure = Exception("boom")
    member _.Callback = Func<int>(fun () -> 1)
    member _.Size: int = failwith "A simple property without validation attributes must not be read"

[<Fact>]
let ``Framework types and simple values are not traversed`` () =
    (validateModel(Envelope validAddress)).IsValid |> shouldEqual true

    validateModel(Envelope(invalidAddress 1))
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Payload.Street"; "Payload.Zip" ]

type Slot(name: string | null) =
    let mutable name = name
    /// Returned by reference: cannot be read by compiled code.
    [<Required>]
    member _.Name: byref<string | null> = &name
    member _.Chars: ReadOnlySpan<char> = "abc".AsSpan()

[<Fact>]
let ``Properties returning references and ref structs don't break validation`` () =
    (validateModel(Slot "slot")).IsValid |> shouldEqual true

    validateModel(Slot null) |> errorsOf |> memberNames |> shouldEqual [ "Name" ]

type Money = {
    [<Range(0, 100)>]
    Amount: int
} with
    /// Produces a new object on every call, following it would never end.
    member this.Doubled = { Amount = this.Amount * 2 }
    [<Range(0, 100)>]
    member this.Tripled = this.Amount * 3

type Invoice = { Total: Money }

[<Fact>]
let ``Records are traversed through their fields only`` () =
    (validateModel { Total = { Amount = 10 } }).IsValid |> shouldEqual true

    // members having validation attributes are still validated, as Validator does
    validateModel { Total = { Amount = 50 } }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Total.Tripled" ]

type Credentials = {
    Password: string
    [<Compare("Password")>]
    Confirmation: string
}

type Registration = { Credentials: Credentials }

[<Fact>]
let ``Nested object is the ObjectInstance of its ValidationContext`` () =
    let errors =
        validateModel {
            Credentials = {
                Password = "secret"
                Confirmation = "sekret"
            }
        }
        |> errorsOf
    memberNames errors |> shouldEqual [ "Credentials.Confirmation" ]
    errors
    |> messagesFor "Credentials.Confirmation"
    |> shouldEqual [ "'Confirmation' and 'Password' do not match." ]

type Stay = {
    CheckIn: int
    CheckOut: int
} with
    interface IValidatableObject with
        member this.Validate(_: ValidationContext) =
            seq {
                if this.CheckIn > this.CheckOut then
                    yield ValidationResult("Check-in is after check-out", [ nameof this.CheckIn; nameof this.CheckOut ])
                    yield ValidationResult("Stay is empty")
            }

type Booking = {
    [<Required>]
    Guest: string | null
    Stay: Stay
}

[<Fact>]
let ``Nested IValidatableObject gets qualified member names`` () =
    let errors =
        validateModel {
            Guest = "John"
            Stay = { CheckIn = 5; CheckOut = 1 }
        }
        |> errorsOf
    errors.All |> shouldHaveLength 2
    memberNames errors |> shouldEqual [ "Stay.CheckIn"; "Stay.CheckOut"; "Stay" ]
    errors
    |> messagesFor "Stay.CheckOut"
    |> shouldEqual [ "Check-in is after check-out" ]
    errors |> messagesFor "Stay" |> shouldEqual [ "Stay is empty" ]

/// A collection validating its items itself: it names them by index.
type Batch() =
    inherit ResizeArray<Address>()
    interface IValidatableObject with
        member this.Validate(_: ValidationContext) =
            seq {
                for index in 1 .. this.Count - 1 do
                    if this[index] = this[0] then
                        yield ValidationResult("Address is repeated", [ $"[{index}].Street" ])
            }

type Round = { Batch: Batch }

[<Fact>]
let ``Member names starting with an index are appended to the path as they are`` () =
    let batch = Batch()
    batch.Add validAddress
    batch.Add { validAddress with Zip = 54321 }
    batch.Add validAddress

    validateModel batch |> errorsOf |> memberNames |> shouldEqual [ "[2].Street" ]

    validateModel { Batch = batch }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Batch[2].Street" ]

type Parcel = {
    Destination: Address
} with
    interface IValidatableObject with
        member this.Validate(_: ValidationContext) =
            seq { yield ValidationResult("Parcel is rejected", [ nameof this.Destination ]) }

[<Fact>]
let ``IValidatableObject is validated only when everything nested is valid`` () =
    validateModel { Destination = invalidAddress 1 }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Destination.Street"; "Destination.Zip" ]

    let errors = validateModel { Destination = validAddress } |> errorsOf
    errors |> messagesFor "Destination" |> shouldEqual [ "Parcel is rejected" ]

type Dispatch = { Origin: Address; Parcel: Parcel }

[<Fact>]
let ``Invalid object met again is still an invalid member of the object referring to it`` () =
    // errors of the address are reported once, but the parcel is not validated: its destination is invalid
    let invalid = invalidAddress 1
    validateModel {
        Origin = invalid
        Parcel = { Destination = invalid }
    }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Origin.Street"; "Origin.Zip" ]

    validateModel {
        Origin = validAddress
        Parcel = { Destination = validAddress }
    }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Parcel.Destination" ]

[<Fact>]
let ``Valid object met again is a valid member whatever else is invalid`` () =
    // the parcel is validated itself: its destination is valid, the invalid address is not a part of it
    let shared = validAddress
    validateModel [ box shared; box(invalidAddress 1); box { Destination = shared } ]
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "[1].Street"; "[1].Zip"; "[2].Destination" ]

type Folder(name: string | null) =
    [<Required>]
    member val Name = name with get, set
    member val Entries = ResizeArray<Entry>() with get

and Entry(folder: Folder) =
    member _.Folder = folder
    interface IValidatableObject with
        member _.Validate(_: ValidationContext) =
            seq { yield ValidationResult("Entry is rejected") }

[<Fact>]
let ``Object is not validated itself when the object it is nested in is invalid and is its member`` () =
    let invalid = Folder(null)
    invalid.Entries.Add(Entry invalid)
    validateModel invalid |> errorsOf |> memberNames |> shouldEqual [ "Name" ]

    let valid = Folder("documents")
    valid.Entries.Add(Entry valid)
    validateModel valid |> errorsOf |> memberNames |> shouldEqual [ "Entries[0]" ]

type Shelf(name: string | null) =
    member val Books = ResizeArray<Book>() with get
    [<Required>]
    member val Name = name with get, set

and Book(shelf: Shelf, pages: int) =
    member _.Shelf = shelf
    member _.Pages = pages
    interface IValidatableObject with
        member this.Validate(_: ValidationContext) =
            seq {
                if pages < 1 then
                    yield ValidationResult("Book is empty", [ nameof this.Pages ])
            }

let private shelfOf (name: string | null) (pages: int list) =
    let shelf = Shelf(name)
    for count in pages do
        shelf.Books.Add(Book(shelf, count))
    shelf

[<Fact>]
let ``Object referring back to the object it is nested in is validated itself after the rest of that object`` () =
    // the name of the shelf is checked after its books, and the book is not validated itself: its shelf is invalid
    validateModel(shelfOf null [ 0 ])
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Name" ]

    validateModel(shelfOf "fiction" [ 0 ])
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Books[0].Pages" ]

    (validateModel(shelfOf "fiction" [ 100; 200 ])).IsValid |> shouldEqual true

[<Fact>]
let ``Objects of a cycle contain each other`` () =
    // the first book is an invalid member of the shelf, which the other books refer to
    validateModel(shelfOf "fiction" [ 100; 0; 0 ])
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Books[1].Pages" ]

type Library = {
    Shelves: Shelf list
    [<Required>]
    City: string | null
}

[<Fact>]
let ``Objects of a cycle don't wait for the objects the cycle is nested in`` () =
    // the city is not a part of the shelf, so the book is validated itself
    validateModel {
        Shelves = [ shelfOf "fiction" [ 100 ]; shelfOf "poetry" [ 0 ] ]
        City = null
    }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Shelves[1].Books[0].Pages"; "City" ]

type Reader = {
    Favourite: Book
} with
    interface IValidatableObject with
        member _.Validate(_: ValidationContext) =
            seq { yield ValidationResult("Reader is rejected") }

[<Fact>]
let ``Object of a cycle met again after the cycle is a valid or an invalid member`` () =
    let read = shelfOf "fiction" [ 100 ]
    validateModel [ box read; box { Favourite = read.Books[0] } ]
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "[1]" ]

    // the reader is not validated: the book is invalid
    let unread = shelfOf "fiction" [ 0 ]
    validateModel [ box unread; box { Favourite = unread.Books[0] } ]
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "[0].Books[0].Pages" ]

/// A node of any graph: the links are walked before the name is checked, the node itself is invalid when it has a flaw.
type Knot(name: string | null, flaw: string | null) =
    member val Links = ResizeArray<Knot>() with get
    [<Required>]
    member val Name = name with get, set
    interface IValidatableObject with
        member _.Validate(_: ValidationContext) =
            seq {
                match flaw with
                | null -> ()
                | flaw -> yield ValidationResult(flaw)
            }

let private flawsOf (model: Knot) =
    match validateModel model with
    | ModelValidationResult.Valid _ -> []
    | ModelValidationResult.Invalid(_, errors) -> [
        for error in errors.All do
            String.concat "," error.MemberNames + ": " + string error.ErrorMessage
      ]

[<Fact>]
let ``Every object of a cycle is validated itself after the whole cycle`` () =
    // a -> b -> c -> a: the name of a is checked last
    let cycle (name: string | null) =
        let a, b, c = Knot(name, "a"), Knot("b", "b"), Knot("c", "c")
        a.Links.Add b
        b.Links.Add c
        c.Links.Add a
        a
    flawsOf(cycle null) |> shouldEqual [ "Name: The Name field is required." ]
    // nested objects come first, and the objects containing an invalid one are not validated themselves
    flawsOf(cycle "a") |> shouldEqual [ "Links[0].Links[0]: c" ]

[<Fact>]
let ``Cycle nested in another object is validated on its own`` () =
    // a -> b -> c -> b: a is not a part of the cycle, so its name doesn't matter for b and c
    let a, b, c = Knot(null, "a"), Knot("b", null), Knot("c", "c")
    a.Links.Add b
    b.Links.Add c
    c.Links.Add b
    flawsOf a
    |> shouldEqual [ "Links[0].Links[0]: c"; "Name: The Name field is required." ]

[<Fact>]
let ``Cycle nested in another cycle waits for the outer one`` () =
    // a -> b -> c -> b and c -> a: all three contain each other
    let nested (name: string | null) =
        let a, b, c = Knot(name, null), Knot("b", "b"), Knot("c", "c")
        a.Links.Add b
        b.Links.Add c
        c.Links.Add b
        c.Links.Add a
        a
    flawsOf(nested null) |> shouldEqual [ "Name: The Name field is required." ]
    flawsOf(nested "a") |> shouldEqual [ "Links[0].Links[0]: c" ]

[<Fact>]
let ``Object referring to a waiting object of a cycle is a part of that cycle`` () =
    // a -> b -> a, then a -> c -> b: c contains a through b, which is waiting for a
    let joined (name: string | null) (flawOfB: string | null) =
        let a, b, c = Knot(name, "a"), Knot("b", flawOfB), Knot("c", "c")
        a.Links.Add b
        b.Links.Add a
        a.Links.Add c
        c.Links.Add b
        a
    flawsOf(joined null null) |> shouldEqual [ "Name: The Name field is required." ]
    flawsOf(joined "a" null) |> shouldEqual [ "Links[1]: c" ]
    flawsOf(joined "a" "b") |> shouldEqual [ "Links[0]: b" ]

[<Fact>]
let ``Objects of a walked cycle are finished`` () =
    // a -> b -> c -> b, then a -> d -> c: the cycle of b and c is over when d refers to c
    let after (flawOfC: string | null) =
        let a, b, c, d = Knot("a", null), Knot("b", null), Knot("c", flawOfC), Knot("d", "d")
        a.Links.Add b
        b.Links.Add c
        c.Links.Add b
        a.Links.Add d
        d.Links.Add c
        a
    flawsOf(after null) |> shouldEqual [ "Links[1]: d" ]
    flawsOf(after "c") |> shouldEqual [ "Links[0].Links[0]: c" ]

/// A struct is a part of a cycle when it refers to an object containing it.
[<Struct>]
type Stamp = {
    Owner: Shelf
} with
    interface IValidatableObject with
        member _.Validate(_: ValidationContext) =
            seq { yield ValidationResult("Stamp is rejected") }

type Stamped(name: string | null) =
    inherit Shelf(name)
    member val Stamp = Unchecked.defaultof<Stamp> with get, set
    [<Required>]
    member val Room = name with get, set

[<Fact>]
let ``Struct referring back to the object it is nested in is validated itself after the rest of that object`` () =
    let invalid = Stamped(null)
    invalid.Stamp <- { Owner = invalid }
    validateModel invalid
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Room"; "Name" ]

    let valid = Stamped("fiction")
    valid.Stamp <- { Owner = valid }
    validateModel valid |> errorsOf |> memberNames |> shouldEqual [ "Stamp" ]

type AlwaysInvalidAttribute() =
    inherit ValidationAttribute("Always invalid")
    override _.IsValid(_: obj | null) = false

[<AlwaysInvalid>]
type Flagged = { Number: int }

type FlaggedHolder = { Flagged: Flagged }

[<Fact>]
let ``Type level validation attributes of nested objects are validated`` () =
    let rootErrors = validateModel { Number = 1 } |> errorsOf
    rootErrors.All |> shouldHaveLength 1
    memberNames rootErrors |> shouldBeEmpty

    let errors = validateModel { Flagged = { Number = 1 } } |> errorsOf
    errors |> messagesFor "Flagged" |> shouldEqual [ "Always invalid" ]

/// Holds the validation attributes of Account.
type AccountRules() =
    [<Required>]
    [<Display(Name = "User name")>]
    member val Handle: string | null = null with get, set

[<MetadataType(typeof<AccountRules>)>]
type Account = { Handle: string | null }

[<AlwaysInvalid>]
type ArchiveRules() = class end

[<MetadataType(typeof<ArchiveRules>)>]
type Archive = { Accounts: Account list }

[<Fact>]
let ``Validation attributes supplied through TypeDescriptor are applied`` () =
    // the way Validator learns about a metadata type: the provider adds its attributes to the ones of the model
    TypeDescriptor.AddProvider(AssociatedMetadataTypeTypeDescriptionProvider(typeof<Account>), typeof<Account>)
    TypeDescriptor.AddProvider(AssociatedMetadataTypeTypeDescriptionProvider(typeof<Archive>), typeof<Archive>)

    validateModel { Handle = null }
    |> errorsOf
    |> messagesFor "Handle"
    |> shouldEqual [ "The User name field is required." ]

    validateModel {
        Accounts = [ { Handle = "ann" }; { Handle = null } ]
    }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Accounts[1].Handle" ]

    let errors = validateModel { Accounts = [ { Handle = "ann" } ] } |> errorsOf
    errors.All
    |> Seq.map _.ErrorMessage
    |> Seq.toList
    |> shouldEqual [ "Always invalid" ]

/// A model described by its own TypeDescriptionProvider: the answers are its properties for TypeDescriptor.
type Survey() =
    member val Answers = Dictionary<string, string>() with get
    /// Not used: the title is one of the answers.
    member _.Title: string | null = null

/// A property whose value is one of the answers.
type AnswerProperty(name: string) =
    inherit PropertyDescriptor(name, [| RequiredAttribute() :> Attribute |])
    override _.ComponentType = typeof<Survey>
    override _.PropertyType = typeof<string>
    override _.IsReadOnly = true
    override _.CanResetValue(_: obj) = false
    override _.ResetValue(_: obj) = ()
    override _.SetValue(_: obj | null, _: obj | null) = ()
    override _.ShouldSerializeValue(_: obj) = false
    override _.GetValue(survey: obj | null) : obj | null =
        match survey with
        | :? Survey as survey ->
            match survey.Answers.TryGetValue name with
            | true, answer -> answer
            | _ -> null
        | _ -> null

/// Title stands for the property of the survey but reads an answer, Email exists for TypeDescriptor only.
type SurveyDescriptor(parent: ICustomTypeDescriptor | null) =
    inherit CustomTypeDescriptor(parent)
    override _.GetProperties() =
        PropertyDescriptorCollection [| AnswerProperty "Title" :> PropertyDescriptor; AnswerProperty "Email" |]
    override this.GetProperties(_: Attribute[] | null) = this.GetProperties()

type SurveyProvider() =
    inherit TypeDescriptionProvider(TypeDescriptor.GetProvider typeof<Survey>)
    override _.GetTypeDescriptor(objectType: Type, instance: obj | null) : ICustomTypeDescriptor | null =
        SurveyDescriptor(base.GetTypeDescriptor(objectType, instance))

[<Fact>]
let ``Properties and their values are the ones TypeDescriptor has`` () =
    TypeDescriptor.AddProvider(SurveyProvider(), typeof<Survey>)
    let survey = Survey()
    survey.Answers["Title"] <- "Feedback"
    let viaValidator = ResizeArray<ValidationResult>()
    Validator.TryValidateObject(survey, ValidationContext(survey), viaValidator, true)
    |> shouldEqual false

    // the title is among the answers, the email is not
    let errors = validateModel survey |> errorsOf
    memberNames errors |> shouldEqual [ "Email" ]
    memberNames errors
    |> shouldEqual(viaValidator |> Seq.collect _.MemberNames |> Seq.toList)

    survey.Answers["Email"] <- "ann@example.org"
    (validateModel survey).IsValid |> shouldEqual true

type Draft() =
    [<Required>]
    member val Mark: string | null = null with get, set

type Revision() =
    inherit Draft()
    [<Range(1, 9)>]
    member val Mark = 0 with get, set

[<Fact>]
let ``Property hidden by a derived class is not validated`` () =
    let errors = validateModel(Revision()) |> errorsOf
    errors
    |> messagesFor "Mark"
    |> shouldEqual [ "The field Mark must be between 1 and 9." ]

type Line = {
    Quantity: int
} with
    interface IValidatableObject with
        member this.Validate(ctx: ValidationContext) =
            let limit = ctx.GetRequiredService<FooService>().Value
            let tenant = ctx.Items["tenant"]

            seq {
                if this.Quantity > limit then
                    yield ValidationResult($"Quantity is above {limit} for {tenant}", [ nameof this.Quantity ])
            }

type Cart = { Lines: Line list }

[<Fact>]
let ``Nested objects get services and items of the ValidationContext`` () =
    let model = {
        Lines = [ { Quantity = 1 }; { Quantity = 50 } ]
    }
    let services = ServiceCollection().AddSingleton<FooService>(FooService(10)).BuildServiceProvider()
    let items = Dictionary<obj, obj | null>()
    items["tenant"] <- "acme"

    let errors = validateModelWith (ValidationContext(model, services, items)) model |> errorsOf

    memberNames errors |> shouldEqual [ "Lines[1].Quantity" ]
    errors
    |> messagesFor "Lines[1].Quantity"
    |> shouldEqual [ "Quantity is above 10 for acme" ]

type Import = {
    [<Required>]
    [<SkipNestedValidation>]
    Imported: Address
    Reviewed: Address
}

[<Fact>]
let ``SkipNestedValidation excludes the property value but not the property itself`` () =
    validateModel {
        Imported = invalidAddress 1
        Reviewed = invalidAddress 2
    }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Reviewed.Street"; "Reviewed.Zip" ]

    validateModel {
        Imported = Unchecked.defaultof<Address>
        Reviewed = validAddress
    }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Imported" ]

[<Fact>]
let ``ValidationContext of another object is rejected`` () =
    let validationContext = ValidationContext(validAddress)
    (fun () -> validateModelWith validationContext (invalidAddress 1) |> ignore)
    |> shouldFail<ArgumentException>

[<Fact>]
let ``ValidationContext of a value type model has to hold an equal value`` () =
    let point: Point = { X = 50 }
    validateModelWith (ValidationContext(point)) point
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "X" ]

    // the context is created for a valid point, the model is another one
    (fun () -> validateModelWith (ValidationContext({ X = 5 }: Point)) point |> ignore)
    |> shouldFail<ArgumentException>

/// Badges are equal when their serials are, whatever the ranks.
[<Struct; CustomEquality; NoComparison>]
type Badge = {
    Serial: int
    [<Range(1, 10)>]
    Rank: int
} with
    override this.Equals(other: obj) =
        match other with
        | :? Badge as badge -> badge.Serial = this.Serial
        | _ -> false
    override this.GetHashCode() = this.Serial

[<Fact>]
let ``Value type model is validated itself, not the equal value of its ValidationContext`` () =
    let validationContext = ValidationContext({ Serial = 1; Rank = 5 })
    validateModelWith validationContext { Serial = 1; Rank = 50 }
    |> errorsOf
    |> memberNames
    |> shouldEqual [ "Rank" ]

[<Struct>]
type Voucher = {
    Amount: int
} with
    interface IValidatableObject with
        member _.Validate(ctx: ValidationContext) =
            let limit = ctx.GetRequiredService<FooService>().Value
            let tenant = ctx.Items["tenant"]
            seq {
                yield ValidationResult($"{ctx.DisplayName} is above {limit} for {tenant}", [ string ctx.MemberName ])
            }

[<Fact>]
let ``Value type model is validated with the settings of its ValidationContext`` () =
    let voucher = { Amount = 50 }
    let services = ServiceCollection().AddSingleton<FooService>(FooService(10)).BuildServiceProvider()
    let items = Dictionary<obj, obj | null>()
    items["tenant"] <- "acme"
    let validationContext = ValidationContext(voucher, services, items)
    validationContext.DisplayName <- "Gift voucher"
    validationContext.MemberName <- "Gift"

    validateModelWith validationContext voucher
    |> errorsOf
    |> messagesFor "Gift"
    |> shouldEqual [ "Gift voucher is above 10 for acme" ]

type RouteForm = {
    [<Required>]
    Driver: string | null
    Waypoints: Address[]
}

[<Fact>]
let ``BindAndValidateForm reports nested errors under the names of form fields`` () =
    task {
        let ctx = DefaultHttpContext()
        ctx.RequestServices <- ServiceCollection().AddSingleton<IModelBinder>(ModelBinder()).BuildServiceProvider()
        ctx.Request.ContentType <- "application/x-www-form-urlencoded"
        ctx.Request.Body <-
            new MemoryStream(
                Encoding.UTF8.GetBytes
                    "Waypoints[0].Street=Main+street&Waypoints[0].Zip=12345&Waypoints[1].Street=x&Waypoints[1].Zip=7"
            )

        let! result = ctx.BindAndValidateForm<RouteForm>()

        let errors = errorsOf result
        memberNames errors |> shouldEqual [ "Driver"; "Waypoints[1].Street" ]
        errors |> messagesFor "Waypoints[1].Street" |> shouldEqual [ streetError ]
    }
