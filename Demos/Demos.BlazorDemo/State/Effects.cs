using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using RxStore;

namespace Demos.BlazorDemo.State
{
    public class AppEffects : IDeclareEffects<AppState, IAppAction>
    {
        public IEnumerable<IObservable<IAppAction>> GetEffects(IObservable<AppState> states, IObservable<IAppAction> actions)
        {
            yield return actions
                .OfType<IncrementCountAction>()
                .Throttle(TimeSpan.FromSeconds(2))
                .WithLatestFrom(states, (action, state) => state)
                .Do(state => Console.WriteLine($"Current count: {state.CurrentCount}"))
                .IgnoreElementsAs<IAppAction>();
            
            yield return actions
                .OfType<ReceiveWeatherForecastsAction>()
                .Select(action => new IncrementCountAction())
                .Do(action => Console.WriteLine("When you load weather forecasts, the counter is increased by 1"));
        }
    }
}
