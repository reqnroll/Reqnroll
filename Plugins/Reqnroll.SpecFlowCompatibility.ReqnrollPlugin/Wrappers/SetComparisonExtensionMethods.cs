using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace TechTalk.SpecFlow.Assist;

public static class SetComparisonExtensionMethods
{
    public static void CompareToSet<T>(this Table table, IEnumerable<T> set, bool sequentialEquality = false)
    {
        Reqnroll.SetComparisonExtensionMethods.CompareToSet(table, set, sequentialEquality);
    }
}