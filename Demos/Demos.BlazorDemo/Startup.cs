using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Demos.BlazorDemo.State;
using RxStore;

namespace Demos.BlazorDemo
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStore<AppState, IAppAction>(AppState.Reducer, AppState.Initial)
                .WithEffects<AppEffects>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
