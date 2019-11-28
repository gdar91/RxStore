using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace RxStore
{
    public abstract class Store
    {
        internal Store(Type stateType, Type actionType)
        {
            StateType = stateType;
            ActionType = actionType;
        }


        internal Type StateType { get; }

        internal Type ActionType { get; }
    }


    public abstract class Store<TState, TAction> : Store, IObservable<TState>
    {
        private readonly IObservable<StateTransition> stateTransitions;

        private readonly IObservable<TState> states;

        private readonly Func<TAction, Unit> dispatch;


        internal Store(IObservable<StateTransition> stateTransitions, Func<TAction, Unit> dispatch)
            : base(typeof(TState), typeof(TAction))
        {
            this.stateTransitions = stateTransitions
                .Publish()
                .AutoConnect(3);


            this.states = this.stateTransitions
                .Select(stateTransition => stateTransition.State)
                .DistinctUntilChanged()
                .Replay(1)
                .AutoConnect(0);


            var actions = this.stateTransitions
                .OfType<StateTransition.ByAction>()
                .Select(stateTransition => stateTransition.Action)
                .Publish()
                .AutoConnect(0);

            var fallback = Observable.Empty<TAction>();

            Effects(actions)
                .Where(effects => effects != null)
                .Select(effects => effects.Catch(fallback))
                .ToObservable()
                .Merge()
                .Do(action => Task.Run(() => dispatch(action)))
                .Publish()
                .AutoConnect(0);


            this.dispatch = dispatch;


            OutStateTransitions = this.stateTransitions
                .Replay(1)
                .AutoConnect(0);
        }


        protected Store(Func<TState, TAction, TState> reducer, TState initialState)
            : this(Construct(reducer, initialState, out var dispatch), dispatch)
        { }

        private static IObservable<StateTransition> Construct(
            Func<TState, TAction, TState> reducer,
            TState initialState,
            out Func<TAction, Unit> dispatch
        )
        {
            var actionsSubject = new Subject<TAction>();

            var initialStateTransition = StateTransition.NewInitial(initialState);

            var stateTransitions =
                actionsSubject
                    .Scan(
                        initialStateTransition,
                        (stateTransition, action) =>
                            StateTransition.NewByAction(
                                reducer(stateTransition.State, action),
                                action
                            )
                    )
                    .StartWith(initialStateTransition);

            dispatch = action =>
            {
                actionsSubject.OnNext(action);

                return Unit.Default;
            };

            return stateTransitions;
        }


        internal IObservable<StateTransition> OutStateTransitions { get; }


        protected virtual IEnumerable<IObservable<TAction>> Effects(IObservable<TAction> actions) =>
            Enumerable.Empty<IObservable<TAction>>();

        public Unit Dispatch(TAction action) => dispatch(action);

        public IDisposable Subscribe(IObserver<TState> observer) => states.Subscribe(observer);




        internal abstract class StateTransition
        {
            private StateTransition(TState state)
            {
                State = state;
            }


            public TState State { get; }


            public static StateTransition NewInitial(TState initialState) =>
                new Initial(initialState);
            
            public static StateTransition NewByAction(TState state, TAction action) =>
                new ByAction(state, action);


            public sealed class Initial : StateTransition
            {
                public Initial(TState state) : base(state)
                { }
            }


            public sealed class ByAction : StateTransition
            {
                public ByAction(TState state, TAction action) : base(state)
                    => Action = action;

                public TAction Action { get; }
            }
        }
    }
}
