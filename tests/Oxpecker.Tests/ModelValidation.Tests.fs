module Oxpecker.Tests.ModelValidation

open System
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
