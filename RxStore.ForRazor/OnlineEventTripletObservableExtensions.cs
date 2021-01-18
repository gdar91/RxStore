using Microsoft.FSharp.Core;
using System;
using System.Reactive.Linq;

namespace RxStore
{
    public static class OnlineEventTripletObservableExtensions
    {
        public static IObservable<TEvent> OnlineEventTriplet<TSignal, TResult, TError, TEvent>(
            this IObservable<TSignal> signals,
            Func<TSignal, IObservable<TResult>> resultsSelector,
            Func<Stamp<Unit>, TEvent> onStart,
            Func<Stamp<TResult>, TEvent> onSuccess,
            Func<Exception, TError> errorSelector,
            Func<Stamp<TError>, TEvent> onError
        )
        {
            return signals
                .Select(command =>
                    resultsSelector(command)
                        .Timestamp()
                        .Select(timestamped => onSuccess(timestamped.AsStamp()))
                        .Let(observable =>
                            onError == null
                                ? observable
                                : observable.Catch<TEvent, Exception>(e =>
                                    Observable
                                        .Return(errorSelector(e))
                                        .Timestamp()
                                        .Select(timestamped => onError(timestamped.AsStamp()))
                                )
                        )
                        .Let(observable =>
                            Observable.Concat(
                                Observable
                                    .Return(default(Unit))
                                    .Timestamp()
                                    .Select(timestamped => onStart(timestamped.AsStamp())),
                                observable
                            )
                        )
                )
                .Switch();
        }




        public static IObservable<TEvent> OnlineEventTriplet<TSignal, TResult, TError, TEvent>(
            this IObservable<TSignal> signals,
            Func<TSignal, IObservable<TResult>> resultsSelector,
            TEvent onStart,
            Func<Stamp<TResult>, TEvent> onSuccess,
            Func<Exception, TError> errorSelector,
            Func<Stamp<TError>, TEvent> onError
        )
        {
            return OnlineEventTriplet(
                signals,
                resultsSelector,
                _ => onStart,
                onSuccess,
                errorSelector,
                onError
            );
        }




        public static IObservable<TEvent> OnlineEventTriplet<TSignal, TResult, TEvent>(
            this IObservable<TSignal> signals,
            Func<TSignal, IObservable<TResult>> resultsSelector,
            Func<Stamp<Unit>, TEvent> onStart,
            Func<Stamp<TResult>, TEvent> onSuccess
        )
        {
            return OnlineEventTriplet(
                signals,
                resultsSelector,
                onStart,
                onSuccess,
                e => e,
                null
            );
        }




        public static IObservable<TEvent> OnlineEventTriplet<TSignal, TResult, TEvent>(
            this IObservable<TSignal> signals,
            Func<TSignal, IObservable<TResult>> resultsSelector,
            TEvent onStart,
            Func<Stamp<TResult>, TEvent> onSuccess
        )
        {
            return OnlineEventTriplet(
                signals,
                resultsSelector,
                _ => onStart,
                onSuccess,
                e => e,
                null
            );
        }




        public static IObservable<TEvent> OnlineEventTriplet<TSignal, TResult, TEvent>(
            this IObservable<TSignal> signals,
            Func<TSignal, IObservable<TResult>> resultsSelector,
            Func<Stamp<Unit>, TEvent> onStart,
            Func<Stamp<TResult>, TEvent> onSuccess,
            Func<Stamp<string>, TEvent> onError
        )
        {
            return OnlineEventTriplet(
                signals,
                resultsSelector,
                onStart,
                onSuccess,
                e => e.Message,
                onError
            );
        }




        public static IObservable<TEvent> OnlineEventTriplet<TEvent, TSignal, TResult>(
            this IObservable<TSignal> commands,
            Func<TSignal, IObservable<TResult>> resultsSelector,
            TEvent onStart,
            Func<Stamp<TResult>, TEvent> onSuccess,
            Func<Stamp<string>, TEvent> onError
        )
        {
            return OnlineEventTriplet(
                commands,
                resultsSelector,
                _ => onStart,
                onSuccess,
                e => e.Message,
                onError
            );
        }
    }
}
