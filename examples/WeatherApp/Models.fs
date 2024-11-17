module WeatherApp.Models

open System

type WeatherForecast = {
    Date: DateOnly
    TemperatureC: int
    Summary: string | null
} with
    member this.TemperatureF = 32 +  int (float this.TemperatureC / 0.5556)
