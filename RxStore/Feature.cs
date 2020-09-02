using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public sealed class Feature<TEvent, TState> : ISubject<TEvent, TState>
    {
        private Feature(IObservable<TState> states, IObserver<TEvent> onEvent)
        {
            States =
                states
                    .Replay(1)
                    .AutoConnect();

            OnEvent = onEvent;
        }


        private IObservable<TState> States { get; }

        private IObserver<TEvent> OnEvent { get; }


        public void OnNext(TEvent value) => OnEvent.OnNext(value);

        public void OnError(Exception error) => OnEvent.OnError(error);

        public void OnCompleted() => OnEvent.OnCompleted();


        public IDisposable Subscribe(IObserver<TState> observer) => States.Subscribe(observer);




        public static Feature<TEvent, TState> Of<TStoreEvent, TStoreState>(
            ISubject<TStoreEvent, TStoreState> parent,
            Func<TStoreState, TState> stateSelector,
            Func<TEvent, TStoreEvent> eventLifter
        )
        {
            var states =
                parent
                    .Select(stateSelector)
                    .DistinctUntilChanged();

            var onEvent = Observer.Create<TEvent>(
                @event =>
                {
                    var liftedEvent = eventLifter(@event);

                    parent.OnNext(liftedEvent);
                },
                parent.OnError,
                () => { }
            );


            var feature = new Feature<TEvent, TState>(states, onEvent);


            return feature;
        }
    }
}
