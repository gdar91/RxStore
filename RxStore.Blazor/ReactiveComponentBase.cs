using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.Components;

namespace RxStore
{
    public abstract class ReactiveComponentBase : ComponentBase, IDisposable
    {
        private Subject<Unit> initializesSubject = new Subject<Unit>();

        private Subject<Unit> parametersSetsSubject = new Subject<Unit>();

        private Subject<bool> afterRendersSubject = new Subject<bool>();

        private Subject<Unit> disposesSubject = new Subject<Unit>();




        public ReactiveComponentBase()
        {
            Initializes = initializesSubject
                .Take(1)
                .Publish()
                .AutoConnect(0);

            Disposes = disposesSubject
                .Take(1)
                .Publish()
                .AutoConnect(2);


            ParametersSets = parametersSetsSubject
                .TakeUntil(Disposes)
                .Publish()
                .AutoConnect(0);
            
            AfterRenders = afterRendersSubject
                .TakeUntil(Disposes)
                .Publish()
                .AutoConnect(0);
        }




        protected IObservable<Unit> Initializes { get; }

        protected IObservable<Unit> ParametersSets { get; }

        protected IObservable<bool> AfterRenders { get; }

        protected IObservable<Unit> Disposes { get; }




        protected override void OnInitialized() => initializesSubject.OnNext(Unit.Default);

        protected override void OnParametersSet() => parametersSetsSubject.OnNext(Unit.Default);

        protected override void OnAfterRender(bool firstRender)
            => afterRendersSubject.OnNext(firstRender);

        public void Dispose() => disposesSubject.OnNext(Unit.Default);
    }
}
