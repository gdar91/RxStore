using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    internal interface IEffectsDispatcher : IDisposable
    {
        void Connect();
    }

    internal sealed class EffectsDispatcher<TState, TAction> : IEffectsDispatcher
    {
        private readonly IConnectableObservable<TAction> dispatches;

        private IDisposable connection;

        public EffectsDispatcher(IEffects<TState, TAction> effects, Store<TState, TAction> store)
        {
            var fallback = Observable.Empty<TAction>();

            dispatches = effects.GetEffects(store.Actions)
                .Select(effectsObservable => effectsObservable.Catch(fallback))
                .ToObservable()
                .Merge()
                .Do(store.Dispatch)
                .Publish();
        }

        public void Connect()
        {
            lock (this)
            {
                connection = connection ?? dispatches.Connect();
            }
        }

        public void Dispose()
        {
            using var resource = connection;
        }
    }
}
