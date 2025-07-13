module WeatherApp.templates.error

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open System.Diagnostics
open System

let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Error"
    ctx.Response.StatusCode <- 200

    let requestId =
        match Activity.Current with
        | null -> ctx.TraceIdentifier
        | activity -> activity.Id |> string
    let showRequestId = requestId |> String.IsNullOrWhiteSpace |> not

    Fragment(){
        h1(class'="text-danger") { "Error." }
        h2(class'="text-danger") { "An error occurred while processing your request." }
        if showRequestId then
            p() {
                strong() { "Request ID: " }
                code() { requestId }
            }
        h3() { "Development Mode" }
        p() {
            "Swapping to the "
            strong() { "Development" }
            " environment will display more detailed information about the error that occurred."
        }
        p() {
            strong(){ "The Development environment shouldn't be enabled for deployed applications." }
            " It can result in displaying sensitive information from exceptions to end users."
            " For local debugging, enable the "
            strong(){ "Development" }
            " environment by setting the "
            strong(){ "ASPNETCORE_ENVIRONMENT" }
            " environment variable to "
            strong(){ "Development" }
            " and restarting the app."
        }
    }


