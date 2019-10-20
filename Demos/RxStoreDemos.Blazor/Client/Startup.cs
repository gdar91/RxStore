using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using RxStore;
using RxStore.DevTools;
using RxStoreDemos.Blazor.Client.State;

namespace RxStoreDemos.Blazor.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStore<AppState, IAppAction>(AppState.Reducer, AppState.Initial, builder =>
            {
                builder.WithDevTools();
            });
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.Services.ConnectStore();

            app.AddComponent<App>("app");
        }
    }
}
