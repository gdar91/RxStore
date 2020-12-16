using Microsoft.FSharp.Core;
using System;
using System.Reactive.Linq;

namespace RxStore
{
    public enum EventUpdateAction
    {
        Stay,
        Advance,
        RaiseMismatch
    }

    public static class EventUpdatesObservableExtensions
    {
        public static IObservable<Versioned<TVersion, TState>> EventUpdates<TVersion, TState, TEvent>(
            this IObservable<Versioned<TVersion, FSharpChoice<TState, TEvent, Unit>>> observable,
            TState initialState,
            Func<TState, TEvent, TState> reducer,
            TVersion initialVersion,
            Func<TVersion, TVersion, EventUpdateAction> stateVersionsAction,
            Func<TVersion, TVersion, EventUpdateAction> eventVersionsAction
        )
        {
            return observable
                .Scan(
                    (Versioned.OfValues(initialVersion, initialState), true),
                    (accumulator, element) => element.Item switch
                    {
                        FSharpChoice<TState, TEvent, Unit>.Choice1Of3 { Item: var state } =>
                            stateVersionsAction(accumulator.Item1.Version, element.Version) switch
                            {
                                EventUpdateAction.Stay =>
                                    (accumulator.Item1, false),
                                EventUpdateAction.Advance =>
                                    (Versioned.OfValues(element.Version, state), true),
                                EventUpdateAction.RaiseMismatch =>
                                    throw new Exception("Version mismatch."),
                                var other =>
                                    throw new Exception($"Impossible enum element {other}.")
                            },
                        FSharpChoice<TState, TEvent, Unit>.Choice2Of3 { Item: var @event } =>
                            eventVersionsAction(accumulator.Item1.Version, element.Version) switch
                            {
                                EventUpdateAction.Stay =>
                                    (accumulator.Item1, false),
                                EventUpdateAction.Advance =>
                                    (
                                        Versioned.OfValues(
                                            element.Version,
                                            reducer(accumulator.Item1.Item, @event)
                                        ),
                                        true
                                    ),
                                EventUpdateAction.RaiseMismatch =>
                                    throw new Exception("Version mismatch."),
                                var other =>
                                    throw new Exception($"Impossible enum element {other}.")
                            },
                        FSharpChoice<TState, TEvent, Unit>.Choice3Of3 _ =>
                            accumulator,
                        var other => throw new Exception($"Impossible union case {other}.")
                    }
                )
                .Where(accumulator => accumulator.Item2)
                .Select(accumulator => accumulator.Item1);
        }


        public static IObservable<Versioned<long, TState>> EventUpdates<TState, TEvent>(
            this IObservable<Versioned<long, FSharpChoice<TState, TEvent, Unit>>> observable,
            TState initialState,
            Func<TState, TEvent, TState> reducer,
            long initialVersion = -1L
        )
        {
            return EventUpdates(
                observable,
                initialState,
                reducer,
                initialVersion,
                (versionA, versionB) => (versionA, versionB) switch
                {
                    var (a, b) when a >= b => EventUpdateAction.Stay,
                    var (a, b) => EventUpdateAction.Advance
                },
                (versionA, versionB) => (versionA, versionB) switch
                {
                    var (a, b) when a >= b => EventUpdateAction.Stay,
                    var (a, b) when a + 1L == b => EventUpdateAction.Advance,
                    var (a, b) => EventUpdateAction.RaiseMismatch
                }
            );
        }


        public static IObservable<Versioned<DynamicVersion, TState>> EventUpdates<TState, TEvent>(
            this IObservable<Versioned<DynamicVersion, FSharpChoice<TState, TEvent, Unit>>> observable,
            TState initialState,
            Func<TState, TEvent, TState> reducer,
            DynamicVersion initialVersion,
            long zeroVersionValue = -1L
        )
        {
            return EventUpdates(
                observable,
                initialState,
                reducer,
                initialVersion,
                (version0, version1) => (version0, version1) switch
                {
                    var (a, b) when a.Lifeline < b.Lifeline =>
                        EventUpdateAction.Advance,
                    var (a, b) when
                            a.Lifeline == b.Lifeline &&
                            a.Value < b.Value =>
                        EventUpdateAction.Advance,
                    var (a, b) =>
                        EventUpdateAction.Stay
                },
                (version0, version1) => (version0, version1) switch
                {
                    var (a, b) when
                            a.Lifeline == b.Lifeline &&
                            a.Value + 1L == b.Value =>
                        EventUpdateAction.Advance,
                    var (a, b) when
                            a.Lifeline == b.Lifeline &&
                            a.Value < b.Value =>
                        EventUpdateAction.RaiseMismatch,
                    var (a, b) when a.Lifeline >= b.Lifeline =>
                        EventUpdateAction.Stay,
                    var (a, b) when
                            a.Lifeline == DateTimeOffset.MinValue &&
                            a.Value == zeroVersionValue &&
                            b.Value == a.Value + 1L =>
                        EventUpdateAction.Advance,
                    var (a, b) =>
                        EventUpdateAction.RaiseMismatch
                }
            );
        }
    }
}
