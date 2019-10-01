using System;

namespace RxStore
{
    public interface IState<TState, TAction> : IObservable<TState>
    {
    }
}
