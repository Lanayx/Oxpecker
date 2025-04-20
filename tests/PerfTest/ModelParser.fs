namespace PerfTest

open System
open System.Collections.Generic
open BenchmarkDotNet.Attributes
open Microsoft.Extensions.Primitives
open Oxpecker
open System.Globalization

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

[<MemoryDiagnoser>]
type ModelParsing() =
    static let modelData =
        [
            "Id", StringValues(Guid.NewGuid().ToString())
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
        |> List.map KeyValuePair.Create
        |> Dictionary

    let firstValue (rawValues: StringValues) =
        if rawValues.Count > 0 then rawValues[0] else null

    let parseModel (culture: CultureInfo) (data: Dictionary<string, StringValues>) = {
        Id =
            let guid = data["Id"] |> firstValue |> nonNull
            Guid.Parse(guid, culture)
        FirstName = data["FirstName"] |> firstValue
        MiddleName = data["MiddleName"] |> firstValue |> Option.ofObj
        LastName = data["LastName"] |> firstValue
        Sex =
            match data["Sex"] |> firstValue with
            | "Female" -> Female
            | "Male" -> Male
            | value -> failwith $"Value '{value}' could not be parsed to {typeof<Sex>}"
        BirthDate =
            let dt = data["BirthDate"] |> firstValue |> nonNull
            DateTime.Parse(dt, culture)

        Nicknames = Some [ yield! data["Nicknames"] |> Seq.cast ]
        Children = [|
            let age = data["Children[0].Age"] |> firstValue |> nonNull
            {
                Name = data["Children[0].Name"] |> firstValue
                Age = Int32.Parse(age, culture)
            }

            let age = data["Children[1].Age"] |> firstValue |> nonNull
            {
                Name = data["Children[1].Name"] |> firstValue
                Age = Int32.Parse(age, culture)
            }

            let age = data["Children[2].Age"] |> firstValue |> nonNull
            {
                Name = data["Children[2].Name"] |> firstValue
                Age = Int32.Parse(age, culture)
            }
        |]
    }

    static let culture = CultureInfo.InvariantCulture

    [<Benchmark(Baseline = true)>]
    member _.DirectModelParser() = parseModel culture modelData

    [<Benchmark>]
    member _.TypeShapeBasedModelParser() =
        ModelParser.parseModel<Model> culture (ComplexData modelData)
