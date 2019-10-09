using System;
using System.Reactive.Linq;

namespace RxStore
{
    internal sealed class FeatureStore<TState, TAction, TFeatureState, TFeatureAction>
        : IStore<TFeatureState, TFeatureAction>
    {
        private readonly Action<TFeatureAction> dispatchAction;

        private readonly IObservable<TFeatureState> state;


        public FeatureStore(
            IStore<TState, TAction> store,
            Func<TState, TFeatureState> stateProjector,
            Func<TAction, (bool, TFeatureAction)> actionChooser,
            Func<TFeatureAction, TAction> actionGeneralizer
        )
        {
            Actions = store.Actions
                .Select(actionChooser)
                .Where(tuple => tuple.Item1)
                .Select(tuple => tuple.Item2);

            dispatchAction = action => store.Dispatch(actionGeneralizer(action));
            
            state = store.Project(stateProjector);
        }


        public IObservable<TFeatureAction> Actions { get; }

        public void Dispatch(TFeatureAction action) => dispatchAction(action);

        public IDisposable Subscribe(IObserver<TFeatureState> observer)
            => state.Subscribe(observer);
    }
}
