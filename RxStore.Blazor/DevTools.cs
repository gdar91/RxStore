using System;
using System.Reactive;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RxStore
{
    public sealed class DevTools<TStore> : ComponentBase, IDisposable
        where TStore : Store
    {
        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Inject]
        private TStore Store { get; set; }


        private IDisposable Connection { get; set; }


        protected override void OnInitialized()
        {
            var observable = (IConnectableObservable<Unit>) typeof(DevToolsConnection<,,>)
                .MakeGenericType(new[] { typeof(TStore), Store.StateType, Store.ActionType })
                .GetMethod(nameof(DevToolsConnection<Store<int, int>, int, int>.CreateObservable))
                .Invoke(null, new object[] { JsRuntime, Store });

            Connection = observable.Connect();
        }

        public void Dispose()
        {
            using var resource = Connection;
        }
    }
}
