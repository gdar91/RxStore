using System;
using System.Reactive;
using System.Reactive.Linq;

namespace RxStore
{
    public static class Operators
    {
        public static IObservable<TProjection> Project<TSource, TProjection>(
            this IObservable<TSource> source,
            Func<TSource, TProjection> selector
        )
            => source.Select(selector).DistinctUntilChanged();

        public static IObservable<TProjection> Project<TSource, TProjection>(
            this IObservable<TSource> source,
            Func<TSource, int, TProjection> selector
        )
            => source.Select(selector).DistinctUntilChanged();




        public static IObservable<TSource> ShareReplayLatest<TSource>(this IObservable<TSource> source) =>
            source.Replay(1).RefCount();




        public static IObservable<object> ElementsAsObjects<TElement>(
            this IObservable<TElement> source
        )
            => source.Select(next => next as object);


        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<object> source
        )
            => source.IgnoreElements().Cast<TAction>();


        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<Unit> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();

        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<bool> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();
        
        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<byte> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();

        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<sbyte> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();

        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<short> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();

        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<ushort> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();

        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<char> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();

        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<int> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();

        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<uint> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();

        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<long> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();

        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<ulong> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();

        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<float> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();

        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<double> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();

        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<decimal> source
        )
            => source.ElementsAsObjects().IgnoreElementsAs<TAction>();
    }
}
