using Microsoft.Extensions.DependencyInjection;

namespace RxStore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStore<TStore>(
            this IServiceCollection services,
            bool withDevTools = false
        )
            where TStore : Store
        {
            services.AddSingleton<TStore>();

            if (withDevTools)
            {
                services.AddSingleton<IDevToolsConnection, DevToolsConnection<TStore>>();
            }

            return services;
        }
    }
}
