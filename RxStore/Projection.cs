using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxStore
{
    public interface IProjection<out TState> : IObservable<TState>, IDisposable
    { }


    public static class Projection
    {
        public static IProjection<TState> Of<TState, TParentState>(
            IObservable<TParentState> parent,
            Func<TParentState, TState> selector
        )
        {
            var observable =
                parent
                    .Select(selector)
                    .DistinctUntilChanged();

            var projection = new ObservableProjection<TState>(observable);

            return projection;
        }


        private sealed class ObservableProjection<TState> : IProjection<TState>
        {
            private readonly IObservable<TState> observable;

            private readonly ISubject<Unit> disposes = new ReplaySubject<Unit>(1);


            public ObservableProjection(IObservable<TState> observable)
            {
                this.observable =
                    observable
                        .TakeUntil(disposes)
                        .Replay(1)
                        .AutoConnect();
            }


            public void Dispose()
            {
                disposes.OnNext(Unit.Default);
            }

            public IDisposable Subscribe(IObserver<TState> observer)
            {
                return observable.Subscribe(observer);
            }
        }
    }
}
