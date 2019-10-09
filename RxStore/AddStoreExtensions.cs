using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RxStore
{
    public static class AddStoreExtensions
    {
        public static IServiceCollection AddStore<TState, TAction>(
            this IServiceCollection services,
            Func<TState, TAction, TState> reducer,
            TState initialState,
            Action<AddStoreBuilder<TState, TAction>> builderAction = null
        )
        {
            services.AddSingleton(provider => new Store<TState, TAction>(reducer, initialState));

            services.AddSingleton<IStore<TState, TAction>>(
                provider => provider.GetRequiredService<Store<TState, TAction>>()
            );


            services.AddSingleton<IEffectsDispatcher, EffectsDispatcher<TState, TAction>>();

            services.TryAddSingleton<EffectsInitializer>();


            if (builderAction != null)
            {
                builderAction(new AddStoreBuilder<TState, TAction>(services));
            }


            return services;
        }
    }
}
