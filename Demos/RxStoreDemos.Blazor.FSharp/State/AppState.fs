namespace RxStoreDemos.Blazor.FSharp.State

open Weather


type AppState =
    { CurrentCount: int;
      WeatherForecastsOption: WeatherForecast list option }


type AppAction =
| IncrementCount
| ReceiveWeatherForecasts of WeatherForecast list


module AppState =

    let reducer state = function
    | IncrementCount ->
        { state with CurrentCount = state.CurrentCount + 1 }
    | ReceiveWeatherForecasts weatherForecasts ->
        { state with WeatherForecastsOption = Some weatherForecasts }


    let initial =
        { CurrentCount = 0;
          WeatherForecastsOption = None }
