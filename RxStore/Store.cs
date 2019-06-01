using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public sealed class Store<TState, TAction> : IObservable<TState>, IDisposable
    {
        private readonly ISubject<TAction> _actions = new Subject<TAction>();

        private readonly IConnectableObservable<TState> _states;

        private readonly IDisposable _statesConnection;

        private readonly IDisposable _effectsConnection;

        public Store(
            Func<TState, TAction, TState> reducer,
            TState initialState,
            IEnumerable<IDeclareEffects<TState, TAction>> effectsDeclarations
        )
        {
            _states = _actions
                .Scan(initialState, reducer)
                .StartWith(initialState)
                .DistinctUntilChanged()
                .Replay(1);
            
            _statesConnection = _states.Connect();
            
            var actions = _actions.AsObservable();
            var fallback = Observable.Empty<TAction>();

            var allEffects = effectsDeclarations
                .SelectMany(effectsDeclaration => effectsDeclaration.GetEffects(actions))
                .Where(effects => effects != null)
                .Select(effects => effects.Catch<TAction>(fallback))
                .ToObservable()
                .Merge()
                .Do(Dispatch)
                .Publish();
            
            _effectsConnection = allEffects.Connect();
        }

        public void Dispatch(TAction action) => _actions.OnNext(action);

        public IDisposable Subscribe(IObserver<TState> observer) => _states.Subscribe(observer);

        public void Dispose()
        {
            using var statesConnection = _statesConnection;
            using var effectsConnection = _effectsConnection;
        }
    }
}
