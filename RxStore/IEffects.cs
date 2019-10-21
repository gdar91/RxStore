using System;
using System.Collections.Generic;

namespace RxStore
{
    public interface IEffects<TState, TAction>
    {
        IEnumerable<IObservable<TAction>> GetEffects(IObservable<TAction> actions);
    }
}
