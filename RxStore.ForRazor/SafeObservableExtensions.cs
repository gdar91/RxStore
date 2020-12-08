using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace RxStore
{
    public static class SafeObservableExtensions
    {
        public static IObservable<TElement> DelaySafe<TElement>(
            this IObservable<TElement> observable,
            TimeSpan timeSpan
        )
        {
            return observable
                .Select(element =>
                    Observable.FromAsync(async cancellationToken =>
                    {
                        await Task.Delay(timeSpan, cancellationToken);

                        return element;
                    })
                )
                .Concat();
        }
    }
}
