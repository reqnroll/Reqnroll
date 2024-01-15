using System.Collections.Generic;

namespace Reqnroll.TestProjectGenerator.Extensions
{
    public static class EnumerableExtensions
    {
        public static string JoinToString<T>(this IEnumerable<T> source, string separator) => string.Join(separator, source);

        public static string JoinToString(this IEnumerable<string> source, string separator) => string.Join(separator, source);
    }
}
