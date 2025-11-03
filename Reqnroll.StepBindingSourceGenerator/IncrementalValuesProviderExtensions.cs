using Microsoft.CodeAnalysis;

namespace Reqnroll.StepBindingSourceGenerator;

internal static class IncrementalValuesProviderExtensions
{
    public static IncrementalValuesProvider<TValue> Concat<TValue>(
        this IncrementalValuesProvider<TValue> source,
        IncrementalValuesProvider<TValue> values)
    {
        return source
            .Collect()
            .Combine(values.Collect())
            .SelectMany(static (pair, _) => pair.Left.AddRange(pair.Right));
    }
}
