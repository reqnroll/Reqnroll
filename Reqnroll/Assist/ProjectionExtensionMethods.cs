using System;
using System.Collections.Generic;

namespace Reqnroll.Assist
{
    public static class ProjectionExtensionMethods
    {
        [Obsolete("Use TableHelpers instead")]
        public static IEnumerable<Projection<T>> ToProjection<T>(this IEnumerable<T> collection, Table table = null)
        {
            var tableHelpers = new TableHelpers(Service.Instance);
            return new EnumerableProjection<T>(tableHelpers, table, collection);
        }

        [Obsolete("Use TableHelpers instead")]
        public static IEnumerable<Projection<T>> ToProjection<T>(this Table table)
        {
            var tableHelpers = new TableHelpers(Service.Instance);
            return new EnumerableProjection<T>(tableHelpers, table);
        }

        [Obsolete("Use TableHelpers instead")]
        public static IEnumerable<Projection<T>> ToProjectionOfSet<T>(this Table table, IEnumerable<T> collection)
        {
            var tableHelpers = new TableHelpers(Service.Instance);
            return new EnumerableProjection<T>(tableHelpers, table);
        }

        [Obsolete("Use TableHelpers instead")]
        public static IEnumerable<Projection<T>> ToProjectionOfInstance<T>(this Table table, T instance)
        {
            var tableHelpers = new TableHelpers(Service.Instance);
            return new EnumerableProjection<T>(tableHelpers, table);
        }
    }
}