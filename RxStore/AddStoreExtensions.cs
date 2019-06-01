using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace RxStore
{
    public static class AddStoreExtensions
    {
        public static AddStoreBuilder<TState, TAction> AddStore<TState, TAction>(
            this IServiceCollection services,
            Func<TState, TAction, TState> reducer,
            TState initialState
        )
        {
            services.AddSingleton(provider =>
                new Store<TState, TAction>(
                    reducer,
                    initialState,
                    provider.GetService<IEnumerable<IDeclareEffects<TState, TAction>>>()
                )
            );

            var addStore = new AddStoreBuilder<TState, TAction>(services);

            return addStore;
        }
    }
}
