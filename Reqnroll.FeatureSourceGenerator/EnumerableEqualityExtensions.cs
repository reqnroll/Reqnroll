namespace Reqnroll.FeatureSourceGenerator;

internal static class EnumerableEqualityExtensions
{
    public static bool SetEquals<T>(this IEnumerable<T> source, IEnumerable<T> other) where T : IEquatable<T>
    {
        if (source is null && other is null)
        {
            return true;
        }

        if (source is null || other is null)
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

    /// <summary>
    /// Gets a hash code for a sequence where re-ordering the elements does not affect the hash code.
    /// </summary>
    /// <typeparam name="T">The type of items in the sequence.</typeparam>
    /// <param name="set">The sequence to treat as a set.</param>
    /// <returns>A hash code for the sequence.</returns>
    public static int GetSetHashCode<T>(this IEnumerable<T> set)
    {
        unchecked
        {
            var hash = 23;

            foreach (var item in set)
            {
                hash += 13 + item?.GetHashCode() ?? 0;
            }

            return hash;
        }
    }
}
