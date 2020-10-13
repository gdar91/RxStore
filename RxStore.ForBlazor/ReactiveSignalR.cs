using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.FSharp.Core;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace RxStore
{
    public static class ReactiveSignalR
    {
        public static IObservable<HubConnection> HubConnectionObservable(
            Func<HubConnection> hubConnectionFactory
        )
        {
            return Observable.Create<HubConnection>(
                async (observer, cancellationToken) =>
                {
                    var hubConnection = hubConnectionFactory();

                    hubConnection.Closed += nullableException =>
                    {
                        if (nullableException is Exception exception)
                        {
                            observer.OnError(exception);
                        }
                        else
                        {
                            observer.OnCompleted();
                        }

                        return Task.CompletedTask;
                    };

                    await hubConnection.StartAsync(cancellationToken);

                    observer.OnNext(hubConnection);

                    return () =>
                    {
                        try
                        {
                            hubConnection.DisposeAsync();
                        }
                        catch
                        { }
                    };
                }
            );
        }
    }
}
