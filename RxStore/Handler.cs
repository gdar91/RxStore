using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public abstract class Handler<TCommand, TEvent> : ISubject<TCommand, TEvent>
    {
        public Handler()
        {
            Events = Observable
                .Defer(() => Setup(Commands).ToObservable())
                .Where(observable => observable != null)
                .Merge();
        }


        private ISubject<TCommand> Commands { get; } =
            Subject.Synchronize(new Subject<TCommand>());

        private IObservable<TEvent> Events { get; }


        protected abstract IEnumerable<IObservable<TEvent>> Setup(ISubject<TCommand> commands);


        public void OnNext(TCommand value) => Commands.OnNext(value);

        public void OnError(Exception error) => Commands.OnError(error);

        public void OnCompleted() => Commands.OnCompleted();


        public IDisposable Subscribe(IObserver<TEvent> observer) => Events.Subscribe(observer);
    }
}
