using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public sealed class Store<TState, TAction> :
        IState<TState, TAction>,
        IActions<TState, TAction>,
        IDispatcher<TState, TAction>,
        IDisposable
    {
        internal readonly ISubject<TAction> actions = new Subject<TAction>();

        private readonly IConnectableObservable<TState> states;
        
        private readonly IDisposable connection;


        public Store(Func<TState, TAction, TState> reducer, TState initialState)
        {
            Actions = actions.AsObservable();

            states = actions
                .Scan(initialState, reducer)
                .StartWith(initialState)
                .DistinctUntilChanged()
                .Replay(1);

            connection = states.Connect();
        }


        public IObservable<TAction> Actions { get; }


        public void Dispatch(TAction action) => actions.OnNext(action);


        public IDisposable Subscribe(IObserver<TState> observer)
            => states.Subscribe(observer);


        public void Dispose()
        {
            using var statesConnection = this.connection;
        }
    }
}
