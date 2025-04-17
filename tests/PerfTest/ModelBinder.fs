namespace PerfTest

open System
open System.Collections.Generic
open BenchmarkDotNet.Attributes
open Microsoft.Extensions.Primitives
open Oxpecker

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
type ModelBinding() =
    static let modelData =
        let id = Guid.NewGuid()
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

    static let directModelBinder =
        let firstValue (rawValues: StringValues) =
            if rawValues.Count > 0 then rawValues[0] else null
        let bindModel (data: IDictionary<string, StringValues>) = {
            Id = data["Id"] |> firstValue |> nonNull |> Guid.Parse
            FirstName = data["FirstName"] |> firstValue
            MiddleName = data["MiddleName"] |> firstValue |> Option.ofObj
            LastName = data["LastName"] |> firstValue
            Sex =
                match data["Sex"] |> firstValue with
                | "Female" -> Female
                | "Male" -> Male
                | value -> failwith $"Value '{value}' could not be parsed to {typeof<Sex>}"
            BirthDate = data["BirthDate"] |> firstValue |> nonNull |> DateTime.Parse
            Nicknames = Some [ yield! data["Nicknames"] |> Seq.cast ]
            Children = [|
                {
                    Name = data["Children[0].Name"] |> firstValue
                    Age = data["Children[0].Age"] |> firstValue |> int
                }
                {
                    Name = data["Children[1].Name"] |> firstValue
                    Age = data["Children[1].Age"] |> firstValue |> int
                }
                {
                    Name = data["Children[2].Name"] |> firstValue
                    Age = data["Children[2].Age"] |> firstValue |> int
                }
            |]
        }
        { new IModelBinder with
            member this.Bind<'T>(data) =
                bindModel(Dictionary data) |> unbox<'T>
        }

    static let typeShapeBasedModelBinder =
        ModelBinder(ModelBinderOptions.Default) :> IModelBinder

    [<Benchmark(Baseline = true)>]
    member this.DirectModelBinder() = directModelBinder.Bind<Model> modelData

    [<Benchmark>]
    member this.TypeShapeBasedModelBinder() =
        typeShapeBasedModelBinder.Bind<Model> modelData
