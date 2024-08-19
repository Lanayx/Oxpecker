module Oxpecker.Tests.ModelParser

open System
open System.Globalization
open Microsoft.Extensions.Primitives
open Oxpecker
open Xunit
open FsUnitTyped

type Sex =
    | Male
    | Female

[<CLIMutable>]
type Child = { Name: string; Age: int }

[<CLIMutable>]
type Model = {
    Id: Guid
    FirstName: string
    MiddleName: string option
    LastName: string
    Sex: Sex
    BirthDate: DateTime
    Nicknames: string list option
    Children: Child[]
}


[<CLIMutable>]
type Model2 = { SearchTerms: string[] }

[<CLIMutable>]
type CompositeModel = {
    FirstChild: Child
    SecondChild: Child option
}

[<Fact>]
let ``ModelParser.parse with model which has primitive array`` () =
    let modelData = dict [ "SearchTerms", StringValues [| "a"; "abc"; "abcdef" |] ]
    let expected = {
        SearchTerms = [| "a"; "abc"; "abcdef" |]
    }
    let instance = Activator.CreateInstance<Model2>()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel instance culture modelData
    result |> shouldEqual(Ok expected)


[<Fact>]
let ``ModelParser.parse with complete model data`` () =
    let id = Guid.NewGuid()
    let modelData =
        dict [
            "Id", StringValues(id.ToString())
            "FirstName", StringValues "Susan"
            "MiddleName", StringValues "Elisabeth"
            "LastName", StringValues "Doe"
            "Sex", StringValues "Female"
            "BirthDate", StringValues "1986-12-29"
            "Nicknames", StringValues [| "Susi"; "Eli"; "Liz" |]
            "Children[0].Name", StringValues "Hamed"
            "Children[0].Age", StringValues "32"
            "Children[1].Name", StringValues "Ali"
            "Children[1].Age", StringValues "22"
            "Children[2].Name", StringValues "Gholi"
            "Children[2].Age", StringValues "44"
        ]
    let expected = {
        Id = id
        FirstName = "Susan"
        MiddleName = Some "Elisabeth"
        LastName = "Doe"
        Sex = Female
        BirthDate = DateTime(1986, 12, 29)
        Nicknames = Some [ "Susi"; "Eli"; "Liz" ]
        Children = [|
            { Name = "Hamed"; Age = 32 }
            { Name = "Ali"; Age = 22 }
            { Name = "Gholi"; Age = 44 }
        |]
    }
    let instance = Activator.CreateInstance<Model>()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel instance culture modelData
    result |> shouldEqual(Ok expected)


[<Fact>]
let ``ModelParser.parse with model data without optional parameters`` () =
    let id = Guid.NewGuid()
    let modelData =
        dict [
            "Id", StringValues(id.ToString())
            "FirstName", StringValues "Susan"
            "LastName", StringValues "Doe"
            "Sex", StringValues "Female"
            "BirthDate", StringValues "1986-12-29"
            "Children[0].Name", StringValues "Hamed"
            "Children[0].Age", StringValues "32"
            "Children[1].Name", StringValues "Ali"
            "Children[1].Age", StringValues "22"
            "Children[2].Name", StringValues "Gholi"
            "Children[2].Age", StringValues "44"
        ]
    let expected = {
        Id = id
        FirstName = "Susan"
        MiddleName = None
        LastName = "Doe"
        Sex = Female
        BirthDate = DateTime(1986, 12, 29)
        Nicknames = None
        Children = [|
            { Name = "Hamed"; Age = 32 }
            { Name = "Ali"; Age = 22 }
            { Name = "Gholi"; Age = 44 }
        |]
    }
    let instance = Activator.CreateInstance<Model>()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel instance culture modelData
    result |> shouldEqual(Ok expected)

