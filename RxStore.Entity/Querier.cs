using RxUnit = System.Reactive.Unit;
using FsUnit = Microsoft.FSharp.Core.Unit;
using Microsoft.FSharp.Core;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace RxStore.Entity
{
    public static class Querier
    {
        public static IObservable<FSharpOption<EntityInfo<T>>> UsingQuerier<T>(
            this IObservable<FSharpOption<EntityInfo<T>>> entityInfoOptions,
            TimeSpan retryInterval,
            TimeSpan refreshInterval,
            Func<RxUnit> fetch
        )
        {
            var passive = entityInfoOptions.ShareReplayLatest();
            var querier = ForEntityInfo(passive, retryInterval, refreshInterval, fetch);

            return Observable.Using(querier.Subscribe, subscription => passive);
        }

        public static IObservable<FSharpOption<EntityInfo<T>>> UsingQuerier<T>(
            this IObservable<FSharpOption<EntityInfo<T>>> entityInfoOptions,
            Func<
                IObservable<FsUnit>,
                IObservable<Stamp<FsUnit>>,
                IObservable<Stamp<string>>,
                IObservable<Stamp<T>>,
                IObservable<FsUnit>
            > signals,
            Func<RxUnit> fetch
        )
        {
            var passive = entityInfoOptions.ShareReplayLatest();
            var querier = ForEntityInfo(passive, signals, fetch);

            return Observable.Using(querier.Subscribe, subscription => passive);
        }




        public static IObservable<RxUnit> ForEntityInfo<T>(
            IObservable<FSharpOption<EntityInfo<T>>> entityInfoOptions,
            TimeSpan retryInterval,
            TimeSpan refreshInterval,
            Func<RxUnit> fetch
        )
        {
            return ForEntityInfo(
                entityInfoOptions,
                (unavailables, pendings, fails, successes) => Observable.Merge(
                    unavailables,
                    fails
                        .Select(stamp => Observable.Timer(stamp.Time.Add(retryInterval)))
                        .Switch()
                        .Select(next => default(FsUnit)),
                    successes
                        .Select(stamp => Observable.Timer(stamp.Time.Add(refreshInterval)))
                        .Switch()
                        .Select(next => default(FsUnit))
                ),
                fetch
            );
        }

        public static IObservable<RxUnit> ForEntityInfo<T>(
            IObservable<FSharpOption<EntityInfo<T>>> entityInfoOptions,
            Func<
                IObservable<FsUnit>,
                IObservable<Stamp<FsUnit>>,
                IObservable<Stamp<string>>,
                IObservable<Stamp<T>>,
                IObservable<FsUnit>
            > signals,
            Func<RxUnit> fetch
        )
        {
            var unavailables = new Subject<FsUnit>();
            var pendings = new Subject<Stamp<FsUnit>>();
            var fails = new Subject<Stamp<string>>();
            var successes = new Subject<Stamp<T>>();

            var fetches = signals(
                unavailables.AsObservable(),
                pendings.AsObservable(),
                fails.AsObservable(),
                successes.AsObservable()
            )
                .Select(signal => Observable
                    .FromAsync(() => Task.FromResult(fetch()))
                    .Catch<RxUnit, Exception>(error => Observable.Empty<RxUnit>())
                )
                .Switch();

            var signalings = entityInfoOptions.Select(entityInfoOption =>
            {
                if (OptionModule.IsNone(entityInfoOption))
                {
                    unavailables.OnNext(default);

                    return RxUnit.Default;
                }

                var entityInfo = entityInfoOption.Value;
                var stamp = entityInfo.Stamp;
                var completion = stamp.Item;

                switch (completion)
                {
                    case Completion<FSharpResult<T, string>> { IsPending: true }:

                        pendings.OnNext(Stamp.MapTo(default(FsUnit), stamp));

                        break;

                    case Completion<FSharpResult<T, string>>.Completed { Item: var result }:

                        if (result.IsError)
                        {
                            fails.OnNext(Stamp.MapTo(result.ErrorValue, stamp));
                        }
                        else
                        {
                            successes.OnNext(Stamp.MapTo(result.ResultValue, stamp));
                        }

                        break;
                }

                return RxUnit.Default;
            });


            var observable = Observable.Merge(
                fetches,
                signalings
            );

            return observable;
        }
    }
}
