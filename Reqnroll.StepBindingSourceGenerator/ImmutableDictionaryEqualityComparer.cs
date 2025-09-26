using System.Collections.Immutable;

namespace Reqnroll.StepBindingSourceGenerator;

internal class ImmutableDictionaryEqualityComparer<TKey, TValue> : IEqualityComparer<ImmutableDictionary<TKey, TValue>?>
    where TKey : notnull
{
    public static IEqualityComparer<ImmutableDictionary<TKey, TValue>> Instance { get; } = 
        new ImmutableDictionaryEqualityComparer<TKey, TValue>();

    public bool Equals(
        ImmutableDictionary<TKey, TValue>? x,
        ImmutableDictionary<TKey, TValue>? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        if (x.Count != y.Count)
        {
            return false;
        }

        if (!Equals(x.KeyComparer, y.KeyComparer))
        {
            return false;
        }

        if (!Equals(x.ValueComparer, y.ValueComparer))
        {
            return false;
        }

        foreach (var kvp in x)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            if (!y.TryGetValue(key, out var other) || !x.ValueComparer.Equals(value, other))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(ImmutableDictionary<TKey, TValue>? obj) => obj?.Count ?? -1;
}
