using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public abstract class EntityInfoOnline<TEvent> : IObservable<TEvent>
    {
        private readonly Subject<bool> requestsSubject;

        private readonly IObservable<TEvent> observable;


        public EntityInfoOnline()
        {
            requestsSubject = new Subject<bool>();

            observable =
                Observable
                    .Using(
                        () =>
                        {
                            requestsSubject.OnNext(true);

                            return Disposable.Create(() => requestsSubject.OnNext(false));
                        },
                        disposable => Observable.Never<TEvent>()
                    )
                    .Publish()
                    .RefCount();

            Handler = new InnerHandler(this);
        }


        public IHandler<Never, TEvent> Handler { get; }


        protected abstract IObservable<TEvent> Online();


        public IDisposable Subscribe(IObserver<TEvent> observer) => observable.Subscribe(observer);


        private sealed class InnerHandler : Handler<Never, TEvent>
        {
            private readonly EntityInfoOnline<TEvent> entityInfoOnline;


            public InnerHandler(EntityInfoOnline<TEvent> entityInfoOnline)
            {
                this.entityInfoOnline = entityInfoOnline;
            }


            protected override IEnumerable<IObservable<TEvent>> Setup(IObservable<Never> commands)
            {
                yield return entityInfoOnline.requestsSubject
                    .DistinctUntilChanged()
                    .Select(online =>
                        online
                            ? entityInfoOnline.Online()
                            : Observable.Empty<TEvent>()
                    )
                    .Switch();
            }
        }
    }
}
