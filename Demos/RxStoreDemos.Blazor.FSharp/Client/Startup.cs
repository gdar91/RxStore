using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using RxStore;
using RxStore.DevTools;
using RxStoreDemos.Blazor.FSharp.State;

namespace RxStoreDemos.Blazor.FSharp.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStore<AppState, AppAction>(
                AppStateModule.reducer,
                AppStateModule.initial,
                builder =>
                {
                    builder.WithDevTools();
                }
            );
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.Services.ConnectStore();

            app.AddComponent<App>("app");
        }
    }
}
