using Microsoft.AspNetCore.Components;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace RxStore
{
    public abstract class PureComponentBase<TView, TCommand> : ReactiveComponentBase
    {
        protected PureComponentBase()
        {
            Views = Property<TView>();
        }

        protected PropertySubject<TView> Views { get; }

        [Parameter]
        public TView View { set => Views.OnNext(value); }

        [Parameter]
        public EventCallback<TCommand> OnCommand { get; set; }


        protected Func<Task> OnCommandWith(TCommand command)
        {
            return () => OnCommand.InvokeAsync(command);
        }
    }
}
