using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

/// <summary>
/// A mutable description of a test method, used to help construct test method instances.
/// </summary>
public class TestMethodDescriptor
{
    public IdentifierString Identifier { get; set; }

    public ScenarioInformation? Scenario { get; set; }

    public ImmutableArray<StepInvocation> StepInvocations { get; set; }

    public ImmutableArray<AttributeDescriptor> Attributes { get; set; }

    public ImmutableArray<ParameterDescriptor> Parameters { get; set; }

    public ImmutableArray<KeyValuePair<string, IdentifierString>> ScenarioParameters { get; set; }
}
