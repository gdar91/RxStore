namespace RxStoreDemos.Blazor.FSharp.State

open System

module Weather =

    [<Measure>]
    type C

    [<Measure>]
    type F

    type WeatherForecast =
        { Date: DateTime;
          TemperatureC: float<C>;
          Summary: WeatherSummary }

    and WeatherSummary =
    | Freezing
    | Bracing
    | Chilly
    | Cool
    | Mild
    | Warm
    | Balmy
    | Hot
    | Sweltering
    | Scorching

    let fahrenheitOfCelsius celsius = 32.0<F> + 1.8<F/C> * celsius

    let temperatureF weatherForecast = weatherForecast.TemperatureC |> fahrenheitOfCelsius
