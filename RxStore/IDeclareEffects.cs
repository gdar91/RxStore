using System;
using System.Collections.Generic;

namespace RxStore
{
    public interface IDeclareEffects<TState, TAction>
    {
        IEnumerable<IObservable<TAction>> GetEffects(IObservable<TAction> actions);
    }
}
