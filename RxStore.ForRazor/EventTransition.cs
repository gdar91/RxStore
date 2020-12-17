namespace RxStore
{
    public record EventTransition<TVersion, TState, TEvent>(
        TVersion Version,
        TState State,
        TEvent Event
    );


    public static class EventTransition
    {
        public static EventTransition<TVersion, TState, TEvent> Of<TVersion, TState, TEvent>(
            TVersion version,
            TState state,
            TEvent @event
        )
        {
            return new EventTransition<TVersion, TState, TEvent>(version, state, @event);
        }
    }
}
