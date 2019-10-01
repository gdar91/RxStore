using System;
using Microsoft.Extensions.DependencyInjection;

namespace RxStore
{
    public sealed class AddStoreBuilder<TState, TAction>
    {
        public readonly IServiceCollection services;


        internal AddStoreBuilder(IServiceCollection services)
        {
            this.services = services;
        }


        public AddStoreBuilder<TState, TAction> WithEffects(Type type)
        {
            services.AddSingleton(typeof(IEffects<TState, TAction>), type);

            return this;
        }

        public AddStoreBuilder<TState, TAction> WithEffects<TDeclaration>()
            where TDeclaration : class, IEffects<TState, TAction>
        {
            services.AddSingleton<IEffects<TState, TAction>, TDeclaration>();

            return this;
        }

        public AddStoreBuilder<TState, TAction> WithEffects(
            Func<IServiceProvider, IEffects<TState, TAction>> implementationFactory
        )
        {
            services.AddSingleton(implementationFactory);

            return this;
        }


        public AddStoreBuilder<TState, TAction> WithProjection<TStateProjection, TActionProjection>(
            Func<TState, TStateProjection> stateProjector,
            Func<TAction, ValueTuple<bool, TActionProjection>> actionChooser,
            Func<TActionProjection, TAction> actionGeneralizer,
            Action<AddStoreBuilder<TStateProjection, TActionProjection>> buildAction = null
        )
        {
            services.AddSingleton<IState<TStateProjection, TActionProjection>>(provider =>
            {
                var state = provider.GetRequiredService<IState<TState, TAction>>();
                
                var stateProjection =
                    new StateProjection<TState, TAction, TStateProjection, TActionProjection>(
                        state,
                        stateProjector
                    );

                return stateProjection;
            });


            services.AddSingleton<IActions<TStateProjection, TActionProjection>>(provider =>
            {
                var actions = provider.GetRequiredService<IActions<TState, TAction>>();
                
                var actionsProjection =
                    new ActionsProjection<TState, TAction, TStateProjection, TActionProjection>(
                        actions,
                        actionChooser
                    );

                return actionsProjection;
            });


            services.AddSingleton<IDispatcher<TStateProjection, TActionProjection>>(provider =>
            {
                var dispatcher = provider.GetRequiredService<IDispatcher<TState, TAction>>();

                var dispatcherProjection =
                    new DispatcherProjection<TState, TAction, TStateProjection, TActionProjection>(
                        dispatcher,
                        actionGeneralizer
                    );

                return dispatcherProjection;
            });


            services
                .AddSingleton<IEffectsDispatcher, EffectsDispatcher<TStateProjection, TActionProjection>>();


            if (buildAction != null)
            {
                buildAction(new AddStoreBuilder<TStateProjection, TActionProjection>(services));
            }


            return this;
        }
    }
}
