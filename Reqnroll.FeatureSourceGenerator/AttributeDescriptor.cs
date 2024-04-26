using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Provides a description of a .NET attribute.
/// </summary>
/// <param name="TypeName">The name of the attribute's type.</param>
/// <param name="Namespace">The namespace of the attribute's type.</param>
/// <param name="Arguments">The arguments passed to the attribute's constructor.</param>
/// <param name="PropertyValues">The property values of the attribute.</param>
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
