using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace RxStore
{
    public sealed class EffectsDispatcher<TState, TAction> : IEffectsDispatcher
    {
        private readonly IDispatcher<TState, TAction> dispatcher;

        private readonly IEnumerable<IEffects<TState, TAction>> effectDeclarations;

        private IDisposable connection;


        public EffectsDispatcher(
            IDispatcher<TState, TAction> dispatcher,
            IEnumerable<IEffects<TState, TAction>> effectDeclarations
        )
        {
            this.dispatcher = dispatcher;
            this.effectDeclarations = effectDeclarations;
        }


        public void Initialize()
        {
            lock (this)
            {
                if (connection != null)
                {
                    return;
                }

                var fallback = Observable.Empty<TAction>();

                var effects = effectDeclarations
                    .Select(effectDeclaration => effectDeclaration.GetEffects())
                    .Where(effects => effects != null)
                    .SelectMany(allEffects => allEffects)
                    .Select(effects => effects.Catch(fallback))
                    .ToObservable()
                    .Merge()
                    .Do(dispatcher.Dispatch)
                    .Publish();
                
                connection = effects.Connect();
            }
        }

        public void Dispose()
        {
            using var resource = connection;
        }
    }
}
