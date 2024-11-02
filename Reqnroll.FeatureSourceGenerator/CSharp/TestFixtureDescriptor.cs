using System.Collections.Immutable;
using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

public class TestFixtureDescriptor
{
    public QualifiedTypeIdentifier? Identifier { get; set; }

    public string? HintName { get; set; }

    public FeatureInformation? Feature { get; set; }

    public ImmutableArray<TypeIdentifier> Interfaces { get; set; }

    public ImmutableArray<AttributeDescriptor> Attributes { get; set; }
}