[<Fact>]
let ``ModelParser.parse with missing array items`` () =
    let id = Guid.NewGuid()
    let modelData =
        dict [
            "Id", StringValues(id.ToString())
            "FirstName", StringValues "Susan"
            "LastName", StringValues "Doe"
            "Sex", StringValues "Female"
            "BirthDate", StringValues "1986-12-29"
            "Children[0].Name", StringValues "Hamed"
            "Children[0].Age", StringValues "32"
            "Children[2].Name", StringValues "Gholi"
            "Children[2].Age", StringValues "44"
        ]
    let expected = {
        Id = id
        FirstName = "Susan"
        MiddleName = None
        LastName = "Doe"
        Sex = Female
        BirthDate = DateTime(1986, 12, 29)
        Nicknames = None
        Children = [|
            { Name = "Hamed"; Age = 32 }
            Unchecked.defaultof<_>
            { Name = "Gholi"; Age = 44 }
        |]
    }
    let instance = Activator.CreateInstance<Model>()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel instance culture modelData
    result |> shouldEqual(Ok expected)

[<Fact>]
let ``ModelParser.parse with complete model data but with different order for array of child`` () =
    let id = Guid.NewGuid()
    let modelData =
        dict [
            "Id", StringValues(id.ToString())
            "FirstName", StringValues "Susan"
            "MiddleName", StringValues "Elisabeth"
            "LastName", StringValues "Doe"
            "Sex", StringValues "female"
            "BirthDate", StringValues "1986-12-29"
            "Nicknames", StringValues [| "Susi"; "Eli"; "Liz" |]
            "Children[2].Name", StringValues "Gholi"
            "Children[0].Name", StringValues "Hamed"
            "Children[1].Age", StringValues "22"
            "Children[2].Age", StringValues "44"
            "Children[1].Name", StringValues "Ali"
            "Children[0].Age", StringValues "32"
        ]
    let expected = {
        Id = id
        FirstName = "Susan"
        MiddleName = Some "Elisabeth"
        LastName = "Doe"
        Sex = Female
        BirthDate = DateTime(1986, 12, 29)
        Nicknames = Some [ "Susi"; "Eli"; "Liz" ]
        Children = [|
            { Name = "Hamed"; Age = 32 }
            { Name = "Ali"; Age = 22 }
            { Name = "Gholi"; Age = 44 }
        |]
    }
    let instance = Activator.CreateInstance<Model>()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel instance culture modelData
    result |> shouldEqual(Ok expected)

[<Fact>]
let ``ModelParser.parse with complete model data but wrong union case`` () =
    let id = Guid.NewGuid()
    let modelData =
        dict [
            "Id", StringValues(id.ToString())
            "FirstName", StringValues "Susan"
            "MiddleName", StringValues "Elisabeth"
            "LastName", StringValues "Doe"
            "Sex", StringValues "wrong"
            "BirthDate", StringValues "1986-12-29"
            "Nicknames", StringValues [| "Susi"; "Eli"; "Liz" |]
            "Children[0].Name", StringValues "Hamed"
            "Children[0].Age", StringValues "32"
            "Children[1].Name", StringValues "Ali"
            "Children[1].Age", StringValues "22"
            "Children[2].Name", StringValues "Gholi"
            "Children[2].Age", StringValues "44"
        ]
    let instance = Activator.CreateInstance<Model>()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel instance culture modelData
    result
    |> shouldEqual(Error "The value 'wrong' is not a valid case for type Oxpecker.Tests.ModelParser+Sex.")

[<Fact>]
let ``ModelParser.parse with complete model data but wrong data`` () =
    let id = Guid.NewGuid()
    let modelData =
        dict [
            "Id", StringValues(id.ToString())
            "FirstName", StringValues "Susan"
            "MiddleName", StringValues "Elisabeth"
            "LastName", StringValues "Doe"
            "Sex", StringValues "Female"
            "BirthDate", StringValues "wrong"
            "Nicknames", StringValues [| "Susi"; "Eli"; "Liz" |]
            "Children[0].Name", StringValues "Hamed"
            "Children[0].Age", StringValues "wrongAge"
            "Children[1].Name", StringValues "Ali"
            "Children[1].Age", StringValues "wrongAge"
            "Children[2].Name", StringValues "Gholi"
            "Children[2].Age", StringValues "wrongAge"
        ]
    let instance = Activator.CreateInstance<Model>()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel instance culture modelData
    result
    |> shouldEqual(Error "Could not parse value 'wrong' to type System.DateTime.")

// ---------------------------------
// ModelParser.parse Tests
// ---------------------------------


