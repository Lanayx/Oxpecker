module ContactApp.templates.shared.errors

open Oxpecker.ViewEngine
open Oxpecker.ModelValidation

let showErrors (modelState: ModelState<_>) (fieldName: string) =
    match modelState with
    | ModelState.Invalid (_, modelErrors) ->
        span(class'="error"){
            modelErrors.ErrorMessagesFor(fieldName) |> String.concat ", "
        } :> HtmlElement
    | _ ->
        Fragment()
