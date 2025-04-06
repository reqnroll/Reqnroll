using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal class ImmutableDictionaryComparer<TKey, TValue> : IEqualityComparer<ImmutableDictionary<TKey, TValue>?>
    where TKey : notnull
{
    public static ImmutableDictionaryComparer<TKey, TValue> Instance { get; } = 
        new ImmutableDictionaryComparer<TKey, TValue>();
    
    public bool Equals(ImmutableDictionary<TKey, TValue>? x, ImmutableDictionary<TKey, TValue>? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        if (x.Count != y.Count)
        {
            return false;
        }

        foreach (var key in x.Keys)
        {
            if (!y.TryGetValue(key, out var yValue))
            {
                return false;
            }

            var xValue = x[key];

            if (!Equals(xValue, yValue))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(ImmutableDictionary<TKey, TValue>? obj) => obj?.Count ?? -1;
}
