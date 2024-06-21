using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

public class CSharpTestMethod(
    IdentifierString identifier,
    ImmutableArray<AttributeDescriptor> attributes = default,
    ImmutableArray<ParameterDescriptor> parameters = default) : 
    TestMethod(identifier, attributes, parameters), IEquatable<CSharpTestMethod?>
{
    public override bool Equals(object obj) => Equals(obj as CSharpTestMethod);

    public bool Equals(CSharpTestMethod? other) => base.Equals(other);

    public override int GetHashCode() => base.GetHashCode();

    public void RenderTo(CSharpSourceTextBuilder sourceBuilder)
    {
        if (!Attributes.IsEmpty)
        {
            foreach (var attribute in Attributes)
            {
                sourceBuilder.AppendAttributeBlock(attribute);
                sourceBuilder.AppendLine();
            }
        }

        // Our test methods are always asynchronous and never return a value.
        sourceBuilder.Append("public async Task ").Append(Identifier);

        if (!Parameters.IsEmpty)
        {
            sourceBuilder.BeginBlock("(");

            var first = true;
            foreach (var parameter in Parameters)
            {
                if (!first)
                {
                    sourceBuilder.AppendLine(",");
                }

                sourceBuilder
                    .AppendTypeReference(parameter.Type)
                    .Append(' ')
                    .Append(parameter.Name);

                first = false;
            }

            sourceBuilder.EndBlock(")");
        }
        else
        {
            sourceBuilder.AppendLine("()");
        }

        sourceBuilder.BeginBlock("{");

        RenderMethodBodyTo(sourceBuilder);

        sourceBuilder.EndBlock("}");
    }

    protected virtual void RenderMethodBodyTo(CSharpSourceTextBuilder sourceBuilder)
    {

    }
}
