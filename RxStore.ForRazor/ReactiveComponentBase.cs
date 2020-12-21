using Microsoft.AspNetCore.Components;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace RxStore
{
    public abstract class ReactiveComponentBase : ComponentBase, IDisposable
    {
        public ReactiveComponentBase()
        {
            Disposes =
                DisposesSubject
                    .Synchronize()
                    .Take(1)
                    .Replay(1)
                    .AutoConnect(0);


            Initializes =
                InitializesSubject
                    .Synchronize()
                    .TakeUntil(Disposes)
                    .Publish()
                    .AutoConnect(0);


            SetParameters =
                SetParametersSubject
                    .Synchronize()
                    .TakeUntil(Disposes)
                    .Publish()
                    .AutoConnect(0);


            ParametersSets =
                ParametersSetsSubject
                    .Synchronize()
                    .TakeUntil(Disposes)
                    .Publish()
                    .AutoConnect(0);


            AfterRenders =
                AfterRendersSubject
                    .Synchronize()
                    .TakeUntil(Disposes)
                    .Publish()
                    .AutoConnect(0);
        }




        protected override void OnInitialized() => InitializesSubject.OnNext(Unit.Default);

        private Subject<Unit> InitializesSubject { get; } = new Subject<Unit>();

        protected IObservable<Unit> Initializes { get; }




        public void Dispose() => DisposesSubject.OnNext(Unit.Default);

        private Subject<Unit> DisposesSubject { get; } = new Subject<Unit>();

        protected IObservable<Unit> Disposes { get; }




        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);

            SetParametersSubject.OnNext(parameters);
        }

        private ISubject<ParameterView> SetParametersSubject { get; } =
            new Subject<ParameterView>();

        protected IObservable<ParameterView> SetParameters { get; }




        protected override void OnParametersSet() => ParametersSetsSubject.OnNext(Unit.Default);

        private Subject<Unit> ParametersSetsSubject { get; } = new Subject<Unit>();

        protected IObservable<Unit> ParametersSets { get; }




        protected override void OnAfterRender(bool firstRender) =>
            AfterRendersSubject.OnNext(firstRender);

        private Subject<bool> AfterRendersSubject { get; } = new Subject<bool>();

        protected IObservable<bool> AfterRenders { get; }




        protected override bool ShouldRender() => false;


        protected bool DefaultShouldRender() => base.ShouldRender();
    }
}
