using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace RxStore
{
    public abstract class PureComponentContainer<TView, TCommand, TComponent> :
        PureComponentBase<TView, TCommand>
        where TComponent : PureComponentBase<TView, TCommand>
    {
        [Inject]
        internal IComponentType<TComponent> ComponentType { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            RenderFragment ChildContent(TView view) =>
                builder =>
                {
                    ComponentType.OpenComponent(0, builder);
                    builder.AddAttribute(1, nameof(View), view);
                    builder.AddAttribute(2, nameof(OnCommand), OnCommand);
                    builder.CloseComponent();
                };

            builder.OpenComponent<Subscribe<TView>>(0);
            builder.AddAttribute(1, nameof(Subscribe<TView>.To), Views);
            builder.AddAttribute(2, nameof(Subscribe<TView>.ChildContent), (RenderFragment<TView>) ChildContent);
            builder.CloseComponent();
        }
    }
}
