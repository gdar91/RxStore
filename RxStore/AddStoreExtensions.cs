using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddSingleton(provider =>
                new SourceStore<TState, TAction>(reducer, initialState)
            );

            services.AddSingleton<Store<TState, TAction>>(provider =>
                provider.GetRequiredService<SourceStore<TState, TAction>>()
            );

            services.AddSingleton<IConnectableStore>(provider =>
                provider.GetRequiredService<SourceStore<TState, TAction>>()
            );


            if (builderAction != null)
            {
                builderAction(new AddStoreBuilder<TState, TAction>(services));
            }


            return services;
        }

        public static void ConnectStore(this IServiceProvider serviceProvider)
        {
            var connectables = Enumerable.Concat<IConnectable>(
                serviceProvider.GetServices<IConnectableEffects>(),
                serviceProvider.GetServices<IConnectableStore>()
            );

            foreach (var connectable in connectables)
            {
                connectable.Connect();
            }
        }
    }
}
