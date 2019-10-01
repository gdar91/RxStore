using System;

namespace RxStore
{
    public interface IActions<TState, TAction>
    {
        IObservable<TAction> Actions { get; }
    }
}
