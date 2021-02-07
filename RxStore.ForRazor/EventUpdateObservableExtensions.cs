using System;
using System.Reactive.Linq;

namespace RxStore
{
    public static class EventUpdateObservableExtensions
    {
        public static IObservable<Versioned<TVersion, EventUpdate<TState, TEvent>>> AsEventUpdates<TVersion, TState, TEvent>(
            this IObservable<Versioned<TVersion, EventTransition<TState, TEvent>>> observable,
            TVersion availableVersion,
            Func<TVersion, TVersion, bool> versionIsSame,
            Func<TVersion, TVersion, bool> versionIsSuccessor
        )
        {
            return observable
                .Select((versioned, index) =>
                    Versioned.OfValues(
                        versioned.Version,
                        (index, availableVersion, versioned.Version) switch
                        {
                            (0, var v0, var v1) when versionIsSame(v0, v1) =>
                                EventTransition.AsUpdateConfirmation(versioned.Item),
                            (0, var v0, var v1) when versionIsSuccessor(v0, v1) =>
                                EventTransition.AsUpdateEvent(versioned.Item),
                            (0, _, _) =>
                                EventTransition.AsUpdateState(versioned.Item),
                            (_, _, _) =>
                                EventTransition.AsUpdateEvent(versioned.Item)
                        }
                    )
                );
        }


        public static IObservable<Versioned<long, EventUpdate<TState, TEvent>>> AsEventUpdates<TState, TEvent>(
            this IObservable<Versioned<long, EventTransition<TState, TEvent>>> observable,
            long availableVersion
        )
        {
            return AsEventUpdates(
                observable,
                availableVersion,
                (version0, version1) => version0 == version1,
                (version0, version1) => version0 + 1L == version1
            );
        }


        public static IObservable<Versioned<DynamicVersion, EventUpdate<TState, TEvent>>> AsEventUpdates<TState, TEvent>(
            this IObservable<Versioned<DynamicVersion, EventTransition<TState, TEvent>>> observable,
            DynamicVersion availableVersion,
            long zeroVersionValue = -1L
        )
        {
            return AsEventUpdates(
                observable,
                availableVersion,
                (version0, version1) => object.Equals(version0, version1),
                (version0, version1) => (version0, version1) switch
                {
                    (var v0, var v1) when
                            v0.Lifeline == v1.Lifeline &&
                            v0.Value + 1L == v1.Value =>
                        true,
                    (var v0, var v1) when
                            v0.Lifeline == DateTimeOffset.MinValue &&
                            v0.Value == zeroVersionValue &&
                            v0.Value + 1L == v1.Value =>
                        true,
                    (_, _) =>
                        false
                }
            );
        }


        public static IObservable<Versioned<TVersion, TState>> OfEventUpdates<TVersion, TState, TEvent>(
            this IObservable<Versioned<TVersion, EventUpdate<TState, TEvent>>> observable,
            TState initialState,
            Func<TState, TEvent, TState> reducer,
            TVersion initialVersion,
            Func<TVersion, TVersion, EventUpdate.Action> stateVersionsAction,
            Func<TVersion, TVersion, EventUpdate.Action> eventVersionsAction
        )
        {
            return observable
                .Scan(
                    (Versioned.OfValues(initialVersion, initialState), true),
                    (accumulator, element) => element.Item switch
                    {
                        EventUpdate<TState, TEvent>.State { Item: var state } =>
                            stateVersionsAction(accumulator.Item1.Version, element.Version) switch
                            {
                                var action when action.IsStay =>
                                    (accumulator.Item1, false),
                                var action when action.IsAdvance =>
                                    (Versioned.OfValues(element.Version, state), true),
                                var action when action.IsRaiseMismatch =>
                                    throw new Exception("Version mismatch."),
                                var action =>
                                    throw new Exception($"Impossible enum element {action}.")
                            },
                        EventUpdate<TState, TEvent>.Event { Item: var @event } =>
                            eventVersionsAction(accumulator.Item1.Version, element.Version) switch
                            {
                                var action when action.IsStay =>
                                    (accumulator.Item1, false),
                                var action when action.IsAdvance =>
                                    (
                                        Versioned.OfValues(
                                            element.Version,
                                            reducer(accumulator.Item1.Item, @event)
                                        ),
                                        true
                                    ),
                                var action when action.IsRaiseMismatch =>
                                    throw new Exception("Version mismatch."),
                                var action =>
                                    throw new Exception($"Impossible enum element {action}.")
                            },
                        var eventUpdate when eventUpdate.IsConfirmation =>
                            accumulator,
                        var eventUpdate =>
                            throw new Exception($"Impossible union case {eventUpdate}.")
                    }
                )
                .Where(accumulator => accumulator.Item2)
                .Select(accumulator => accumulator.Item1);
        }


        public static IObservable<Versioned<long, TState>> OfEventUpdates<TState, TEvent>(
            this IObservable<Versioned<long, EventUpdate<TState, TEvent>>> observable,
            TState initialState,
            Func<TState, TEvent, TState> reducer,
            long initialVersion = -1L
        )
        {
            return OfEventUpdates(
                observable,
                initialState,
                reducer,
                initialVersion,
                (versionA, versionB) => (versionA, versionB) switch
                {
                    var (a, b) when a >= b => EventUpdate.Action.Stay,
                    var (a, b) => EventUpdate.Action.Advance
                },
                (versionA, versionB) => (versionA, versionB) switch
                {
                    var (a, b) when a >= b => EventUpdate.Action.Stay,
                    var (a, b) when a + 1L == b => EventUpdate.Action.Advance,
                    var (a, b) => EventUpdate.Action.RaiseMismatch
                }
            );
        }


        public static IObservable<Versioned<DynamicVersion, TState>> OfEventUpdates<TState, TEvent>(
            this IObservable<Versioned<DynamicVersion, EventUpdate<TState, TEvent>>> observable,
            TState initialState,
            Func<TState, TEvent, TState> reducer,
            DynamicVersion initialVersion,
            long zeroVersionValue = -1L
        )
        {
            return OfEventUpdates(
                observable,
                initialState,
                reducer,
                initialVersion,
                (version0, version1) => (version0, version1) switch
                {
                    var (a, b) when a.Lifeline < b.Lifeline =>
                        EventUpdate.Action.Advance,
                    var (a, b) when
                            a.Lifeline == b.Lifeline &&
                            a.Value < b.Value =>
                        EventUpdate.Action.Advance,
                    var (a, b) =>
                        EventUpdate.Action.Stay
                },
                (version0, version1) => (version0, version1) switch
                {
                    var (a, b) when
                            a.Lifeline == b.Lifeline &&
                            a.Value + 1L == b.Value =>
                        EventUpdate.Action.Advance,
                    var (a, b) when
                            a.Lifeline == b.Lifeline &&
                            a.Value < b.Value =>
                        EventUpdate.Action.RaiseMismatch,
                    var (a, b) when a.Lifeline >= b.Lifeline =>
                        EventUpdate.Action.Stay,
                    var (a, b) when
                            a.Lifeline == DateTimeOffset.MinValue &&
                            a.Value == zeroVersionValue &&
                            b.Value == a.Value + 1L =>
                        EventUpdate.Action.Advance,
                    var (a, b) =>
                        EventUpdate.Action.RaiseMismatch
                }
            );
        }
    }
}
