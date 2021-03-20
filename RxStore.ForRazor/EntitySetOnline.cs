﻿using Fills;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public abstract class EntitySetOnline<TKey, TEvent> : EntityOnline<TEvent>, IMemo<TKey, IObservable<Unit>>
    {
        private readonly Memo<TKey, IObservable<Unit>> byKey;


        protected EntitySetOnline()
        {
            byKey = new(key =>
                Observable
                    .Create<Unit>(_ =>
                    {
                        var whenOfflineSubject = new ReplaySubject<Unit>(1);
                        var whenOffline = whenOfflineSubject.AsObservable();

                        OnNextOutEventsObservable(WhenOnline(key, whenOffline));

                        return () => whenOfflineSubject.OnNext(Unit.Default);
                    })
                    .Publish()
                    .RefCount()
            );
        }


        protected abstract IObservable<TEvent> WhenOnline(TKey key, IObservable<Unit> whenOffline);


        public IObservable<Unit> this[TKey key] => byKey[key];
    }
}
