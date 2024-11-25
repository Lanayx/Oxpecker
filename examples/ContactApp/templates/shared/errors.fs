module ContactApp.templates.shared.errors

open Oxpecker.ViewEngine
open Oxpecker.ModelValidation

let showErrors (modelState: ModelState<_>) (fieldName: string) =
    match modelState with
    | ModelState.Invalid (_, modelErrors) ->
        span(class'="error"){
            System.String.Join(", ", modelErrors.ErrorMessagesFor(fieldName))
        } :> HtmlElement
    | _ ->
        Fragment()
