using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

/// <summary>
/// Represents a class which is a test fixture to execute the scenarios associated with a feature.
/// </summary>
public class CSharpTestFixtureClass
{
    public CSharpTestFixtureClass(
        TypeIdentifier identifier,
        ImmutableArray<AttributeDescriptor> attributes = default,
        ImmutableArray<CSharpScenarioMethod> methods = default)
    {
        if (identifier.IsEmpty)
        {
            throw new ArgumentException("Value cannot be an empty identifier.", nameof(identifier));
        }

        Identifier = identifier;

        Attributes = attributes.IsDefault ? ImmutableArray<AttributeDescriptor>.Empty : attributes;
        Methods = methods.IsDefault ? ImmutableArray<CSharpScenarioMethod>.Empty : methods;
    }

    public TypeIdentifier Identifier { get; }
    public ImmutableArray<AttributeDescriptor> Attributes { get; }
    public ImmutableArray<CSharpScenarioMethod> Methods { get; }

    public void WriteTo(CSharpSourceTextBuilder sourceBuilder)
    {
        sourceBuilder.Append("namespace ").Append(Identifier.Namespace.ToString()).AppendLine();
        sourceBuilder.BeginBlock("{");

        if (!Attributes.IsEmpty)
        {
            foreach (var attribute in Attributes)
            {
                sourceBuilder.AppendAttributeBlock(attribute);
                sourceBuilder.AppendLine();
            }
        }

        sourceBuilder.Append("public class ").Append(Identifier.LocalName.ToString());
        sourceBuilder.BeginBlock("{");

        WriteTestFixturePreambleTo(sourceBuilder);

        sourceBuilder.AppendLine();

        WriteScenarioMethodsTo(sourceBuilder);

        sourceBuilder.EndBlock("}");
        sourceBuilder.EndBlock("}");
    }

    protected virtual void WriteTestFixturePreambleTo(CSharpSourceTextBuilder sourceBuilder)
    {
        throw new NotImplementedException();
    }

    protected virtual void WriteScenarioMethodsTo(CSharpSourceTextBuilder sourceBuilder)
    {
        var first = true;
        foreach (var method in Methods)
        {
            if (!first)
            {
                sourceBuilder.AppendLine();
            }

            method.WriteTo(sourceBuilder);

            if (first)
            {
                first = false;
            }
        }
    }
}

public abstract class CSharpScenarioMethod(string name)
{
    public string Name { get; } = name;

    public void WriteTo(CSharpSourceTextBuilder sourceBuilder)
    {
        throw new NotImplementedException();
    }
}
