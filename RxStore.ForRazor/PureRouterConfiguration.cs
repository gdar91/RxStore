using System;
using System.Text.RegularExpressions;

namespace RxStore
{
    public sealed record PureRouterConfiguration<TData>(params PureRouterMatch<TData>[] Matches);


    public delegate bool PureRouterMatch<TData>(string uri, out TData result);


    public static class PureRouterMatch
    {
        public static PureRouterMatch<TData> FromRegex<TData>(
            string pattern,
            Func<GroupCollection, TData> dataSelector
        )
        {
            return (string uri, out TData result) =>
            {
                var match = Regex.Match(uri, pattern);

                if (!match.Success)
                {
                    result = default;

                    return false;
                }

                result = dataSelector(match.Groups);

                return true;
            };
        }
    }
}
