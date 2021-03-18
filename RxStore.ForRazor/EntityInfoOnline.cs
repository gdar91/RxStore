using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public abstract class EntityInfoOnline<TEvent> : EntityOnline<TEvent>, IObservable<Unit>
    {
        private readonly IObservable<Unit> observable;


        protected EntityInfoOnline()
        {
            observable =
                Observable
                    .Create<Unit>(observer =>
                    {
                        var whenOfflineSubject = new ReplaySubject<Unit>(1);
                        var whenOffline = whenOfflineSubject.AsObservable();

                        OnNextOutEventsObservable(WhenOnline(whenOffline));

                        return () => whenOfflineSubject.OnNext(Unit.Default);
                    })
                    .Publish()
                    .RefCount();
        }


        protected abstract IObservable<TEvent> WhenOnline(IObservable<Unit> whenOffline);


        public IDisposable Subscribe(IObserver<Unit> observer) => observable.Subscribe(observer);
    }
}
