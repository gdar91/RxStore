using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace RxStore
{
    public class Store<
        TState, TAction,
        TState1, TAction1,
        TState2, TAction2,
        TState3, TAction3
    >
        : Store<TState, TAction>
    {
        protected Store(
            Store<TState1, TAction1> store1,
            Func<TAction1, TAction> action1Generalizer,
            Store<TState2, TAction2> store2,
            Func<TAction2, TAction> action2Generalizer,
            Store<TState3, TAction3> store3,
            Func<TAction3, TAction> action3Generalizer,
            Func<
                TState1,
                TState2,
                TState3,
                TState
            > stateComposer,
            Func<
                Func<TAction1, Unit>,
                Func<TAction2, Unit>,
                Func<TAction3, Unit>,
                Func<TAction, Unit>,
                TAction,
                Unit
            > actionSelector
        )
            : base(
                Construct(
                    store1,
                    action1Generalizer,
                    store2,
                    action2Generalizer,
                    store3,
                    action3Generalizer,
                    stateComposer,
                    actionSelector,
                    out var dispatch
                ),
                dispatch
            )
        { }


        private delegate StateTransition StateTransitionGeneralizer(
            Store<TState1, TAction1>.StateTransition stateTransition1,
            Store<TState2, TAction2>.StateTransition stateTransition2,
            Store<TState3, TAction3>.StateTransition stateTransition3,
            TState state
        );

        private static IObservable<StateTransition> Construct(
            Store<TState1, TAction1> store1,
            Func<TAction1, TAction> action1Generalizer,
            Store<TState2, TAction2> store2,
            Func<TAction2, TAction> action2Generalizer,
            Store<TState3, TAction3> store3,
            Func<TAction3, TAction> action3Generalizer,
            Func<
                TState1,
                TState2,
                TState3,
                TState
            > stateComposer,
            Func<
                Func<TAction1, Unit>,
                Func<TAction2, Unit>,
                Func<TAction3, Unit>,
                Func<TAction, Unit>,
                TAction,
                Unit
            > actionSelector,
            out Func<TAction, Unit> dispatch
        )
        {
            dispatch = action => actionSelector(
                store1.Dispatch,
                store2.Dispatch,
                store3.Dispatch,
                action => Unit.Default,
                action
            );


            return Observable.Zip(
                Observable.Merge(
                    store1.OutStateTransitions
                        .Skip(1)
                        .Select(next => (StateTransitionGeneralizer) For1),
                    
                    store2.OutStateTransitions
                        .Skip(1)
                        .Select(next => (StateTransitionGeneralizer) For2),
                    
                    store3.OutStateTransitions
                        .Skip(1)
                        .Select(next => (StateTransitionGeneralizer) For3),

                    Observable.Return<StateTransitionGeneralizer>(For0)
                ),
                Observable.CombineLatest(
                    store1.OutStateTransitions,
                    store2.OutStateTransitions,
                    store3.OutStateTransitions,
                    (
                        stateTransition1,
                        stateTransition2,
                        stateTransition3
                    ) => (
                        stateTransition1,
                        stateTransition2,
                        stateTransition3
                    )
                ),
                (func, tuple) => func(
                    tuple.stateTransition1,
                    tuple.stateTransition2,
                    tuple.stateTransition3,
                    stateComposer(
                        tuple.stateTransition1.State,
                        tuple.stateTransition2.State,
                        tuple.stateTransition3.State
                    )
                )
            );


            Store<TState, TAction>.StateTransition For0(
                Store<TState1, TAction1>.StateTransition stateTransition1,
                Store<TState2, TAction2>.StateTransition stateTransition2,
                Store<TState3, TAction3>.StateTransition stateTransition3,
                TState state
            )
            {
                return StateTransition.NewInitial(state);
            }

            Store<TState, TAction>.StateTransition For1(
                Store<TState1, TAction1>.StateTransition stateTransition1,
                Store<TState2, TAction2>.StateTransition stateTransition2,
                Store<TState3, TAction3>.StateTransition stateTransition3,
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

            Store<TState, TAction>.StateTransition For2(
                Store<TState1, TAction1>.StateTransition stateTransition1,
                Store<TState2, TAction2>.StateTransition stateTransition2,
                Store<TState3, TAction3>.StateTransition stateTransition3,
                TState state
            )
            {
                return stateTransition2 switch
                {
                    Store<TState2, TAction2>.StateTransition.ByAction byAction =>
                        StateTransition.NewByAction(state, action2Generalizer(byAction.Action)),
                    _ => throw new NotImplementedException()
                };
            }

            Store<TState, TAction>.StateTransition For3(
                Store<TState1, TAction1>.StateTransition stateTransition1,
                Store<TState2, TAction2>.StateTransition stateTransition2,
                Store<TState3, TAction3>.StateTransition stateTransition3,
                TState state
            )
            {
                return stateTransition3 switch
                {
                    Store<TState3, TAction3>.StateTransition.ByAction byAction =>
                        StateTransition.NewByAction(state, action3Generalizer(byAction.Action)),
                    _ => throw new NotImplementedException()
                };
            }
        }
    }
}
