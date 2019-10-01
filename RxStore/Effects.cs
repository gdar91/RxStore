using System;
using System.Collections.Generic;

namespace RxStore
{
    public abstract class Effects<TState, TAction> : IEffects<TState, TAction>
    {
        protected Effects(IActions<TState, TAction> actions)
        {
            Actions = actions.Actions;
        }


        protected IObservable<TAction> Actions  { get; }


        public abstract IEnumerable<IObservable<TAction>> GetEffects();
    }
}
