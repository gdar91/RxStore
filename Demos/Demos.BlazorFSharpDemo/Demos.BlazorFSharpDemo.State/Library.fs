namespace Demos.BlazorFSharpDemo.State


type AppState =
    { currentCount: int;
      weatherForecasts: WeatherForecast list }

and WeatherForecast =
    { date: System.DateTime;
      temperatureC: int;
      temperatureF: int;
      summary: string }


type AppAction =
| IncrementCount
| ReceiveWeatherForecasts of ReceiveWeatherForecastsAction

and ReceiveWeatherForecastsAction = { forecasts: WeatherForecast list }


module AppState =
    
    let initial =
        { currentCount = 0;
          weatherForecasts = [] }

    let reduce state = function
    | IncrementCount -> { state with currentCount = state.currentCount + 1 }
    | ReceiveWeatherForecasts action -> { state with weatherForecasts = action.forecasts }
