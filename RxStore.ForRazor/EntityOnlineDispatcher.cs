using System;
using System.Reactive;
using System.Reactive.Linq;

namespace RxStore
{
    internal interface IEntityOnlineDispatcher : IObservable<Unit>
    { }


    internal sealed class EntityOnlineDispatcher<TEvent, TEntityOnline> : IEntityOnlineDispatcher
        where TEntityOnline : EntityOnline<TEvent>
    {
        private readonly IObservable<Unit> observable;


        public EntityOnlineDispatcher(TEntityOnline entityOnline, IDispatcher<TEvent> dispatcher)
        {
            observable =
                entityOnline.OutEvents
                    .Do(dispatcher)
                    .IgnoreElements()
                    .Select(next => Unit.Default)
                    .Publish()
                    .RefCount();
        }


        public IDisposable Subscribe(IObserver<Unit> observer) => observable.Subscribe(observer);
    }
}
