using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public abstract class Effects<TState, TAction> : IConnectableEffects
    {
        private readonly IConnectableObservable<TAction> effects;

        private IDisposable connection;


        protected Effects(Store<TState, TAction> store)
        {
            var fallback = Observable.Empty<TAction>();

            effects = GetEffects(store.Actions)
                .Select(effectsObservable => effectsObservable.Catch(fallback))
                .ToObservable()
                .Merge()
                .Do(store.Dispatch)
                .Publish();
        }


        public abstract IEnumerable<IObservable<TAction>> GetEffects(IObservable<TAction> actions);


        void IConnectable.Connect()
        {
            lock (this)
            {
                connection = connection ?? effects.Connect();
            }
        }

        public void Dispose()
        {
            using var resource = connection;
        }
    }
}
