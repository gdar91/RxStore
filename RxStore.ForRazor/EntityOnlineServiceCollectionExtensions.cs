using Microsoft.Extensions.DependencyInjection;

namespace RxStore
{
    public static class EntityOnlineServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityInfoOnline<TEvent, TEntityInfoOnline>(
            this IServiceCollection services,
            ServiceLifetime serviceLifetime
        )
            where TEntityInfoOnline : EntityInfoOnline<TEvent>
        {
            services.Add(
                new ServiceDescriptor(
                    typeof(TEntityInfoOnline),
                    typeof(TEntityInfoOnline),
                    serviceLifetime
                )
            );

            services.Add(
                new ServiceDescriptor(
                    typeof(IEntityOnlineDispatcher),
                    typeof(EntityOnlineDispatcher<TEvent, TEntityInfoOnline>),
                    serviceLifetime
                )
            );

            return services;
        }


        public static IServiceCollection AddEntitySetOnline<TKey, TEvent, TEntitySetOnline>(
            this IServiceCollection services,
            ServiceLifetime serviceLifetime
        )
            where TEntitySetOnline : EntitySetOnline<TKey, TEvent>
        {
            services.Add(
                new ServiceDescriptor(
                    typeof(TEntitySetOnline),
                    typeof(TEntitySetOnline),
                    serviceLifetime
                )
            );

            services.Add(
                new ServiceDescriptor(
                    typeof(IEntityOnlineDispatcher),
                    typeof(EntityOnlineDispatcher<TEvent, TEntitySetOnline>),
                    serviceLifetime
                )
            );

            return services;
        }
    }
}
