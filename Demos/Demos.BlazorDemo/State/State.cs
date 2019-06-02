namespace Demos.BlazorDemo.State
{
    public sealed class AppState
    {
        public int CurrentCount { get; private set; }

        public WeatherForecast[] WeatherForecasts { get; private set; }


        public static AppState Initial { get; } = new AppState
        {
            CurrentCount = 0,
            WeatherForecasts = new WeatherForecast[0]
        };

        public static AppState Reducer(AppState state, IAppAction action) => action switch
        {
            IncrementCountAction incrementCountAction => new AppState
            {
                CurrentCount = state.CurrentCount + 1,
                WeatherForecasts = state.WeatherForecasts
            },

            ReceiveWeatherForecastsAction receiveWeatherForecastsAction => new AppState
            {
                CurrentCount = state.CurrentCount,
                WeatherForecasts = receiveWeatherForecastsAction.Forecasts
            },

            _ => state
        };
    }

    public class WeatherForecast
    {
        public System.DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF { get; set; }

        public string Summary { get; set; }
    }


    public interface IAppAction
    {
    }

    public class IncrementCountAction : IAppAction
    {
    }

    public class ReceiveWeatherForecastsAction : IAppAction
    {
        public WeatherForecast[] Forecasts { get; set; }
    }
}
