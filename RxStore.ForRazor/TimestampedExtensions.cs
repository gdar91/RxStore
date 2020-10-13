using RxStore.Entity;
using System.Reactive;

namespace RxStore
{
    public static class TimestampedExtensions
    {
        public static Stamp<T> AsStamp<T>(this Timestamped<T> timestamped) =>
            Stamp.OfValues(timestamped.Timestamp, timestamped.Value);
    }
}
