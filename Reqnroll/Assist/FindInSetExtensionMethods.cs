using System;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.Assist
{
    public static class FindInSetExtensionMethods
    {
        [Obsolete("Use TableHelpers instead")]
        public static T FindInSet<T>(this Table table, IEnumerable<T> set)
        {
            var instanceTable = TEHelpers.GetTheProperInstanceTable(table, typeof(T));

            var matches = set.Where(instance => InstanceMatchesTable(Service.Instance, instance, instanceTable)).ToArray();

            if (matches.Length > 1) throw new ComparisonException("Multiple instances match the table");

            return matches.FirstOrDefault();
        }

        [Obsolete("Use TableHelpers instead")]
        public static IEnumerable<T> FindAllInSet<T>(this Table table, IEnumerable<T> set)
        {
            var instanceTable = TEHelpers.GetTheProperInstanceTable(table, typeof(T));

            return set.Where(instance => InstanceMatchesTable(Service.Instance, instance, instanceTable)).ToArray();
        }

        private static bool InstanceMatchesTable<T>(Service service, T instance, Table table)
        {
            return table.Rows.All(row => !InstanceComparisonExtensionMethods.ThereIsADifference(service, instance, row));
        }
    }
}
