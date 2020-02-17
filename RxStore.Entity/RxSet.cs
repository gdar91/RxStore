using System;
using System.Collections.Generic;

namespace RxStore.Entity
{
    public sealed class RxSet<Key, Value>
    {
        private readonly Dictionary<Key, IObservable<Value>> dictionary =
            new Dictionary<Key, IObservable<Value>>();

        private readonly Func<Key, IObservable<Value>> valueFactory;

        public RxSet(Func<Key, IObservable<Value>> valueFactory)
        {
            this.valueFactory = valueFactory;
        }

        public IObservable<Value> this[Key key]
        {
            get
            {
                lock (dictionary)
                {
                    if (dictionary.TryGetValue(key, out var persistedValue))
                    {
                        return persistedValue;
                    }

                    var value = valueFactory(key);

                    dictionary[key] = value;

                    return value;
                }
            }
        }
    }

    public static class RxSet<Key>
    {
        public static RxSet<Key, Value> Of<Value>(Func<Key, IObservable<Value>> valueFactory) =>
            new RxSet<Key, Value>(valueFactory);
    }
}
