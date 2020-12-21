using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public interface IHandler<in TCommand, out TEvent> : ISubject<TCommand, TEvent>
    { }


    public abstract class Handler<TCommand, TEvent> : IHandler<TCommand, TEvent>
    {
        public Handler()
        {
            Events =
                Observable
                    .Defer(() => Setup(Commands.AsObservable()).ToObservable())
                    .Where(observable => observable != null)
                    .Merge();
        }


        private Subject<TCommand> Commands { get; } = new Subject<TCommand>();

        private IObservable<TEvent> Events { get; }


        protected abstract IEnumerable<IObservable<TEvent>> Setup(IObservable<TCommand> commands);


        public void OnNext(TCommand value) => Commands.OnNext(value);

        public void OnError(Exception error) => Commands.OnError(error);

        public void OnCompleted() => Commands.OnCompleted();


        public IDisposable Subscribe(IObserver<TEvent> observer) => Events.Subscribe(observer);
    }
}
