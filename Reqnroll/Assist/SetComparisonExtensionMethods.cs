using System;
using System.Collections.Generic;
using Reqnroll.Assist;

namespace Reqnroll
{
    public static class SetComparisonExtensionMethods
    {
        public static void CompareToSet<T>(this Table table, IEnumerable<T> set, bool sequentialEquality = false)
        {
            var tableHelpers = new TableHelpers(Service.Instance);
            var checker = new SetComparer<T>(table, tableHelpers);
            checker.CompareToSet(set, sequentialEquality);
        }
    }
}
