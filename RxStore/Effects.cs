using System;
using System.Collections.Generic;

namespace RxStore
{
    public interface IEffects<TState, TAction>
    {
        IEnumerable<IObservable<TAction>> GetEffects();
    }


    public abstract class Effects<TState, TAction>
        : Effects<TState, TAction, IStore<TState, TAction>>
    {
        protected Effects(IStore<TState, TAction> store) : base(store)
        { }
    }


    public abstract class Effects<TState, TAction, TStore> : IEffects<TState, TAction>
        where TStore : IStore<TState, TAction>
    {
        protected Effects(TStore store)
        {
            Store = store;
        }


        protected TStore Store { get; }

        protected IObservable<TAction> Actions  => Store.Actions;


        public abstract IEnumerable<IObservable<TAction>> GetEffects();
    }
}
