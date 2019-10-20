using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    internal sealed class SourceStore<TState, TAction> : Store<TState, TAction>, IConnectableStore
    {
        private readonly ISubject<TAction> actions = new Subject<TAction>();

        private readonly IConnectableObservable<(TAction action, TState state)> actionStates;

        private readonly IConnectableObservable<TState> states;


        private IDisposable actionStatesConnection;

        private IDisposable statesConnection;


        public SourceStore(Func<TState, TAction, TState> reducer, TState initialState)
            : base(initialState)
        {
            Actions = actions.AsObservable();


            actionStates = actions
                .Scan<TAction, (TAction action, TState state)>(
                    (default, initialState),
                    (tuple, action) => (action, reducer(tuple.state, action))
                )
                .Publish();

            ActionStates = actionStates.AsObservable();


            states = actionStates
                .Select(actionState => actionState.state)
                .StartWith(initialState)
                .DistinctUntilChanged()
                .Replay(1);
        }


        internal override IObservable<TAction> Actions { get; }

        internal override IObservable<(TAction action, TState state)> ActionStates { get; }


        public override void Dispatch(TAction action) => actions.OnNext(action);

        public override IDisposable Subscribe(IObserver<TState> observer) => states.Subscribe(observer);


        void IConnectable.Connect()
        {
            lock (this)
            {
                statesConnection = statesConnection ?? states.Connect();
                actionStatesConnection = actionStatesConnection ?? actionStates.Connect();
            }
        }

        public void Dispose()
        {
            using var resource1 = this.actionStatesConnection;
            using var resource2 = this.statesConnection;
        }
    }
}
