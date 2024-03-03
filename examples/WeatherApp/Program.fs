open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Oxpecker
open WeatherApp.templates
open WeatherApp.Models

let htmlView' f (ctx: HttpContext) = ctx.WriteHtmlView(f ctx)

let getWeatherData (ctx: HttpContext) =
    task {
        // Simulate asynchronous loading to demonstrate long rendering
        do! Task.Delay(500)

        let startDate = DateOnly.FromDateTime(DateTime.Now)
        let summaries = [ "Freezing"; "Bracing"; "Chilly"; "Cool"; "Mild"; "Warm"; "Balmy"; "Hot"; "Sweltering"; "Scorching" ]
        let forecasts =
            [|
                for index in 1..5 do
                    {
                        Date = startDate.AddDays(index)
                        TemperatureC = Random.Shared.Next(-20, 55)
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    }
            |]
        return! ctx.WriteHtmlView(weather.data forecasts)
    } :> Task

let endpoints = [
    GET [
        route "/" <| htmlView' home.html
        route "/counter" <| htmlView' counter.html
        route "/weather" <| htmlView' weather.html
        route "/weatherData" <| getWeatherData
        route "/error" <| htmlView' error.html
    ]
]

let configureApp (appBuilder: WebApplication) =
    if appBuilder.Environment.IsDevelopment() then
        appBuilder.UseDeveloperExceptionPage() |> ignore
    else
        appBuilder.UseExceptionHandler("/error", true) |> ignore
    appBuilder
        .UseStaticFiles()
        .UseAntiforgery()
        .UseRouting()
        .UseOxpecker(endpoints) |> ignore

let configureServices (services: IServiceCollection) =
    services
        .AddRouting()
        .AddAntiforgery()
        .AddOxpecker()
    |> ignore


[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services
    let app = builder.Build()
    configureApp app
    app.Run()
    0
