using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.FSharp.Core;
using RxStore.Entity;

namespace RxStore
{
    public static class RxStoreExtensions
    {
        public static Stamp<T> AsStamp<T>(this Timestamped<T> timestamped) =>
            Stamp.OfValues(timestamped.Timestamp, timestamped.Value);


        public static IObservable<T> Observe<T>(
            this HubConnection hubConnection,
            string methodName,
            params object[] args
        )
        {
            return Observable.Create<T>(async (observer, cancellationToken) =>
            {
                var asyncEnumerable = hubConnection.StreamAsyncCore<T>(
                    methodName,
                    args,
                    cancellationToken
                );

                await foreach (var item in asyncEnumerable)
                {
                    observer.OnNext(item);
                }

                observer.OnCompleted();

                return Disposable.Empty;
            });
        }


        public static IObservable<FSharpOption<HubConnection>> Observe(
            this IHubConnectionBuilder hubConnectionBuilder
        )
        {
            return Observable.Create<FSharpOption<HubConnection>>(
                async (observer, cancellationToken) =>
                {
                    HubConnection hubConnection = default;

                    try
                    {
                        observer.OnNext(FSharpOption<HubConnection>.None);

                        hubConnection = hubConnectionBuilder.Build();

                        hubConnection.Closed += exception =>
                        {
                            if (exception is Exception e)
                            {
                                observer.OnError(e);
                            }

                            return Task.CompletedTask;
                        };

                        await hubConnection.StartAsync(cancellationToken);

                        observer.OnNext(FSharpOption<HubConnection>.Some(hubConnection));
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await hubConnection?.DisposeAsync();
                        }
                        catch
                        {
                        }

                        observer.OnError(e);
                    }

                    return () =>
                    {
                        try
                        {
                            hubConnection?.DisposeAsync();
                        }
                        catch
                        {
                        }
                    };
                }
            );
        }
    }
}
