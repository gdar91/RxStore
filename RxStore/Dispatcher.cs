using System;
using System.Reactive;

namespace RxStore
{
    public interface IDispatcher<in TEvent> : IObserver<TEvent>
    { }


    public static class Dispatcher<TEvent>
    {
        public static IDispatcher<TEvent> Of<TParentEvent>(
            IObserver<TParentEvent> parent,
            Func<TEvent, TParentEvent> selector
        )
        {
            var observer = Observer.Create<TEvent>(
                next => parent.OnNext(selector(next)),
                parent.OnError,
                () => { }
            );

            var dispatcher = new ObserverDispatcher(observer);

            return dispatcher;
        }


        private sealed class ObserverDispatcher : IDispatcher<TEvent>
        {
            private readonly IObserver<TEvent> observer;

            public ObserverDispatcher(IObserver<TEvent> observer)
            {
                this.observer = observer;
            }
            
            public void OnCompleted()
            {
                observer.OnCompleted();
            }

            public void OnError(Exception error)
            {
                observer.OnError(error);
            }

            public void OnNext(TEvent value)
            {
                observer.OnNext(value);
            }
        }
    }
}
