using Microsoft.FSharp.Core;

namespace RxStore
{
    public record EventUpdate<TVersion, TState, TEvent>(
        TVersion Version,
        FSharpChoice<TState, TEvent, Unit> Value
    );


    public enum EventUpdateAction
    {
        Stay,
        Advance,
        RaiseMismatch
    }


    public static class EventUpdate
    {
        public static EventUpdate<TVersion, TState, TEvent> OfState<TVersion, TState, TEvent>(
            EventTransition<TVersion, TState, TEvent> eventTransition
        )
        {
            return new EventUpdate<TVersion, TState, TEvent>(
                eventTransition.Version,
                FSharpChoice<TState, TEvent, Unit>.NewChoice1Of3(eventTransition.State)
            );
        }


        public static EventUpdate<TVersion, TState, TEvent> OfEvent<TVersion, TState, TEvent>(
            EventTransition<TVersion, TState, TEvent> eventTransition
        )
        {
            return new EventUpdate<TVersion, TState, TEvent>(
                eventTransition.Version,
                FSharpChoice<TState, TEvent, Unit>.NewChoice2Of3(eventTransition.Event)
            );
        }


        public static EventUpdate<TVersion, TState, TEvent> OfUnit<TVersion, TState, TEvent>(
            EventTransition<TVersion, TState, TEvent> eventTransition
        )
        {
            return new EventUpdate<TVersion, TState, TEvent>(
                eventTransition.Version,
                FSharpChoice<TState, TEvent, Unit>.NewChoice3Of3(default)
            );
        }
    }
}
