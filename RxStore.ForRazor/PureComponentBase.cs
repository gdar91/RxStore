using Microsoft.AspNetCore.Components;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace RxStore
{
    public abstract class PureComponentBase<TView, TCommand> : ReactiveComponentBase
    {
        private readonly ReplaySubject<TView> viewsSubject = new ReplaySubject<TView>(1);

        protected PureComponentBase()
        {
            Views =
                viewsSubject
                    .Synchronize()
                    .DistinctUntilChanged()
                    .TakeUntil(Disposes)
                    .Replay(1)
                    .AutoConnect(0);
        }

        protected IObservable<TView> Views { get; }

        [Parameter]
        public TView View { set => viewsSubject.OnNext(value); }

        [Parameter]
        public EventCallback<TCommand> OnCommand { get; set; }


        protected Func<Task> OnCommandWith(TCommand command)
        {
            return () => OnCommand.InvokeAsync(command);
        }
    }
}
