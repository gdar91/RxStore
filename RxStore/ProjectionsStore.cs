using System;

namespace RxStore
{
    public abstract class ProjectionsStore<TState, TAction> : IStore<TState, TAction>
    {
        private readonly IStore<TState, TAction> store;

        protected ProjectionsStore(IStore<TState, TAction> store)
        {
            this.store = store;
        }

        public IObservable<TAction> Actions => store.Actions;

        public void Dispatch(TAction action) => store.Dispatch(action);

        public IDisposable Subscribe(IObserver<TState> observer) => store.Subscribe(observer);
    }
}
