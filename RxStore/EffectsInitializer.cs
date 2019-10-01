using System.Collections.Generic;

namespace RxStore
{
    public sealed class EffectsInitializer
    {
        private readonly IEnumerable<IEffectsDispatcher> allEffectsDispatchers;


        public EffectsInitializer(IEnumerable<IEffectsDispatcher> allEffectsDispatchers)
        {
            this.allEffectsDispatchers = allEffectsDispatchers;
        }


        public void InitializeAll()
        {
            foreach (var effectsDispatcher in allEffectsDispatchers)
            {
                effectsDispatcher.Initialize();
            }
        }
    }
}
