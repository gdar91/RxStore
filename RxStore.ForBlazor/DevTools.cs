using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Reactive.Linq;

namespace RxStore
{
    public sealed class DevTools<TEvent, TState> : ReactiveComponentBase
    {
        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Inject]
        private Store<TEvent, TState> Store { get; set; }


        [Parameter]
        public string InstanceName { get; set; }


        public DevTools()
        {
            Initializes
                .Select(initialize =>
                    DevToolsConnection<TEvent, TState>.CreateObservable(
                        JsRuntime,
                        Store,
                        string.IsNullOrWhiteSpace(InstanceName)
                            ? nameof(RxStore) 
                            : InstanceName
                    )
                )
                .Switch()
                .TakeUntil(Disposes)
                .Subscribe();
        }
    }
}
