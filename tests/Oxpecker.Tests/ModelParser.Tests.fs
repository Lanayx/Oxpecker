module Oxpecker.Tests.ModelParser

open System
open System.Globalization
open Microsoft.Extensions.Primitives
open Oxpecker
open Xunit
open FsUnit.Xunit

type Sex =
    | Male
    | Female

type Child = { Name: string | null; Age: int }

type Model = {
    Id: Guid
    FirstName: string | null
    MiddleName: string option
    LastName: string | null
    Sex: Sex
    BirthDate: DateTime
    Nicknames: string list option
    Children: Child[]
}

type Model2 = { SearchTerms: string[] }

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
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Model2> culture modelData
    result |> should equal expected


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
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Model> culture modelData
    result |> should equal expected


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
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Model> culture modelData
    result |> should equal expected

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
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Model> culture modelData
    result |> should equal expected

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
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Model> culture modelData
    result |> should equal expected

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
    let culture = CultureInfo.InvariantCulture

    let result() = ModelParser.parseModel<Model> culture modelData |> ignore

    result |> should (throwWithMessage "Could not parse value 'wrong' to type 'Oxpecker.Tests.ModelParser+Sex'.") typeof<Exception>

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
    let culture = CultureInfo.InvariantCulture

    let result() = ModelParser.parseModel<Model> culture modelData |> ignore

    result
    |> should (throwWithMessage "Could not parse value 'wrong' to type 'System.DateTime'.") typeof<Exception>

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
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Model> culture modelData
    result |> should equal expected


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
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Model> culture modelData
    result |> should equal expected

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
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Model> culture modelData
    result |> should equal expected


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
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<CompositeModel> culture modelData
    result |> should equal expected

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
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<CompositeModel> culture modelData

    result |> should equal expected

[<Fact>]
let ``parseModel<string> parses null`` () =
    let values =  Unchecked.defaultof<string> |> String.toDict
    let expected = Unchecked.defaultof<string>
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<string> culture values

    result |> should equal expected

[<Fact>]
let ``parseModel<string> parses empty string`` () =
    let values = String.Empty |> String.toDict
    let expected = String.Empty
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<string> culture values

    result |> should equal expected

[<Fact>]
let ``parseModel<float> fails to parse 'some-value'`` () =
    let values = "some-value" |> String.toDict
    let expected = "Could not parse value 'some-value' to type 'System.Double'."
    let culture = CultureInfo.InvariantCulture

    let result() = ModelParser.parseModel<float> culture values |> ignore

    result |> should (throwWithMessage expected) typeof<Exception>

[<Fact>]
let ``parseModel<int> fails to parse 'some-value'`` () =
    let values = "some-value" |> String.toDict
    let expected = "Could not parse value 'some-value' to type 'System.Int32'."
    let culture = CultureInfo.InvariantCulture

    let result() = ModelParser.parseModel<int> culture values |> ignore

    result |> should (throwWithMessage expected) typeof<Exception>

[<Fact>]
let ``parseModel<int64> fails to parse null`` () =
    let values = Unchecked.defaultof<string> |> String.toDict
    let expected = "Could not parse value 'null' to type 'System.Int64'."
    let culture = CultureInfo.InvariantCulture

    let result() = ModelParser.parseModel<int64> culture values |> ignore

    result |> should (throwWithMessage expected) typeof<Exception>

[<Fact>]
let ``parseModel<Nullable<int>> parses null`` () =
    let values = Unchecked.defaultof<string> |> String.toDict
    let expected = Nullable()
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Nullable<int>> culture values

    result |> should equal expected

[<Fact>]
let ``parseModel<Nullable<int>> parses 1`` () =
    let values = "1" |> String.toDict
    let expected = Nullable 1
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Nullable<int>> culture values

    result |> should equal expected

[<Fact>]
let ``parseModel<decimal option> parses null`` () =
    let values = Unchecked.defaultof<string> |> String.toDict
    let expected = None
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<decimal option> culture values

    result |> should equal expected

[<Fact>]
let ``parseModel<decimal option> parses 100`` () =
    let values = "100" |> String.toDict
    let expected = Some 100M
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<decimal option> culture values

    result |> should equal expected

[<Fact>]
let ``parseModel<string option> parses null`` () =
    let values = Unchecked.defaultof<string> |> String.toDict
    let expected = None
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<string option> culture values

    result |> should equal expected

[<Fact>]
let ``parseModel<string option> parses empty string`` () =
    let values = String.Empty |> String.toDict
    let expected = Some String.Empty
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<string option> culture values

    result |> should equal expected

[<Fact>]
let ``parseModel<string option> parses 'some-value'`` () =
    let values = "some-value" |> String.toDict
    let expected = Some "some-value"
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<string option> culture values

    result |> should equal expected

[<Fact>]
let ``parseModel<Sex option> parses 'Female'`` () =
    let values = "Female" |> String.toDict
    let expected = Some Female
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Sex option> culture values

    result |> should equal expected

