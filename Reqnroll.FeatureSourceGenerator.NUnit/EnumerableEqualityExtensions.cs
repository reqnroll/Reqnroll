namespace Reqnroll.FeatureSourceGenerator.NUnit;

internal static class EnumerableEqualityExtensions
{
    public static bool SetEquals<T>(this IEnumerable<T> source, IEnumerable<T> other) where T : IEquatable<T>
    {
        if (source == null && other == null)
        {
            return true;
        }

        if (source == null || other == null)
        {
            return false;
        }

        var otherItems = other.ToList();

        foreach (var item in source)
        {
            if (!otherItems.Remove(item))
            {
                return false;
            }
        }

        if (otherItems.Count != 0)
        {
            return false;
        }

        return true;
    }
}
