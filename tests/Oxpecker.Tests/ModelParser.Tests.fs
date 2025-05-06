module Oxpecker.Tests.ModelParser

open System
open System.Collections.Generic
open System.Globalization
open FsUnitTyped
open Microsoft.Extensions.Primitives
open Oxpecker
open Xunit

let private toComplexData data =
    data |> List.map KeyValuePair.Create |> Dictionary |> RawData.initComplexData

let private defaultParseModel<'T> data =
    let culture = CultureInfo.InvariantCulture
    let ignoreCase = false
    ModelParser.parseModel<'T> culture ignoreCase data

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

[<Struct>]
type Model2 = { SearchTerms: string[] }

type CompositeModel = {
    FirstChild: Child
    SecondChild: Child option
}

[<Fact>]
let ``parseModel<Model2> returns empty array for null SearchTerms`` () =
    let modelData =
        [ "SearchTerms", StringValues Unchecked.defaultof<string> ] |> toComplexData
    let expected = { SearchTerms = [||] }
    let result = defaultParseModel<Model2> modelData
    result |> shouldEqual expected

[<Fact>]
let ``parseModel<Model2> returns empty array for null string array`` () =
    let modelData =
        [ "SearchTerms", StringValues Unchecked.defaultof<string array> ]
        |> toComplexData
    let expected = { SearchTerms = [||] }
    let result = defaultParseModel<Model2> modelData
    result |> shouldEqual expected

[<Fact>]
let ``parseModel<Model2> returns empty array for empty string array`` () =
    let modelData = [ "SearchTerms", StringValues [||] ] |> toComplexData
    let expected = { SearchTerms = [||] }
    let result = defaultParseModel<Model2> modelData
    result |> shouldEqual expected

[<Fact>]
let ``parseModel<Model2> handles array with null element`` () =
    let modelData = [ "SearchTerms", StringValues [| null |] ] |> toComplexData
    let expected = {
        SearchTerms = [| Unchecked.defaultof<_> |]
    }
    let result = defaultParseModel<Model2> modelData
    result |> shouldEqual expected

[<Fact>]
let ``parseModel<Model2> converts single string to single-element array`` () =
    let modelData = [ "SearchTerms", StringValues "a" ] |> toComplexData
    let expected = { SearchTerms = [| "a" |] }
    let result = defaultParseModel<Model2> modelData
    result |> shouldEqual expected

[<Fact>]
let ``parseModel<Model2> handles single-element string array`` () =
    let modelData = [ "SearchTerms", StringValues [| "a" |] ] |> toComplexData
    let expected = { SearchTerms = [| "a" |] }
    let result = defaultParseModel<Model2> modelData
    result |> shouldEqual expected