[<Fact>]
let ``parseModel<Sex array> parses the array containing null`` () =
    let xs: (string | null) array | null = [| "Female"; null; "Male"; "Female"; "Female"; "Male" |]
    let values = xs |> StringValues |> StringValues.toDict
    let expected: Sex array = [| Female; Unchecked.defaultof<_>; Male; Female; Female; Male |]
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Sex array> culture values

    result |> should equal expected

[<Fact>]
let ``parseModel<Sex option array> parses the array containing null`` () =
    let xs: (string | null) array | null = [| "Female"; null; "Male"; "Female"; "Female"; "Male" |]
    let values = xs |> StringValues |> StringValues.toDict
    let expected = [| Some Female; None; Some Male; Some Female; Some Female; Some Male |]
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Sex option array> culture values

    result |> should equal expected

type [<Struct>] Direction = Left | Right

[<Fact>]
let ``parseModel<Direction Nullable> parses 'Right'`` () =
    let values = "right" |> String.toDict
    let expected = Nullable Right
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Nullable<Direction>> culture values

    result |> should equal expected

type BookType = Unknown = 0 | Hardcover = 1 | Paperback = 2 | EBook = 3

[<Fact>]
let ``parseModel<BookType> parses 'PaperBack'`` () =
    let modelData =
        dict [
            "", StringValues "Paperback"
        ]
    let expected = BookType.Paperback
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<BookType> culture modelData

    result |> should equal expected

[<Fact>]
let ``parseModel<BookType> parses '3'`` () =
    let modelData = "3" |> String.toDict
    let expected = BookType.EBook
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<BookType> culture modelData

    result |> should equal expected

[<Fact>]
let ``parseModel<BookType> parses '100'`` () =
    let modelData = "100" |> String.toDict
    let expected = enum<BookType> 100
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<BookType> culture modelData

    result |> should equal expected

[<Fact>]
let ``parseModel<BookType> fails to parse null`` () =
    let values = Unchecked.defaultof<string> |> String.toDict
    let expected = "Could not parse value 'null' to type 'Oxpecker.Tests.ModelParser+BookType'."
    let culture = CultureInfo.InvariantCulture

    let result() = ModelParser.parseModel<BookType> culture values |> ignore

    result |> should (throwWithMessage expected) typeof<Exception>

type Baz = { Name: string option; Value: int Nullable }

[<NoEquality; NoComparison>]
type Bar = { Bar: string | null; Baz: Baz | null }

type Foo = { Foo: string; Bars: Bar option array }

[<Fact>]
let ``parseModel<Foo> parses the data with no seqential index elements`` () =
    let modelData = dict [
        "Bars[2].Bar", StringValues "Bar"
        "Bars[0].Baz.Name", StringValues "abc"
        "Bars[0].Baz.Value", StringValues "0"
        "Bars[2].Baz.Value", StringValues "1"
    ]
    let expected = {
        Foo = Unchecked.defaultof<_>
        Bars = [|
            Some { Bar = null; Baz = { Name = Some "abc"; Value = Nullable 0 } }
            None
            Some { Bar = "Bar"; Baz = { Name = None; Value = Nullable 1 } }
        |]
    }
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Foo> culture modelData

    Assert.Equivalent(expected, result)

[<Fact>]
let ``parseModel<Foo> parses the data with no matched prefix`` () =
    let modelData = dict [
        "Barss[0].Baz.Value", StringValues "0"
    ]
    let expected = { Foo = Unchecked.defaultof<_>; Bars = Unchecked.defaultof<_> }
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Foo> culture modelData

    Assert.Equivalent(expected, result)

[<Fact>]
let ``parseModel<Foo> parses the data with inproper index access`` () =
    let modelData = dict [
        "Bars[0].Baz[0].Value", StringValues "0"
    ]
    let expected = {
        Foo = Unchecked.defaultof<_>
        Bars = [| Some { Bar  = null; Baz = { Name = None; Value = Nullable() } } |]
    }
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Foo> culture modelData

    Assert.Equivalent(expected, result)

[<Fact>]
let ``parseModel<Foo> parses the data with partially incorrect key`` () =
    let modelData = dict [
        "Bars[0].Test.Descr", StringValues "0"
    ]
    let expected = {
        Foo = Unchecked.defaultof<_>
        Bars = [| Some { Bar  = null; Baz = null } |]
    }
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Foo> culture modelData

    Assert.Equivalent(expected, result)

[<Fact>]
let ``parseModel<Foo> parses the data with missing index`` () =
    let modelData = dict [
        "Bars.Baz.Value", StringValues "0"
    ]
    let expected = { Foo = Unchecked.defaultof<_>; Bars = [||] }
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Foo> culture modelData

    Assert.Equivalent(expected, result)

[<Fact>]
let ``parseModel<Bar> parses the data with no matched prefix`` () =
    let modelData = dict [
        "Bazz.Value", StringValues "0"
    ]
    let expected = { Bar = null; Baz = null }
    let culture = CultureInfo.InvariantCulture

    let result = ModelParser.parseModel<Bar> culture modelData

    Assert.Equivalent(expected, result)
