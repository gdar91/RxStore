using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace RxStore
{
    public interface IEffectsDispatcher : IDisposable
    {
        void Initialize();
    }


    public sealed class EffectsDispatcher<TState, TAction> : IEffectsDispatcher
    {
        private readonly IStore<TState, TAction> stateManager;

        private readonly IEnumerable<IEffects<TState, TAction>> effectDeclarations;

        private IDisposable connection;


        public EffectsDispatcher(
            IStore<TState, TAction> stateManager,
            IEnumerable<IEffects<TState, TAction>> effectDeclarations
        )
        {
            this.stateManager = stateManager;
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
                    .Do(stateManager.Dispatch)
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
