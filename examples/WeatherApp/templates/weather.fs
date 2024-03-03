module WeatherApp.templates.weather

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open WeatherApp.templates.shared
open WeatherApp.Models

let data (forecasts: WeatherForecast[]) =
    table(class'="table") {
        thead() {
            tr() {
                th() { "Date" }
                th() { "Temp. (C)" }
                th() { "Temp. (F)" }
                th() { "Summary" }
            }
        }
        tbody() {
            for forecast in forecasts do
                tr() {
                    td() { forecast.Date.ToShortDateString() }
                    td() { string forecast.TemperatureC }
                    td() { string forecast.TemperatureF }
                    td() { forecast.Summary }
                }
        }
    }

let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Weather"

    __(){
        h1() { "Weather" }
        p() { "This component demonstrates showing data." }
        p(hxGet="/weatherData", hxTrigger="load", hxSwap="outerHTML"){
            em() { "Loading..." }
        }
    }
    |> layout.html ctx
