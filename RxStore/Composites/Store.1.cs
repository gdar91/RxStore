using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace RxStore
{
    public class Store<
        TState, TAction,
        TState1, TAction1
    >
        : Store<TState, TAction>
    {
        protected Store(
            Store<TState1, TAction1> store1,
            Func<TAction1, TAction> action1Generalizer,
            Func<
                TState1,
                TState
            > stateComposer,
            Func<
                Func<TAction1, Unit>,
                Func<TAction, Unit>,
                TAction,
                Unit
            > actionSelector
        )
            : base(
                Construct(
                    store1,
                    action1Generalizer,
                    stateComposer,
                    actionSelector,
                    out var dispatch
                ),
                dispatch
            )
        { }


        private delegate StateTransition StateTransitionGeneralizer(
            Store<TState1, TAction1>.StateTransition stateTransition1,
            TState state
        );

        private static IObservable<StateTransition> Construct(
            Store<TState1, TAction1> store1,
            Func<TAction1, TAction> action1Generalizer,
            Func<
                TState1,
                TState
            > stateComposer,
            Func<
                Func<TAction1, Unit>,
                Func<TAction, Unit>,
                TAction,
                Unit
            > actionSelector,
            out Func<TAction, Unit> dispatch
        )
        {
            dispatch = action => actionSelector(
                store1.Dispatch,
                action => Unit.Default,
                action
            );


            return Observable.Zip(
                Observable.Merge(
                    store1.OutStateTransitions
                        .Skip(1)
                        .Select(next => (StateTransitionGeneralizer) For1),
                    
                    Observable.Return<StateTransitionGeneralizer>(For0)
                ),
                store1.OutStateTransitions,
                (func, stateTransition1) => func(
                    stateTransition1,
                    stateComposer(
                        stateTransition1.State
                    )
                )
            );


            Store<TState, TAction>.StateTransition For0(
                Store<TState1, TAction1>.StateTransition stateTransition1,
                TState state
            )
            {
                return StateTransition.NewInitial(state);
            }

            Store<TState, TAction>.StateTransition For1(
                Store<TState1, TAction1>.StateTransition stateTransition1,
                TState state
            )
            {
                return stateTransition1 switch
                {
                    Store<TState1, TAction1>.StateTransition.ByAction byAction =>
                        StateTransition.NewByAction(state, action1Generalizer(byAction.Action)),
                    _ => throw new NotImplementedException()
                };
            }
        }
    }
}
