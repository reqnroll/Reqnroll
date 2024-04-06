using Microsoft.CodeAnalysis;

namespace Reqnroll.FeatureSourceGenerator;

internal static class IncrementalValuesProviderExtensions
{
    public static IncrementalValuesProvider<T> WhereIsNotNull<T>(this IncrementalValuesProvider<T?> provider)
    {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        return provider.Where(item => item is not null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
    }

    public static IncrementalValuesProvider<(TOuter, TInner)> Join<TOuter, TInner, TKey>(
        this IncrementalValuesProvider<TOuter> outer,
        IncrementalValuesProvider<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector)
    {
        throw new NotImplementedException();
    }

    public static IncrementalValuesProvider<(TOuter, TInner?)> OuterJoin<TOuter, TInner, TKey>(
        this IncrementalValuesProvider<TOuter> outer,
        IncrementalValuesProvider<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector)
    {
        throw new NotImplementedException();
    }
}
