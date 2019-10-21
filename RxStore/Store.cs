using System;

namespace RxStore
{
    public abstract class Store<TState, TAction> : IObservable<TState>
    {
        internal readonly TState initialState;


        internal Store(TState initialState)
        {
            this.initialState = initialState;
        }


        internal abstract IObservable<TAction> Actions { get; }

        internal abstract IObservable<(TAction action, TState state)> ActionStates { get; }


        public abstract void Dispatch(TAction action);
        
        public abstract IDisposable Subscribe(IObserver<TState> observer);
    }


    internal interface IConnectableStore : IDisposable
    {
        void Connect();
    }
}
