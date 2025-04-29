namespace PerfTest

open System
open System.Collections.Generic
open BenchmarkDotNet.Attributes
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open Oxpecker
open System.Globalization

type Sex =
    | Male
    | Female

[<CLIMutable>]
type Child = { Name: string | null; Age: int }

[<CLIMutable>]
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

    let parseModel (culture: CultureInfo) (data: Dictionary<string,StringValues>) = {
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

    // BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
    // AMD Ryzen 5 5600H with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
    // .NET SDK 9.0.201
    //   [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2 DEBUG
    //   DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
    //
    //
    // | Method              | Mean         | Error       | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
    // |-------------------- |-------------:|------------:|----------:|-------:|--------:|-------:|----------:|------------:|
    // | DirectModelParser   |     543.1 ns |     4.65 ns |   4.12 ns |   1.00 |    0.01 | 0.0906 |     760 B |        1.00 |
    // | OxpeckerModelParser |   1,776.6 ns |    19.53 ns |  16.31 ns |   3.27 |    0.04 | 0.0725 |     608 B |        0.80 |
    // | GiraffeModelParser  | 123,657.0 ns | 1,058.06 ns | 937.94 ns | 227.71 |    2.37 | 7.3242 |   62546 B |       82.30 |



    static let options  = ModelBinderOptions.Default
    static let complexData = RawData.initComplexData modelData

    [<Benchmark(Baseline = true)>]
    member _.DirectModelParser() = parseModel options.CultureInfo modelData

    [<Benchmark>]
    member _.OxpeckerModelParser() = ModelParser.parseModel<Model> options complexData

    [<Benchmark>]
    member _.GiraffeModelParser() = Giraffe.ModelParser.parse<Model> (Some options.CultureInfo) modelData
