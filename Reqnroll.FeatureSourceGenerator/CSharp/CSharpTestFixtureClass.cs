using System.Collections.Immutable;
using System.Text;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

/// <summary>
/// Represents a class which is a test fixture to execute the scenarios associated with a feature.
/// </summary>
public class CSharpTestFixtureClass : TestFixtureClass
{ 
    private static readonly Encoding Encoding = new UTF8Encoding(false);

    public CSharpTestFixtureClass(
        TypeIdentifier identifier,
        string hintName,
        ImmutableArray<AttributeDescriptor> attributes = default,
        ImmutableArray<CSharpTestMethod> methods = default) :
        base(
            identifier,
            hintName,
            attributes)
    {
        Methods = methods.IsDefault ? ImmutableArray<CSharpTestMethod>.Empty : methods;
    }

    public ImmutableArray<CSharpTestMethod> Methods { get; }

    public override IEnumerable<TestMethod> GetMethods() => Methods;

    public override SourceText Render()
    {
        var buffer = new CSharpSourceTextBuilder();

        RenderTo(buffer);

        return SourceText.From(buffer.ToString(), Encoding);
    }

    public void RenderTo(CSharpSourceTextBuilder sourceBuilder)
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

        RenderTestFixturePreambleTo(sourceBuilder);

        sourceBuilder.AppendLine();

        RenderMethodsTo(sourceBuilder);

        sourceBuilder.EndBlock("}");
        sourceBuilder.EndBlock("}");
    }

    protected virtual void RenderTestFixturePreambleTo(CSharpSourceTextBuilder sourceBuilder)
    {
        throw new NotImplementedException();
    }

    protected virtual void RenderMethodsTo(CSharpSourceTextBuilder sourceBuilder)
    {
        var first = true;
        foreach (var method in Methods)
        {
            if (!first)
            {
                sourceBuilder.AppendLine();
            }

            method.RenderTo(sourceBuilder);

            if (first)
            {
                first = false;
            }
        }
    }
}
