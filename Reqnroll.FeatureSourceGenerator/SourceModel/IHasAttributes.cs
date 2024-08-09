using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

/// <summary>
/// Defines a source element which has attributes.
/// </summary>
public interface IHasAttributes
{
    /// <summary>
    /// Gets the attributes of the source element.
    /// </summary>
    ImmutableArray<AttributeDescriptor> Attributes { get; }
}
