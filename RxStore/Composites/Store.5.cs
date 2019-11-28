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
        TState3, TAction3,
        TState4, TAction4,
        TState5, TAction5
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
            Store<TState4, TAction4> store4,
            Func<TAction4, TAction> action4Generalizer,
            Store<TState5, TAction5> store5,
            Func<TAction5, TAction> action5Generalizer,
            Func<
                TState1,
                TState2,
                TState3,
                TState4,
                TState5,
                TState
            > stateComposer,
            Func<
                Func<TAction1, Unit>,
                Func<TAction2, Unit>,
                Func<TAction3, Unit>,
                Func<TAction4, Unit>,
                Func<TAction5, Unit>,
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
                    store4,
                    action4Generalizer,
                    store5,
                    action5Generalizer,
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
            Store<TState4, TAction4>.StateTransition stateTransition4,
            Store<TState5, TAction5>.StateTransition stateTransition5,
            TState state
        );

        private static IObservable<StateTransition> Construct(
            Store<TState1, TAction1> store1,
            Func<TAction1, TAction> action1Generalizer,
            Store<TState2, TAction2> store2,
            Func<TAction2, TAction> action2Generalizer,
            Store<TState3, TAction3> store3,
            Func<TAction3, TAction> action3Generalizer,
            Store<TState4, TAction4> store4,
            Func<TAction4, TAction> action4Generalizer,
            Store<TState5, TAction5> store5,
            Func<TAction5, TAction> action5Generalizer,
            Func<
                TState1,
                TState2,
                TState3,
                TState4,
                TState5,
                TState
            > stateComposer,
            Func<
                Func<TAction1, Unit>,
                Func<TAction2, Unit>,
                Func<TAction3, Unit>,
                Func<TAction4, Unit>,
                Func<TAction5, Unit>,
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
                store4.Dispatch,
                store5.Dispatch,
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

                    store4.OutStateTransitions
                        .Skip(1)
                        .Select(next => (StateTransitionGeneralizer) For4),

                    store5.OutStateTransitions
                        .Skip(1)
                        .Select(next => (StateTransitionGeneralizer) For5),

                    Observable.Return<StateTransitionGeneralizer>(For0)
                ),
                Observable.CombineLatest(
                    store1.OutStateTransitions,
                    store2.OutStateTransitions,
                    store3.OutStateTransitions,
                    store4.OutStateTransitions,
                    store5.OutStateTransitions,
                    (
                        stateTransition1,
                        stateTransition2,
                        stateTransition3,
                        stateTransition4,
                        stateTransition5
                    ) => (
                        stateTransition1,
                        stateTransition2,
                        stateTransition3,
                        stateTransition4,
                        stateTransition5
                    )
                ),
                (func, tuple) => func(
                    tuple.stateTransition1,
                    tuple.stateTransition2,
                    tuple.stateTransition3,
                    tuple.stateTransition4,
                    tuple.stateTransition5,
                    stateComposer(
                        tuple.stateTransition1.State,
                        tuple.stateTransition2.State,
                        tuple.stateTransition3.State,
                        tuple.stateTransition4.State,
                        tuple.stateTransition5.State
                    )
                )
            );


            Store<TState, TAction>.StateTransition For0(
                Store<TState1, TAction1>.StateTransition stateTransition1,
                Store<TState2, TAction2>.StateTransition stateTransition2,
                Store<TState3, TAction3>.StateTransition stateTransition3,
                Store<TState4, TAction4>.StateTransition stateTransition4,
                Store<TState5, TAction5>.StateTransition stateTransition5,
                TState state
            )
            {
                return StateTransition.NewInitial(state);
            }

            Store<TState, TAction>.StateTransition For1(
                Store<TState1, TAction1>.StateTransition stateTransition1,
                Store<TState2, TAction2>.StateTransition stateTransition2,
                Store<TState3, TAction3>.StateTransition stateTransition3,
                Store<TState4, TAction4>.StateTransition stateTransition4,
                Store<TState5, TAction5>.StateTransition stateTransition5,
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
                Store<TState4, TAction4>.StateTransition stateTransition4,
                Store<TState5, TAction5>.StateTransition stateTransition5,
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
                Store<TState4, TAction4>.StateTransition stateTransition4,
                Store<TState5, TAction5>.StateTransition stateTransition5,
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

            Store<TState, TAction>.StateTransition For4(
                Store<TState1, TAction1>.StateTransition stateTransition1,
                Store<TState2, TAction2>.StateTransition stateTransition2,
                Store<TState3, TAction3>.StateTransition stateTransition3,
                Store<TState4, TAction4>.StateTransition stateTransition4,
                Store<TState5, TAction5>.StateTransition stateTransition5,
                TState state
            )
            {
                return stateTransition4 switch
                {
                    Store<TState4, TAction4>.StateTransition.ByAction byAction =>
                        StateTransition.NewByAction(state, action4Generalizer(byAction.Action)),
                    _ => throw new NotImplementedException()
                };
            }

            Store<TState, TAction>.StateTransition For5(
                Store<TState1, TAction1>.StateTransition stateTransition1,
                Store<TState2, TAction2>.StateTransition stateTransition2,
                Store<TState3, TAction3>.StateTransition stateTransition3,
                Store<TState4, TAction4>.StateTransition stateTransition4,
                Store<TState5, TAction5>.StateTransition stateTransition5,
                TState state
            )
            {
                return stateTransition5 switch
                {
                    Store<TState5, TAction5>.StateTransition.ByAction byAction =>
                        StateTransition.NewByAction(state, action5Generalizer(byAction.Action)),
                    _ => throw new NotImplementedException()
                };
            }
        }
    }
}
