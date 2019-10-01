using System;

namespace RxStore
{
    public interface IEffectsDispatcher : IDisposable
    {
        void Initialize();
    }
}
