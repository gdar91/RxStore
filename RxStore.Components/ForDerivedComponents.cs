using Microsoft.AspNetCore.Components;
using Microsoft.FSharp.Core;
using RxStore.Entity;
using System;

namespace RxStore
{
    public partial class ForOption<T> : RxStoreC.ForOption<T>
    { }

    public partial class ForOptionObservable<T> : RxStoreC.ForOptionObservable<T>
    { }

    public partial class ForCompletion<T> : RxStoreC.ForCompletion<T>
    { }

    public partial class ForCompletionObservable<T> : RxStoreC.ForCompletionObservable<T>
    { }

    public partial class ForResult<TOk, TError> : RxStoreC.ForResult<TOk, TError>
    { }

    public partial class ForResultObservable<TOk, TError> : RxStoreC.ForResultObservable<TOk, TError>
    { }

    public partial class ForEntityInfo<T, TError> : RxStoreC.ForEntityInfo<T, TError>
    { }

    partial class ForEntityInfoObservable<T, TError> : RxStoreC.ForEntityInfoObservable<T, TError>
    { }

    public partial class ForEntityInfoOption<T, TError> : RxStoreC.ForEntityInfoOption<T, TError>
    { }

    public partial class ForEntityInfoOptionObservable<T, TError> : RxStoreC.ForEntityInfoOptionObservable<T, TError>
    { }
}
