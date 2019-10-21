using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    internal sealed class FeatureStore<TState, TAction, TFeatureState, TFeatureAction>
        : Store<TFeatureState, TFeatureAction>, IConnectableStore
    {
        private readonly IConnectableObservable<TFeatureAction> actions;

        private readonly IConnectableObservable<(TFeatureAction action, TFeatureState state)> actionStates;

        private readonly IConnectableObservable<TFeatureState> states;

        private readonly Action<TFeatureAction> dispatchAction;


        private IDisposable actionsConnection;

        private IDisposable actionStatesConnection;

        private IDisposable stateConnection;


        public FeatureStore(
            Store<TState, TAction> store,
            Func<TState, TFeatureState> stateProjector,
            Func<TAction, (bool, TFeatureAction)> actionChooser,
            Func<TFeatureAction, TAction> actionGeneralizer
        )
            : base(stateProjector(store.initialState))
        {
            actionStates = store.ActionStates
                .Select(actionState => (actionChooser(actionState.action), actionState.state))
                .Where(tuple => tuple.Item1.Item1)
                .Select(tuple => (tuple.Item1.Item2, stateProjector(tuple.state)))
                .Publish();

            ActionStates = actionStates.AsObservable();


            actions = actionStates
                .Select(actionState => actionState.Item1)
                .Publish();

            Actions = actions.AsObservable();


            states =  actionStates
                .Select(actionState => actionState.Item2)
                .StartWith(initialState)
                .DistinctUntilChanged()
                .Replay(1);


            dispatchAction = action => store.Dispatch(actionGeneralizer(action));
        }


        internal override IObservable<TFeatureAction> Actions { get; }

        internal override IObservable<(TFeatureAction action, TFeatureState state)> ActionStates { get; }


        public override void Dispatch(TFeatureAction action) => dispatchAction(action);

        public override IDisposable Subscribe(IObserver<TFeatureState> observer)
            => states.Subscribe(observer);


        void IConnectableStore.Connect()
        {
            lock (this)
            {
                stateConnection = stateConnection ?? states.Connect();
                actionsConnection = actionsConnection ?? actions.Connect();
                actionStatesConnection = actionStatesConnection ?? actionStates.Connect();
            }
        }

        public void Dispose()
        {
            using var resource1 = actionStatesConnection;
            using var resource2 = actionsConnection;
            using var resource3 = stateConnection;
        }
    }
}
