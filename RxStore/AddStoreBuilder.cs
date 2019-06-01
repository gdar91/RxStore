using System;
using Microsoft.Extensions.DependencyInjection;

namespace RxStore
{
    public sealed class AddStoreBuilder<TState, TAction>
    {
        internal AddStoreBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; private set; }

        public AddStoreBuilder<TState, TAction> WithEffects(Type type)
        {
            Services.AddSingleton(typeof(IDeclareEffects<TState, TAction>), type);

            return this;
        }

        public AddStoreBuilder<TState, TAction> WithEffects<THasEffects>()
            where THasEffects : class, IDeclareEffects<TState, TAction>
        {
            Services.AddSingleton<IDeclareEffects<TState, TAction>, THasEffects>();

            return this;
        }

        public AddStoreBuilder<TState, TAction> WithEffects(
            Func<IServiceProvider, IDeclareEffects<TState, TAction>> implementationFactory
        )
        {
            Services.AddSingleton(implementationFactory);

            return this;
        }

        public AddStoreBuilder<TState, TAction> AddEffects<THasEffects>(THasEffects implementationInstance)
            where THasEffects : class, IDeclareEffects<TState, TAction>
        {
            Services.AddSingleton<IDeclareEffects<TState, TAction>>(implementationInstance);

            return this;
        }
    }
}