[<Fact>]
let ``ModelParser.parse with complete model data but mixed casing`` () =
    let id = Guid.NewGuid()
    let modelData =
        dict [
            "id", StringValues(id.ToString())
            "firstName", StringValues "Susan"
            "MiddleName", StringValues "Elisabeth"
            "lastname", StringValues "Doe"
            "Sex", StringValues "female"
            "BirthDate", StringValues "1986-12-29"
            "NickNames", StringValues [| "Susi"; "Eli"; "Liz" |]
            "Children[0].Name", StringValues "Hamed"
            "Children[0].Age", StringValues "32"
            "Children[1].name", StringValues "Ali"
            "Children[1].Age", StringValues "22"
            "Children[2].Name", StringValues "Gholi"
            "Children[2].age", StringValues "44"
        ]
    let expected = {
        Id = Guid.Empty
        FirstName = null
        MiddleName = Some "Elisabeth"
        LastName = null
        Sex = Female
        BirthDate = DateTime(1986, 12, 29)
        Nicknames = None
        Children = [|
            { Name = "Hamed"; Age = 32 }
            { Name = null; Age = 22 }
            { Name = "Gholi"; Age = 0 }
        |]
    }
    let instance = Activator.CreateInstance<Model>()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel instance culture modelData
    result |> shouldEqual(Ok expected)


[<Fact>]
let ``ModelParser.parse with incomplete model data`` () =
    let modelData =
        dict [
            "FirstName", StringValues "Susan"
            "MiddleName", StringValues "Elisabeth"
            "Sex", StringValues "Female"
            "BirthDate", StringValues "1986-12-29"
            "Nicknames", StringValues [| "Susi"; "Eli"; "Liz" |]
            "Children[0].Name", StringValues "Hamed"
            "Children[1].Age", StringValues "44"
        ]

    let expected = {
        Id = Guid.Empty
        FirstName = "Susan"
        MiddleName = Some "Elisabeth"
        LastName = null
        Sex = Female
        BirthDate = DateTime(1986, 12, 29)
        Nicknames = Some [ "Susi"; "Eli"; "Liz" ]
        Children = [| { Name = "Hamed"; Age = 0 }; { Name = null; Age = 44 } |]
    }
    let instance = Activator.CreateInstance<Model>()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel instance culture modelData
    result |> shouldEqual(Ok expected)

[<Fact>]
let ``ModelParser.parse with incomplete model data and with different order for array of child`` () =
    let modelData =
        dict [
            "FirstName", StringValues "Susan"
            "MiddleName", StringValues "Elisabeth"
            "Sex", StringValues "Female"
            "BirthDate", StringValues "1986-12-29"
            "Nicknames", StringValues [| "Susi"; "Eli"; "Liz" |]
            "Children[1].Age", StringValues "44"
            "Children[0].Name", StringValues "Hamed"
        ]

    let expected = {
        Id = Guid.Empty
        FirstName = "Susan"
        MiddleName = Some "Elisabeth"
        LastName = null
        Sex = Female
        BirthDate = DateTime(1986, 12, 29)
        Nicknames = Some [ "Susi"; "Eli"; "Liz" ]
        Children = [| { Name = "Hamed"; Age = 0 }; { Name = null; Age = 44 } |]
    }
    let instance = Activator.CreateInstance<Model>()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel instance culture modelData
    result |> shouldEqual(Ok expected)


[<Fact>]
let ``ModelParser.parse with composite model and SecondChild missing data`` () =
    let modelData =
        dict [
            "FirstChild.Name", StringValues "FirstName"
            "FirstChild.Age", StringValues "2"
        ]
    let expected = {
        FirstChild = { Name = "FirstName"; Age = 2 }
        SecondChild = None
    }
    let instance = Activator.CreateInstance<CompositeModel>()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel instance culture modelData
    result |> shouldEqual(Ok expected)

[<Fact>]
let ``ModelParser.parse with complete composite model data`` () =
    let modelData =
        dict [
            "FirstChild.Name", StringValues "FirstName"
            "FirstChild.Age", StringValues "2"
            "SecondChild.Name", StringValues "SecondName"
            "SecondChild.Age", StringValues "10"
        ]
    let expected = {
        FirstChild = { Name = "FirstName"; Age = 2 }
        SecondChild = Some { Name = "SecondName"; Age = 10 }
    }
    let instance = Activator.CreateInstance<CompositeModel>()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel instance culture modelData
    result |> shouldEqual(Ok expected)
