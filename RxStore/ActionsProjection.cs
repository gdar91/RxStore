using System;
using System.Reactive.Linq;

namespace RxStore
{
    public sealed class ActionsProjection<TState, TAction, TStateProjection, TActionProjection> :
        IActions<TStateProjection, TActionProjection>
    {
        public ActionsProjection(
            IActions<TState, TAction> actions,
            Func<TAction, ValueTuple<bool, TActionProjection>> actionChooser
        )
        {
            Actions = actions.Actions
                .Select(actionChooser)
                .Where(tuple => tuple.Item1)
                .Select(tuple=> tuple.Item2)
                .ShareReplayLatest();
        }

        public IObservable<TActionProjection> Actions { get; }
    }
}
