using Fills;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public abstract class EntitySetOnline<TKey, TEvent>
    {
        private readonly Subject<(TKey Key, bool Online)> requestsSubject;

        private readonly Memo<TKey, IObservable<TEvent>> memo;


        public EntitySetOnline()
        {
            requestsSubject = new Subject<(TKey Key, bool Online)>();

            memo = new Memo<TKey, IObservable<TEvent>>(key =>
                Observable
                    .Using(
                        () =>
                        {
                            requestsSubject.OnNext((key, true));

                            return Disposable.Create(() =>
                                requestsSubject.OnNext((key, false))
                            );
                        },
                        disposable => Observable.Never<TEvent>()
                    )
                    .Publish()
                    .RefCount()
            );

            Handler = new InnerHandler(this);
        }


        public IObservable<TEvent> this[TKey key] => memo[key];


        public IHandler<Never, TEvent> Handler { get; }


        protected abstract IObservable<TEvent> Online(TKey key);


        private sealed class InnerHandler : Handler<Never, TEvent>
        {
            private readonly EntitySetOnline<TKey, TEvent> entitySetOnline;


            public InnerHandler(EntitySetOnline<TKey, TEvent> entitySetOnline)
            {
                this.entitySetOnline = entitySetOnline;
            }


            protected override IEnumerable<IObservable<TEvent>> Setup(IObservable<Never> commands)
            {
                yield return entitySetOnline.requestsSubject
                    .AsObservable()
                    .GroupBy(request => request.Key)
                        .SelectMany(group =>
                            group
                                .DistinctUntilChanged()
                                .Select(request =>
                                    request.Online
                                        ? entitySetOnline.Online(request.Key)
                                        : Observable.Empty<TEvent>()
                                )
                                .Switch()
                        );
            }
        }
    }
}
