using Demos.BlazorFSharpDemo.State;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using RxStore;

namespace Demos.BlazorFSharpDemo.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStore<AppState, AppAction>(AppStateModule.reduce, AppStateModule.initial);
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
