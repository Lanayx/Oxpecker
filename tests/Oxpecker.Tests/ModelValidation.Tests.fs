module Oxpecker.Tests.ModelValidation

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