[<Fact>]
let ``parseModel<Model2> handles multi-element string array`` () =
    let modelData =
        [ "SearchTerms", StringValues [| "a"; "abc"; "abcdef" |] ] |> toComplexData
    let expected = {
        SearchTerms = [| "a"; "abc"; "abcdef" |]
    }
    let result = defaultParseModel<Model2> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<Model> parses complete model data correctly`` () =
    let id = Guid.NewGuid()
    let modelData =
        [
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
        |> toComplexData
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
    let result = defaultParseModel<Model> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<Model> handles missing optional parameters`` () =
    let id = Guid.NewGuid()
    let modelData =
        [
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
        |> toComplexData
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
    let result = defaultParseModel<Model> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<Model> handles missing array items`` () =
    let id = Guid.NewGuid()
    let modelData =
        [
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
        |> toComplexData
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
    let result = defaultParseModel<Model> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<Model> correctly handles unordered array items`` () =
    let id = Guid.NewGuid()
    let modelData =
        [
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
        |> toComplexData
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
    let result = defaultParseModel<Model> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<Model> fails when union case is invalid`` () =
    let id = Guid.NewGuid()
    let modelData =
        [
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
        |> toComplexData
    let result () =
        defaultParseModel<Model> modelData |> ignore
    result
    |> shouldFailWithMessage<NotParsedException>
        "Could not parse value 'wrong' to type 'Oxpecker.Tests.ModelParser+Sex'."

[<Fact>]
let ``defaultParseModel<Model> fails when data contains invalid values`` () =
    let id = Guid.NewGuid()
    let modelData =
        [
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
        |> toComplexData
    let result () =
        defaultParseModel<Model> modelData |> ignore
    result
    |> shouldFailWithMessage<NotParsedException> "Could not parse value 'wrong' to type 'System.DateTime'."

[<Fact>]
let ``defaultParseModel<Model> handles mixed casing in keys`` () =
    let id = Guid.NewGuid()
    let modelData =
        [
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
        |> toComplexData
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
    let ignoreCase = true
    let result = ModelParser.parseModel<Model> culture ignoreCase modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<Model> handles incomplete model data`` () =
    let modelData =
        [
            "FirstName", StringValues "Susan"
            "middlename", StringValues "Elisabeth" // wrong case
            "Sex", StringValues "Female"
            "BirthDate", StringValues "1986-12-29"
            "Nicknames", StringValues [| "Susi"; "Eli"; "Liz" |]
            "Children[0].Name", StringValues "Hamed"
            "Children[1].Age", StringValues "44"
        ]
        |> toComplexData
    let expected = {
        Id = Guid.Empty
        FirstName = "Susan"
        MiddleName = None
        LastName = null
        Sex = Female
        BirthDate = DateTime(1986, 12, 29)
        Nicknames = Some [ "Susi"; "Eli"; "Liz" ]
        Children = [| { Name = "Hamed"; Age = 0 }; { Name = null; Age = 44 } |]
    }
    let result = defaultParseModel<Model> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<Model> handles incomplete model data with unordered child array`` () =
    let modelData =
        [
            "FirstName", StringValues "Susan"
            "MiddleName", StringValues "Elisabeth"
            "Sex", StringValues "Female"
            "BirthDate", StringValues "1986-12-29"
            "Nicknames", StringValues [| "Susi"; "Eli"; "Liz" |]
            "Children[1].Age", StringValues "44"
            "Children[0].Name", StringValues "Hamed"
        ]
        |> toComplexData
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
    let result = defaultParseModel<Model> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<CompositeModel> handles missing SecondChild data`` () =
    let modelData =
        [
            "FirstChild.Name", StringValues "FirstName"
            "FirstChild.Age", StringValues "2"
        ]
        |> toComplexData
    let expected = {
        FirstChild = { Name = "FirstName"; Age = 2 }
        SecondChild = None
    }
    let result = defaultParseModel<CompositeModel> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<CompositeModel> parses complete composite model data`` () =
    let modelData =
        [
            "FirstChild.Name", StringValues "FirstName"
            "FirstChild.Age", StringValues "2"
            "SecondChild.Name", StringValues "SecondName"
            "SecondChild.Age", StringValues "10"
        ]
        |> toComplexData
    let expected = {
        FirstChild = { Name = "FirstName"; Age = 2 }
        SecondChild = Some { Name = "SecondName"; Age = 10 }
    }
    let result = defaultParseModel<CompositeModel> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<string> parses null value`` () =
    let data = Unchecked.defaultof<string> |> StringValues |> SimpleData
    let expected = Unchecked.defaultof<string>
    let result = defaultParseModel<string> data
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<string> parses empty string value`` () =
    let data = String.Empty |> StringValues |> SimpleData
    let expected = String.Empty
    let result = defaultParseModel<string> data

    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<float> fails to parse invalid string value`` () =
    let expected = "Could not parse value 'some-value' to type 'System.Double'."
    let data = "some-value" |> StringValues |> SimpleData
    let result () = defaultParseModel<float> data |> ignore
    result |> shouldFailWithMessage<NotParsedException> expected

[<Fact>]
let ``defaultParseModel<int> fails to parse invalid string value`` () =
    let expected = "Could not parse value 'some-value' to type 'System.Int32'."
    let data = "some-value" |> StringValues |> SimpleData
    let result () = defaultParseModel<int> data |> ignore
    result |> shouldFailWithMessage<NotParsedException> expected

[<Fact>]
let ``defaultParseModel<int64> fails to parse null value`` () =
    let data = Unchecked.defaultof<string> |> StringValues |> SimpleData
    let expected = "Could not parse value '<null>' to type 'System.Int64'."
    let result () = defaultParseModel<int64> data |> ignore
    result |> shouldFailWithMessage<NotParsedException> expected

[<Fact>]
let ``defaultParseModel<Nullable<int>> parses null value`` () =
    let data = Unchecked.defaultof<string> |> StringValues |> SimpleData
    let expected = Nullable()
    let result = defaultParseModel<Nullable<int>> data
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<Nullable<int>> parses a valid integer value`` () =
    let data = "1" |> StringValues |> SimpleData
    let expected = Nullable 1
    let result = defaultParseModel<Nullable<int>> data
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<decimal option> parses null value`` () =
    let data = Unchecked.defaultof<string> |> StringValues |> SimpleData
    let expected = None
    let result = defaultParseModel<decimal option> data
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<decimal option> parses a valid decimal value`` () =
    let data = "100" |> StringValues |> SimpleData
    let expected = Some 100M
    let result = defaultParseModel<decimal option> data
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<string option> parses null value`` () =
    let data = Unchecked.defaultof<string> |> StringValues |> SimpleData
    let expected = None
    let result = defaultParseModel<string option> data
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<string option> parses an empty string value`` () =
    let data = String.Empty |> StringValues |> SimpleData
    let expected = Some String.Empty
    let result = defaultParseModel<string option> data
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<string option> parses a valid string value`` () =
    let data = "some-value" |> StringValues |> SimpleData
    let expected = Some "some-value"
    let result = defaultParseModel<string option> data
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<Sex option> parses a valid union case 'Female'`` () =
    let data = "Female" |> StringValues |> SimpleData
    let expected = Some Female
    let result = defaultParseModel<Sex option> data
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<Sex array> parses an array containing null values`` () =
    let xs: (string | null) array = [| "Female"; null; "Male"; "Female"; "Female"; "Male" |]
    let data = xs |> StringValues |> SimpleData
    let expected: Sex array = [| Female; Unchecked.defaultof<_>; Male; Female; Female; Male |]
    let result = defaultParseModel<Sex array> data
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<bool array> parses an array with valid data`` () =
    let xs: (string | null) array = [| "true"; "false"; "True"; "falsE"; "TRUE"; "FALSE" |]
    let data = xs |> StringValues |> SimpleData
    let expected: bool array = [| true; false; true; false; true; false |]
    let result = defaultParseModel<bool array> data
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<Sex option array> parses an array containing null values`` () =
    let xs: (string | null) array = [| "Female"; null; "Male"; "Female"; "Female"; "Male" |]
    let data = xs |> StringValues |> SimpleData
    let expected = [| Some Female; None; Some Male; Some Female; Some Female; Some Male |]
    let result = defaultParseModel<Sex option array> data
    result |> shouldEqual expected

[<Struct>]
type Direction =
    | Left
    | Right

[<Fact>]
let ``defaultParseModel<Nullable<Direction>> parses a valid direction 'Right'`` () =
    let data = "right" |> StringValues |> SimpleData
    let expected = Nullable Right
    let result = defaultParseModel<Nullable<Direction>> data
    result |> shouldEqual expected

type BookType =
    | Unknown = 0
    | Hardcover = 1
    | Paperback = 2
    | EBook = 3

[<Fact>]
let ``defaultParseModel<BookType> parses a valid enum value 'Paperback'`` () =
    let modelData = "Paperback" |> StringValues |> SimpleData
    let expected = BookType.Paperback
    let result = defaultParseModel<BookType> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<BookType> parses a valid numeric value '3'`` () =
    let modelData = "3" |> StringValues |> SimpleData
    let expected = BookType.EBook
    let result = defaultParseModel<BookType> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<BookType> parses an out-of-range numeric value '100'`` () =
    let modelData = "100" |> StringValues |> SimpleData
    let expected = enum<BookType> 100
    let result = defaultParseModel<BookType> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<BookType> fails to parse null value`` () =
    let data = Unchecked.defaultof<string> |> StringValues |> SimpleData
    let expected =
        "Could not parse value '<null>' to type 'Oxpecker.Tests.ModelParser+BookType'."
    let result () =
        defaultParseModel<BookType> data |> ignore
    result |> shouldFailWithMessage<NotParsedException> expected

[<Fact>]
let ``defaultParseModel<ResizeArray<BookType>> parses a collection of enum values`` () =
    let data =
        [| "3"; "Hardcover"; "Paperback"; "100"; "0" |] |> StringValues |> SimpleData
    let expected =
        ResizeArray [
            BookType.EBook
            BookType.Hardcover
            BookType.Paperback
            enum<BookType> 100
            BookType.Unknown
        ]
    let result = defaultParseModel<ResizeArray<BookType>> data
    result |> shouldEqualSeq expected

[<Fact>]
let ``defaultParseModel<BookType list> parses a list of enum values`` () =
    let data =
        [| "3"; "Hardcover"; "Paperback"; "100"; "0" |] |> StringValues |> SimpleData
    let expected = [
        BookType.EBook
        BookType.Hardcover
        BookType.Paperback
        enum<BookType> 100
        BookType.Unknown
    ]
    let result = defaultParseModel<BookType list> data
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<BookType seq> parses a sequence of enum values`` () =
    let data =
        [| "3"; "Hardcover"; "Paperback"; "100"; "0" |] |> StringValues |> SimpleData
    let expected =
        seq {
            BookType.EBook
            BookType.Hardcover
            BookType.Paperback
            enum<BookType> 100
            BookType.Unknown
        }
    let result = defaultParseModel<BookType seq> data
    result |> shouldEqualSeq expected

type Baz = {
    Name: string option
    Value: int Nullable
}

[<Struct; NoEquality; NoComparison>]
type Bar = { Bar: string | null; Baz: Baz | null }

type Foo = { Foo: string; Bars: Bar option seq }

[<Fact>]
let ``defaultParseModel<Foo> parses data with non-sequential index elements`` () =
    let modelData =
        [
            "Bars[2].Bar", StringValues "Bar"
            "Bars[0].Baz.Name", StringValues "abc"
            "Bars[0].Baz.Value", StringValues "0"
            "Bars[2].Baz.Value", StringValues "1"
        ]
        |> toComplexData
    let expected = {
        Foo = Unchecked.defaultof<_>
        Bars = [|
            Some {
                Bar = null
                Baz = {
                    Name = Some "abc"
                    Value = Nullable 0
                }
            }
            None
            Some {
                Bar = "Bar"
                Baz = { Name = None; Value = Nullable 1 }
            }
        |]
    }
    let result = defaultParseModel<Foo> modelData
    result |> shouldEquivalent expected

[<Fact>]
let ``defaultParseModel<Foo> parses data with unmatched prefix`` () =
    let modelData = [ "Barss[0].Baz.Value", StringValues "0" ] |> toComplexData
    let expected = {
        Foo = Unchecked.defaultof<_>
        Bars = Unchecked.defaultof<_>
    }
    let result = defaultParseModel<Foo> modelData
    result |> shouldEquivalent expected

[<Fact>]
let ``defaultParseModel<Foo> parses data with improper index access`` () =
    let modelData = [ "Bars[0].Baz[0].Value", StringValues "0" ] |> toComplexData
    let expected = {
        Foo = Unchecked.defaultof<_>
        Bars = [|
            Some {
                Bar = null
                Baz = { Name = None; Value = Nullable() }
            }
        |]
    }
    let result = defaultParseModel<Foo> modelData
    result |> shouldEquivalent expected

[<Fact>]
let ``defaultParseModel<Foo> parses data with partially incorrect keys`` () =
    let modelData = [ "Bars[0].Test.Descr", StringValues "0" ] |> toComplexData
    let expected = {
        Foo = Unchecked.defaultof<_>
        Bars = [| Some { Bar = null; Baz = null } |]
    }
    let result = defaultParseModel<Foo> modelData
    result |> shouldEquivalent expected

[<Fact>]
let ``defaultParseModel<Foo> parses data with missing index`` () =
    let modelData = [ "Bars.Baz.Value", StringValues "0" ] |> toComplexData
    let expected = {
        Foo = Unchecked.defaultof<_>
        Bars = [||]
    }
    let result = defaultParseModel<Foo> modelData
    result |> shouldEquivalent expected

[<Fact>]
let ``defaultParseModel<Bar> parses data with no matched prefix`` () =
    let modelData = [ "Bazz.Value", StringValues "0" ] |> toComplexData
    let expected = { Bar = null; Baz = null }
    let result = defaultParseModel<Bar> modelData
    result |> shouldEquivalent expected

type AnonymousType1 = {|
    Value:
        {|
            Value: {| Value: {| Id: int; Name: string |} |}
        |}
|}

[<Fact>]
let ``defaultParseModel<AnonymousType1> parses nested anonymous type data`` () =
    let modelData =
        [
            "Value.Value.Value.Name", StringValues "foo"
            "Value.Value.Value.Id", StringValues "111"
        ]
        |> toComplexData
    let expected: AnonymousType1 = {|
        Value = {|
            Value = {|
                Value = {| Id = 111; Name = "foo" |}
            |}
        |}
    |}
    let result = defaultParseModel<AnonymousType1> modelData
    result |> shouldEqual expected

type AnonymousType2 = {|
    Values:
        {|
            Value:
                {|
                    Values:
                        {|
                            Value: {| Id: int; Name: string | null |}
                        |} array
                |}
        |} array
|}

[<Fact>]
let ``defaultParseModel<AnonymousType2> parses deeply nested anonymous type data`` () =
    let modelData =
        [
            "Values[2].Value.Values[2].Value.Name", StringValues "foo"
            "Values[2].Value.Values[0].Value.Id", StringValues "111"
            "Values[1].Value.Values[0].Value.Name", StringValues "bar"
            "Values[2].Value.Values[2].Value.Id", StringValues "222"
        ]
        |> toComplexData
    let expected: AnonymousType2 = {|
        Values = [|
            Unchecked.defaultof<_>
            {|
                Value = {|
                    Values = [| {| Value = {| Id = 0; Name = "bar" |} |} |]
                |}
            |}
            {|
                Value = {|
                    Values = [|
                        {|
                            Value = {| Id = 111; Name = null |}
                        |}
                        Unchecked.defaultof<_>
                        {|
                            Value = {| Id = 222; Name = "foo" |}
                        |}
                    |]
                |}
            |}
        |]
    |}
    let result = defaultParseModel<AnonymousType2> modelData
    result |> shouldEqual expected

[<Fact>]
let ``defaultParseModel<int> fails to parse non-integer data`` () =
    let modelData =
        [
            "FirstName", StringValues "Susan"
            "MiddleName", StringValues "Elisabeth"
            "LastName", StringValues "Doe"
        ]
        |> toComplexData
    let expected =
        "Could not parse value 'seq [[FirstName, Susan]; [MiddleName, Elisabeth]; [LastName, Doe]]' to type 'System.Int32'."
    let result () =
        defaultParseModel<int> modelData |> ignore
    result |> shouldFailWithMessage<NotParsedException> expected

type Poco() =
    member val Id = 0 with get, set
    member val Name = "" with get, set
    member val Value = 0 with get, set

[<Fact>]
let ``defaultParseModel<Poco> parses valid POCO data`` () =
    let modelData =
        [
            "Id", StringValues "666"
            "Name", StringValues "Lorem ipsum"
            "Value", StringValues "1234"
        ]
        |> toComplexData
    let expected = Poco(Id = 666, Name = "Lorem ipsum", Value = 1_234)
    let result = defaultParseModel<Poco> modelData
    result |> shouldEquivalent expected
