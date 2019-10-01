using System;

namespace RxStore
{
    public sealed class DispatcherProjection<TState, TAction, TStateProjection, TActionProjection> :
        IDispatcher<TStateProjection, TActionProjection>
    {
        private readonly Action<TActionProjection> dispatchAction;


        public DispatcherProjection(
            IDispatcher<TState, TAction> dispatcher,
            Func<TActionProjection, TAction> actionGeneralizer
        )
        {
            dispatchAction = action => dispatcher.Dispatch(actionGeneralizer(action));
        }


        public void Dispatch(TActionProjection action) => dispatchAction(action);
    }
}
