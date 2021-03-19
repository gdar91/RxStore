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




        public static IServiceCollection AddCommandInfoOnline<TCommand, TEvent, TCommandInfoOnline>(
            this IServiceCollection services,
            ServiceLifetime serviceLifetime
        )
            where TCommandInfoOnline : CommandInfoOnline<TCommand, TEvent>
        {
            services.Add(
                new ServiceDescriptor(
                    typeof(TCommandInfoOnline),
                    typeof(TCommandInfoOnline),
                    serviceLifetime
                )
            );

            services.Add(
                new ServiceDescriptor(
                    typeof(IEntityOnlineDispatcher),
                    typeof(EntityOnlineDispatcher<TEvent, TCommandInfoOnline>),
                    serviceLifetime
                )
            );

            return services;
        }


        public static IServiceCollection AddCommandSetOnline<TKey, TCommand, TEvent, TCommandSetOnline>(
            this IServiceCollection services,
            ServiceLifetime serviceLifetime
        )
            where TCommandSetOnline : CommandSetOnline<TKey, TCommand, TEvent>
        {
            services.Add(
                new ServiceDescriptor(
                    typeof(TCommandSetOnline),
                    typeof(TCommandSetOnline),
                    serviceLifetime
                )
            );

            services.Add(
                new ServiceDescriptor(
                    typeof(IEntityOnlineDispatcher),
                    typeof(EntityOnlineDispatcher<TEvent, TCommandSetOnline>),
                    serviceLifetime
                )
            );

            return services;
        }
    }
}
