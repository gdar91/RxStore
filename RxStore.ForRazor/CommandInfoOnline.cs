using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fills;

namespace RxStore
{
    public abstract class CommandInfoOnline<TCommand, TEvent> : EntityOnline<TEvent>, IObserver<TCommand>
    {
        private readonly Subject<TCommand> commandsSubject = new();


        protected CommandInfoOnline()
        {
            OutEvents =
                commandsSubject
                    .Select(OnCommand)
                    .Exhaust();
        }


        protected abstract IObservable<TEvent> OnCommand(TCommand command);


        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(TCommand value)
        {
            commandsSubject.OnNext(value);
        }
    }
}
