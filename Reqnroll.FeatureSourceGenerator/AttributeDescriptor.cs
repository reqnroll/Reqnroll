using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

public record AttributeDescriptor(
    string TypeName,
    string Namespace,
    ImmutableArray<object?> Arguments,
    ImmutableArray<KeyValuePair<string, object?>> PropertyValues)
{
    public AttributeDescriptor(
        string TypeName,
        string Namespace,
        ImmutableArray<object?>? Arguments = null,
        ImmutableArray<KeyValuePair<string, object?>>? PropertyValues = null) : this(
            TypeName,
            Namespace,
            Arguments ?? ImmutableArray<object?>.Empty,
            PropertyValues ?? ImmutableArray<KeyValuePair<string, object?>>.Empty)
    {
    }
}
