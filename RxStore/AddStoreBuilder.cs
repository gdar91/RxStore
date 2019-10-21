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




        public AddStoreBuilder<TState, TAction> WithProjections(Type type)
        {
            services.AddSingleton(type);

            return this;
        }

        public AddStoreBuilder<TState, TAction> WithProjections<TProjections>()
            where TProjections : Store<TState, TAction>
        {
            services.AddSingleton<TProjections>();

            return this;
        }

        public AddStoreBuilder<TState, TAction> WithProjections<TProjections>(
            Func<IServiceProvider, TProjections> implementationFactory
        )
            where TProjections : Store<TState, TAction>
        {
            services.AddSingleton(implementationFactory);

            return this;
        }




        public AddStoreBuilder<TState, TAction> WithEffects(Type type)
        {
            services.AddSingleton(typeof(IEffects<TState, TAction>), type);
            services.AddSingleton<IEffectsDispatcher, EffectsDispatcher<TState, TAction>>();

            return this;
        }

        public AddStoreBuilder<TState, TAction> WithEffects<TEffects>()
            where TEffects : class, IEffects<TState, TAction>
        {
            services.AddSingleton<IEffects<TState, TAction>, TEffects>();
            services.AddSingleton<IEffectsDispatcher, EffectsDispatcher<TState, TAction>>();

            return this;
        }

        public AddStoreBuilder<TState, TAction> WithEffects(
            Func<IServiceProvider, IEffects<TState, TAction>> implementationFactory
        )
        {
            services.AddSingleton<IEffects<TState, TAction>>(implementationFactory);
            services.AddSingleton<IEffectsDispatcher, EffectsDispatcher<TState, TAction>>();

            return this;
        }




        public AddStoreBuilder<TState, TAction> AddFeatureStore<TFeatureState, TFeatureAction>(
            Func<TState, TFeatureState> stateProjector,
            Func<TAction, ValueTuple<bool, TFeatureAction>> actionChooser,
            Func<TFeatureAction, TAction> actionGeneralizer,
            Action<AddStoreBuilder<TFeatureState, TFeatureAction>> buildAction = null
        )
        {
            services.AddSingleton(provider =>
            {
                var store = provider.GetRequiredService<Store<TState, TAction>>();

                var featureStore = new FeatureStore<TState, TAction, TFeatureState, TFeatureAction>(
                    store,
                    stateProjector,
                    actionChooser,
                    actionGeneralizer
                );
                
                return featureStore;
            });

            services.AddSingleton<IConnectableStore>(provider =>
                provider.GetRequiredService<FeatureStore<TState, TAction, TFeatureState, TFeatureAction>>()
            );


            if (buildAction != null)
            {
                buildAction(new AddStoreBuilder<TFeatureState, TFeatureAction>(services));
            }


            return this;
        }
    }
}
