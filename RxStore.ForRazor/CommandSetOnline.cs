using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fills;

namespace RxStore
{
    public abstract class CommandSetOnline<TKey, TCommand, TEvent> :
        EntityOnline<TEvent>,
        IMemo<TKey, IObserver<TCommand>>
    {
        private readonly Subject<(TKey, TCommand)> keyCommandTuplesSubject = new();

        private readonly Memo<TKey, IObserver<TCommand>> byKey;


        protected CommandSetOnline()
        {
            OutEvents =
                keyCommandTuplesSubject
                    .GroupBy(tuple => tuple.Item1)
                    .SelectMany(group =>
                        group
                            .Select(tuple => tuple.Item2)
                            .Select(command => OnCommand(group.Key, command))
                            .Exhaust()
                    );

            byKey =
                new(key =>
                    Observer.Create<TCommand>(command =>
                        keyCommandTuplesSubject.OnNext((key, command))
                    )
                );
        }


        protected abstract IObservable<TEvent> OnCommand(TKey key, TCommand command);


        public IObserver<TCommand> this[TKey key] => byKey[key];
    }
}
