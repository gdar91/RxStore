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
                    .Take(1)
                    .Replay(1)
                    .AutoConnect(0);


            Initializes =
                InitializesSubject
                    .TakeUntil(Disposes)
                    .Publish()
                    .AutoConnect(0);


            SetParameters =
                SetParametersSubject
                    .TakeUntil(Disposes)
                    .Publish()
                    .AutoConnect(0);


            ParametersSets =
               ParametersSetsSubject
                   .TakeUntil(Disposes)
                   .Publish()
                   .AutoConnect(0);


            AfterRenders =
                AfterRendersSubject
                    .TakeUntil(Disposes)
                    .Publish()
                    .AutoConnect(0);
        }




        protected override void OnInitialized() =>
            InitializesSubject.OnNext(Unit.Default);

        private ISubject<Unit> InitializesSubject { get; } =
            Subject.Synchronize(new Subject<Unit>());

        protected IObservable<Unit> Initializes { get; }




        public void Dispose() =>
            DisposesSubject.OnNext(Unit.Default);

        private ISubject<Unit> DisposesSubject { get; } =
            Subject.Synchronize(new Subject<Unit>());

        protected IObservable<Unit> Disposes { get; }




        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);

            SetParametersSubject.OnNext(parameters);
        }

        private ISubject<ParameterView> SetParametersSubject { get; } =
            Subject.Synchronize(new Subject<ParameterView>());

        protected IObservable<ParameterView> SetParameters { get; }




        protected override void OnParametersSet() =>
            ParametersSetsSubject.OnNext(Unit.Default);

        private ISubject<Unit> ParametersSetsSubject { get; } =
            Subject.Synchronize(new Subject<Unit>());

        protected IObservable<Unit> ParametersSets { get; }




        protected override void OnAfterRender(bool firstRender) =>
            AfterRendersSubject.OnNext(firstRender);

        private ISubject<bool> AfterRendersSubject { get; } =
            Subject.Synchronize(new Subject<bool>());

        protected IObservable<bool> AfterRenders { get; }




        protected override bool ShouldRender() => false;


        protected bool DefaultShouldRender() => base.ShouldRender();
    }
}
