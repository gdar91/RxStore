using System;

namespace RxStore
{
    public interface IStore<TState, TAction> : IObservable<TState>
    {
        IObservable<TAction> Actions { get; }

        void Dispatch(TAction action);
    }
}
