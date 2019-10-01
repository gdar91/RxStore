using System;

namespace RxStore
{
    public sealed class StateProjection<TState, TAction, TStateProjection, TActionProjection> :
        IState<TStateProjection, TActionProjection>
    {
        private readonly IObservable<TStateProjection> stateProjection;


        public StateProjection(
            IState<TState, TAction> state,
            Func<TState, TStateProjection> stateProjector
        )
        {
            stateProjection = state.Project(stateProjector).ShareReplayLatest();
        }


        public IDisposable Subscribe(IObserver<TStateProjection> observer)
            => stateProjection.Subscribe(observer);
    }
}
