module WeatherApp.templates.weather

open Microsoft.AspNetCore.Http
open Oxpecker
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open WeatherApp.Models

let data (ctx: HttpContext) (forecasts: WeatherForecast[]) =
    div(id="weatherData") {
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
        form(action="/weatherData", hxTarget="#weatherData", hxSwap="outerHTML", hxPushUrl="false",
             method="POST"){
            ctx.GetAntiforgeryInput()
            input(type'="hidden", name="test", value="test")
            button(type'="submit") { "Refresh" }
        }
    }


let html (ctx: HttpContext) =
    ctx.Items["Title"] <- "Weather"

    Fragment(){
        h1() { "Weather" }
        p() { "This component demonstrates showing data." }
        p(hxGet="/weatherData", hxTrigger="load", hxSwap="outerHTML"){
            em() { "Loading..." }
        }
    }
