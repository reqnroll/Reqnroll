using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Reqnroll.FeatureSourceGenerator;

public class InMemoryAnalyzerConfigOptions(IEnumerable<KeyValuePair<string, string>> values) : AnalyzerConfigOptions
{
    private readonly ImmutableDictionary<string, string> _values = values.ToImmutableDictionary(KeyComparer);

    public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => _values.TryGetValue(key, out value);
}
