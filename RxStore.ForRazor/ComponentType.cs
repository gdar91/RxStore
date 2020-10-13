using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace RxStore
{
    internal interface IComponentType<TComponent> where TComponent : IComponent
    {
        void OpenComponent(int sequence, RenderTreeBuilder builder);
    }


    internal sealed class ComponentType<TComponent, TDestComponent> : IComponentType<TComponent>
        where TComponent : IComponent
        where TDestComponent : TComponent
    {
        public void OpenComponent(int sequence, RenderTreeBuilder builder)
        {
            builder.OpenComponent<TDestComponent>(sequence);
        }
    }


    public static class ComponentTypeServiceCollectionExtensions
    {
        public static IServiceCollection AddComponentTypeMapping<TComponent, TDestComponent>(
            this IServiceCollection services
        )
            where TComponent : IComponent
            where TDestComponent : TComponent
        {
            return services.AddSingleton<IComponentType<TComponent>>(
                provider => new ComponentType<TComponent, TDestComponent>()
            );
        }
    }
}
