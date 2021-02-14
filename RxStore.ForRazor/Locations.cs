using Fills;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Reactive.Linq;

namespace RxStore
{
    public sealed class Locations : IObservable<string>
    {
        private readonly IObservable<string> observable;


        public Locations(NavigationManager navigationManager)
        {
            observable =
                Observable
                    .FromEventPattern<LocationChangedEventArgs>(
                        handler => navigationManager.LocationChanged += handler,
                        handler => navigationManager.LocationChanged -= handler
                    )
                    .Select(eventPatter => eventPatter.EventArgs.Location)
                    .StartWith(() => navigationManager.Uri)
                    .Select(navigationManager.ToBaseRelativePath)
                    .DistinctUntilChanged()
                    .ReplayScoped(1)
                    .RefCount();
        }


        public IDisposable Subscribe(IObserver<string> observer) => observable.Subscribe(observer);
    }
}
