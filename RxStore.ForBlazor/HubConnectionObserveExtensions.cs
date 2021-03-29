using Fills;
using Microsoft.AspNetCore.SignalR.Client;
using System;

namespace RxStore
{
    public static class HubConnectionObserveExtensions
    {
        private static IObservable<TResult> ObserveCore<TResult>(
            HubConnection hubConnection,
            string methodName,
            object[] args
        )
        {
            return FillsObservable.FromAsyncEnumerable(cancellationToken =>
                hubConnection.StreamAsyncCore<TResult>(
                    methodName,
                    args,
                    cancellationToken
                )
            );
        }


        public static IObservable<TResult> Observe<TResult>(
            this HubConnection hubConnection,
            string methodName
        )
        {
            return ObserveCore<TResult>(
                hubConnection,
                methodName,
                Array.Empty<object>()
            );
        }


        public static IObservable<TResult> Observe<TResult>(
            this HubConnection hubConnection,
            string methodName,
            object arg1
        )
        {
            return ObserveCore<TResult>(
                hubConnection,
                methodName,
                new[]
                {
                    arg1
                }
            );
        }


        public static IObservable<TResult> Observe<TResult>(
            this HubConnection hubConnection,
            string methodName,
            object arg1,
            object arg2
        )
        {
            return ObserveCore<TResult>(
                hubConnection,
                methodName,
                new[]
                {
                    arg1,
                    arg2
                }
            );
        }


        public static IObservable<TResult> Observe<TResult>(
            this HubConnection hubConnection,
            string methodName,
            object arg1,
            object arg2,
            object arg3
        )
        {
            return ObserveCore<TResult>(
                hubConnection,
                methodName,
                new[]
                {
                    arg1,
                    arg2,
                    arg3
                }
            );
        }


        public static IObservable<TResult> Observe<TResult>(
            this HubConnection hubConnection,
            string methodName,
            object arg1,
            object arg2,
            object arg3,
            object arg4
        )
        {
            return ObserveCore<TResult>(
                hubConnection,
                methodName,
                new[]
                {
                    arg1,
                    arg2,
                    arg3,
                    arg4
                }
            );
        }


        public static IObservable<TResult> Observe<TResult>(
            this HubConnection hubConnection,
            string methodName,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5
        )
        {
            return ObserveCore<TResult>(
                hubConnection,
                methodName,
                new[]
                {
                    arg1,
                    arg2,
                    arg3,
                    arg4,
                    arg5
                }
            );
        }


        public static IObservable<TResult> Observe<TResult>(
            this HubConnection hubConnection,
            string methodName,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5,
            object arg6
        )
        {
            return ObserveCore<TResult>(
                hubConnection,
                methodName,
                new[]
                {
                    arg1,
                    arg2,
                    arg3,
                    arg4,
                    arg5,
                    arg6
                }
            );
        }


        public static IObservable<TResult> Observe<TResult>(
            this HubConnection hubConnection,
            string methodName,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5,
            object arg6,
            object arg7
        )
        {
            return ObserveCore<TResult>(
                hubConnection,
                methodName,
                new[]
                {
                    arg1,
                    arg2,
                    arg3,
                    arg4,
                    arg5,
                    arg6,
                    arg7
                }
            );
        }


        public static IObservable<TResult> Observe<TResult>(
            this HubConnection hubConnection,
            string methodName,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5,
            object arg6,
            object arg7,
            object arg8
        )
        {
            return ObserveCore<TResult>(
                hubConnection,
                methodName,
                new[]
                {
                    arg1,
                    arg2,
                    arg3,
                    arg4,
                    arg5,
                    arg6,
                    arg7,
                    arg8
                }
            );
        }


        public static IObservable<TResult> Observe<TResult>(
            this HubConnection hubConnection,
            string methodName,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5,
            object arg6,
            object arg7,
            object arg8,
            object arg9
        )
        {
            return ObserveCore<TResult>(
                hubConnection,
                methodName,
                new[]
                {
                    arg1,
                    arg2,
                    arg3,
                    arg4,
                    arg5,
                    arg6,
                    arg7,
                    arg8,
                    arg9
                }
            );
        }


        public static IObservable<TResult> Observe<TResult>(
            this HubConnection hubConnection,
            string methodName,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5,
            object arg6,
            object arg7,
            object arg8,
            object arg9,
            object arg10
        )
        {
            return ObserveCore<TResult>(
                hubConnection,
                methodName,
                new[]
                {
                    arg1,
                    arg2,
                    arg3,
                    arg4,
                    arg5,
                    arg6,
                    arg7,
                    arg8,
                    arg9,
                    arg10
                }
            );
        }
    }
}
