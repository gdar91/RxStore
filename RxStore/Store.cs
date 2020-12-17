using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public sealed partial class Store<TEvent, TState> :
        ISubject<TEvent, TState>,
        IDispatcher<TEvent>,
        IProjection<TState>
    {
        public Store(TState initialState, Func<TState, TEvent, TState> reducer)
        {
            var initialStateTransition = StateTransition.NewInitial(initialState);

            StateTransitions =
                Events
                    .Scan(initialStateTransition, StateTransition.LiftReducer(reducer))
                    .StartWith(initialStateTransition)
                    .Replay(1)
                    .AutoConnect();

            States =
                StateTransitions
                    .Select(StateTransition.StateOf)
                    .DistinctUntilChanged()
                    .Replay(1)
                    .AutoConnect(0);
        }


        private ISubject<TEvent> Events { get; } = Subject.Synchronize(new Subject<TEvent>());


        internal IObservable<StateTransition> StateTransitions { get; }

        private IObservable<TState> States { get; }


        public void OnNext(TEvent value) => Events.OnNext(value);

        public void OnError(Exception error) => Events.OnError(error);

        public void OnCompleted() => Events.OnCompleted();


        public IDisposable Subscribe(IObserver<TState> observer) => States.Subscribe(observer);


        public void Dispose() => OnCompleted();
    }
}
