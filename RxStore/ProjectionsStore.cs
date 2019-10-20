using System;

namespace RxStore
{
    public abstract class ProjectionsStore<TState, TAction> : Store<TState, TAction>
    {
        private readonly Store<TState, TAction> store;


        protected ProjectionsStore(Store<TState, TAction> store) : base(store.initialState)
        {
            this.store = store;
        }


        internal override IObservable<TAction> Actions => store.Actions;

        internal override IObservable<(TAction action, TState state)> ActionStates => store.ActionStates;


        public override void Dispatch(TAction action) => store.Dispatch(action);

        public override IDisposable Subscribe(IObserver<TState> observer) => store.Subscribe(observer);
    }
}
