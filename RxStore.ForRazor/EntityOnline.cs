using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public abstract class EntityOnline<TEvent>
    {
        private readonly Subject<IObservable<TEvent>> outEventsObservables = new();


        protected EntityOnline()
        {
            OutEvents = outEventsObservables.SelectMany(observable => observable);
        }


        internal IObservable<TEvent> OutEvents { get; private protected set; }


        private protected void OnNextOutEventsObservable(IObservable<TEvent> observable)
        {
            outEventsObservables.OnNext(observable);
        }
    }
}
