

// ReSharper disable once CheckNamespace
namespace TechTalk.SpecFlow.Assist;

public static class InstanceComparisonExtensionMethods
{

    public static void CompareToInstance<T>(this Table table, T instance)
    {
        Reqnroll.InstanceComparisonExtensionMethods.CompareToInstance(table, instance);
    }

    /// <summary>
    /// Indicates whether the table is equivalent to the specified instance by comparing the values of all
    /// columns against the properties of the instance.  Will return false after finding the first difference.
    /// </summary>
    public static bool IsEquivalentToInstance<T>(this Table table, T instance)
    {
        return Reqnroll.InstanceComparisonExtensionMethods.IsEquivalentToInstance(table, instance);
    }
}
