using System;
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
        
        public static IObservable<TAction> IgnoreElementsAs<TAction>(
            this IObservable<object> source
        )
            => source.IgnoreElements().Cast<TAction>();
    }
}
