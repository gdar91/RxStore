### Brief Summary
RxStore is a reactive [Redux](https://redux.js.org/)-style predictable state container utilizing the power of [Rx.NET](http://reactivex.io/). Inspired by [NgRx](https://ngrx.io/guide/store).

### Basic Usage
To start using RxStore, you can create your app-specific state class, action classes and a reducer. You can also add side effects.

#### Creating a basic state class
```
public class AppState
{
    public int Counter { get; set; }
}
```

#### Creating an action hierarchy
````
public interface IAppAction
{ }

public class IncrementCounterAction : IAppAction
{ }

public class SetCounterAction : IAppAction
{
    public int Value { get; set; }
}
````

#### Creating a reducer and an initial state (inside a class)
````
public static AppState Reducer(AppState state, IAppAction action)
{
    switch (action)
    {
        case IncrementCounterAction incrementCounterAction:
            return new AppState { Counter = state.Counter + 1 };

        case SetCounterAction setCounterAction:
            return state.Counter == setCounterAction.Value
                ? state
                : new AppState { Counter = setCounterAction.Value };

        default:
            return state;
    }
}

public static AppState Initial { get; } = new AppState { Counter = 0 };
````

#### Adding to a DI container
````
services.AddStore<AppState, IAppAction>(AppState.Reducer, AppState.Initial);
````

#### Accessing from the DI
Inject it as a dependency of type Store<AppState, IAppAction>.

#### Using the store
````
var counterObservable = store.Project(state => state.Counter);
````
You can use the `counterObservable` in the code.
If you are using Blazor and want to asynchronously bind the observable in the razor syntax, check out [BlazeRx](https://www.nuget.org/packages/BlazeRx/).




### Side-Effects
*Documentation coming soon...*




### Demo projects
Check out our [demo projects](https://github.com/gdar91/RxStore/tree/master/Demos) on GitHub.
