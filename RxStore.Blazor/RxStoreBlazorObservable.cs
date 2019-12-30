using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace RxStore
{
    public static class RxStoreBlazorObservable
    {
        public static IObservable<TElement> FromSignalRHubConnection<TElement>(
            Func<IObserver<TElement>, HubConnection> hubConnectionFactory
        )
        {
            return Observable.Create<TElement>(async (observer, cancellationToken) =>
            {
                var hubConnection = hubConnectionFactory(observer);

                hubConnection.Closed += e =>
                {
                    if (e != null)
                    {
                        observer.OnError(e);
                    }

                    return Task.CompletedTask;
                };

                await hubConnection.StartAsync(cancellationToken);

                return () => hubConnection.DisposeAsync();
            });
        }
    }
}
