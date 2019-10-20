using RxStoreDemos.Blazor.Shared;

namespace RxStoreDemos.Blazor.Client.State
{
    public sealed class AppState
    {
        public AppState(int currentCount, WeatherForecast[] weatherForecasts)
        {
            CurrentCount = currentCount;
            WeatherForecasts = weatherForecasts;
        }


        public int CurrentCount { get; }

        public WeatherForecast[] WeatherForecasts { get; }


        public static AppState Reducer(AppState state, IAppAction action)
        {
            return action switch
            {
                IncrementCountAction a => new AppState(state.CurrentCount + 1, state.WeatherForecasts),
                ReceiveWeatherForecastsAction a => new AppState(state.CurrentCount, a.WeatherForecasts),
                _ => state
            };
        }

        public static AppState Initial { get; } = new AppState(0, null);
    }


    public interface IAppAction
    { }


    public sealed class IncrementCountAction : IAppAction
    { }


    public sealed class ReceiveWeatherForecastsAction : IAppAction
    {
        public ReceiveWeatherForecastsAction(WeatherForecast[] weatherForecasts)
        {
            WeatherForecasts = weatherForecasts;
        }

        public WeatherForecast[] WeatherForecasts { get; }
    }
}
