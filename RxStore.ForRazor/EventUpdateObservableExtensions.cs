using Microsoft.FSharp.Core;
using System;
using System.Reactive.Linq;

namespace RxStore
{
    public static class EventUpdateObservableExtensions
    {
        public static IObservable<EventUpdate<TVersion, TState, TEvent>> AsEventUpdates<TVersion, TState, TEvent>(
            this IObservable<EventTransition<TVersion, TState, TEvent>> observable,
            TVersion availableVersion,
            Func<TVersion, TVersion, bool> versionIsSame,
            Func<TVersion, TVersion, bool> versionIsSuccessor
        )
        {
            return observable
                .Select((transition, index) =>
                    (index, availableVersion, transition.Version) switch
                    {
                        (0, var v0, var v1) when versionIsSame(v0, v1) =>
                            EventUpdate.OfUnit(transition),
                        (0, var v0, var v1) when versionIsSuccessor(v0, v1) =>
                            EventUpdate.OfEvent(transition),
                        (0, _, _) =>
                            EventUpdate.OfState(transition),
                        (_, _, _) =>
                            EventUpdate.OfEvent(transition)
                    }
                );
        }


        public static IObservable<EventUpdate<long, TState, TEvent>> AsEventUpdates<TState, TEvent>(
            this IObservable<EventTransition<long, TState, TEvent>> observable,
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


        public static IObservable<EventUpdate<DynamicVersion, TState, TEvent>> AsEventUpdates<TState, TEvent>(
            this IObservable<EventTransition<DynamicVersion, TState, TEvent>> observable,
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
            this IObservable<EventUpdate<TVersion, TState, TEvent>> observable,
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
                    (accumulator, element) => element.Value switch
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


        public static IObservable<Versioned<long, TState>> OfEventUpdates<TState, TEvent>(
            this IObservable<EventUpdate<long, TState, TEvent>> observable,
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


        public static IObservable<Versioned<DynamicVersion, TState>> OfEventUpdates<TState, TEvent>(
            this IObservable<EventUpdate<DynamicVersion, TState, TEvent>> observable,
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
