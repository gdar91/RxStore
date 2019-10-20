namespace RxStore.DevTools
{
    public static class AddDevToolsExtensions
    {
        public static AddStoreBuilder<TState, TAction> WithDevTools<TState, TAction>(
            this AddStoreBuilder<TState, TAction> addStoreBuilder
        )
        {
            addStoreBuilder.WithEffects<DevTools<TState, TAction>>();

            return addStoreBuilder;
        }
    }
}
