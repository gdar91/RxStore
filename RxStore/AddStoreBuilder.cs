using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RxStore
{
    public sealed class AddStoreBuilder<TState, TAction>
    {
        public readonly IServiceCollection services;


        internal AddStoreBuilder(IServiceCollection services)
        {
            this.services = services;
        }




        public AddStoreBuilder<TState, TAction> WithProjections(Type type)
        {
            services.AddSingleton(type);

            return this;
        }

        public AddStoreBuilder<TState, TAction> WithProjections<TDeclaration>()
            where TDeclaration : class, IStore<TState, TAction>
        {
            services.AddSingleton<TDeclaration>();

            return this;
        }

        public AddStoreBuilder<TState, TAction> WithProjections<TDeclaration>(
            Func<IServiceProvider, TDeclaration> implementationFactory
        )
            where TDeclaration : IStore<TState, TAction>
        {
            services.AddSingleton(implementationFactory);

            return this;
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




        public AddStoreBuilder<TState, TAction> AddFeatureStore<TFeatureState, TFeatureAction>(
            Func<TState, TFeatureState> stateProjector,
            Func<TAction, ValueTuple<bool, TFeatureAction>> actionChooser,
            Func<TFeatureAction, TAction> actionGeneralizer,
            Action<AddStoreBuilder<TFeatureState, TFeatureAction>> buildAction = null
        )
        {
            services.AddSingleton<IStore<TFeatureState, TFeatureAction>>(provider =>
            {
                var store = provider.GetRequiredService<IStore<TState, TAction>>();

                var featureStore = new FeatureStore<TState, TAction, TFeatureState, TFeatureAction>(
                    store,
                    stateProjector,
                    actionChooser,
                    actionGeneralizer
                );
                
                return featureStore;
            });


            services
                .AddSingleton<IEffectsDispatcher, EffectsDispatcher<TFeatureState, TFeatureAction>>();

            services.TryAddSingleton<EffectsInitializer>();


            if (buildAction != null)
            {
                buildAction(new AddStoreBuilder<TFeatureState, TFeatureAction>(services));
            }


            return this;
        }
    }
}
