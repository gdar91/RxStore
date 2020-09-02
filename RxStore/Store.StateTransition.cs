using System;
using System.Collections.Generic;

namespace RxStore
{
    partial class Store<TEvent, TState>
    {
        internal abstract class StateTransition
        {
            private StateTransition()
            { }




            public sealed class Initial : StateTransition, IEquatable<Initial>
            {
                public Initial(TState state)
                {
                    State = state;
                }

                public TState State { get; }


                #region Equality

                public override bool Equals(object obj)
                {
                    return Equals(obj as Initial);
                }

                public bool Equals(Initial other)
                {
                    return other != null &&
                           EqualityComparer<TState>.Default.Equals(State, other.State);
                }

                public override int GetHashCode()
                {
                    return -1319491066 + EqualityComparer<TState>.Default.GetHashCode(State);
                }

                public static bool operator ==(Initial left, Initial right)
                {
                    return EqualityComparer<Initial>.Default.Equals(left, right);
                }

                public static bool operator !=(Initial left, Initial right)
                {
                    return !(left == right);
                }

                #endregion
            }


            public static StateTransition NewInitial(TState state) =>
                new Initial(state);




            public sealed class ByEvent : StateTransition, IEquatable<ByEvent>
            {
                public ByEvent(TState state, TEvent @event)
                {
                    State = state;
                    Event = @event;
                }

                public TState State { get; }

                public TEvent Event { get; }


                #region Equality

                public override bool Equals(object obj)
                {
                    return Equals(obj as ByEvent);
                }

                public bool Equals(ByEvent other)
                {
                    return other != null &&
                           EqualityComparer<TState>.Default.Equals(State, other.State) &&
                           EqualityComparer<TEvent>.Default.Equals(Event, other.Event);
                }

                public override int GetHashCode()
                {
                    int hashCode = -2121958075;
                    hashCode = hashCode * -1521134295 + EqualityComparer<TState>.Default.GetHashCode(State);
                    hashCode = hashCode * -1521134295 + EqualityComparer<TEvent>.Default.GetHashCode(Event);
                    return hashCode;
                }

                public static bool operator ==(ByEvent left, ByEvent right)
                {
                    return EqualityComparer<ByEvent>.Default.Equals(left, right);
                }

                public static bool operator !=(ByEvent left, ByEvent right)
                {
                    return !(left == right);
                }

                #endregion
            }


            public static StateTransition NewByEvent(TState state, TEvent @event) =>
                new ByEvent(state, @event);




            public static TState StateOf(StateTransition stateTransition) => stateTransition switch
            {
                Initial initialStateTransition => initialStateTransition.State,
                ByEvent byEventStateTransition => byEventStateTransition.State,
                _ => throw new Exception()
            };


            public static Func<StateTransition, TEvent, StateTransition> LiftReducer(
                Func<TState, TEvent, TState> reducer
            )
            {
                StateTransition Reducer(StateTransition stateTransition, TEvent @event) =>
                    NewByEvent(
                        reducer(StateOf(stateTransition), @event),
                        @event
                    );

                return Reducer;
            }
        }
    }
}
