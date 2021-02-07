using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace RxStore
{
    public sealed class DispatchEntityOnlines : ReactiveComponentBase
    {
        [Inject]
        private IEnumerable<IEntityOnlineDispatcher> EntityOnlineDispatchers { get; set; }


        public DispatchEntityOnlines()
        {
            Initializes
                .Select(_ => EntityOnlineDispatchers.ToObservable())
                .Switch()
                .Merge()
                .TakeUntil(Disposes)
                .Publish()
                .AutoConnect(0);
        }
    }
}
