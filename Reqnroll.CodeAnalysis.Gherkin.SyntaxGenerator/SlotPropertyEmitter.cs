namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal class SlotPropertyEmitter(SyntaxSlotPropertyInfo property)
{
    public void EmitSlotPropertyTo(CSharpBuilder builder)
    {
        switch (property.NodeType)
        {
            case SyntaxNodeType.SyntaxToken:
            case SyntaxNodeType.SyntaxTokenList:
            case SyntaxNodeType.SyntaxList:
                AppendPropertySummaryTo(builder, property);
                builder
                    .Append("public partial ")
                    .Append(property.TypeName)
                    .Append(' ')
                    .Append(property.Name)
                    .Append(" => new(InternalNode.")
                    .Append(NamingHelper.PascalCaseToCamelCase(property.Name))
                    .Append(", this, GetSlotPosition(")
                    .Append(property.Index.ToString())
                    .AppendLine("));");
                break;

            case SyntaxNodeType.SyntaxNode:
                AppendSyntaxNodePropertyTo(builder);
                break;

            default:
                throw new NotImplementedException(
                    $"Support for syntax node type {property.NodeType} has not been added.");
        }
    }

    private void AppendSyntaxNodePropertyTo(CSharpBuilder builder)
    {
        // Emit a backing field for the property
        builder
            .Append("private ")
            .Append(property.TypeName)
            .Append("? _")
            .Append(NamingHelper.PascalCaseToCamelCase(property.Name))
            .AppendLine(";")
            .AppendLine();

        AppendPropertySummaryTo(builder, property);

        builder
            .Append("public partial ")
            .Append(property.TypeName);

        if (!property.IsRequired)
        {
            builder.Append('?');
        }

        builder
            .Append(' ')
            .Append(property.Name)
            .Append(" => ");

        // Add correct invocation to get the required or optional syntax node.
        if (property.IsRequired)
        {
            builder.Append("GetRequiredSyntaxNode");
        }
        else
        {
            builder.Append("GetSyntaxNode");
        }

        builder
            .Append("(ref _")
            .Append(NamingHelper.PascalCaseToCamelCase(property.Name))
            .Append(", ")
            .Append(property.Index.ToString())
            .AppendLine(");");
    }

    private void AppendPropertySummaryTo(CSharpBuilder builder, SyntaxSlotPropertyInfo property)
    {
        var summary = property.Description;
        if (summary != null)
        {
            builder.AppendLine("/// <summary>");
            builder.Append("/// Gets ").Append(char.ToLowerInvariant(summary[0])).AppendLine(summary.AsSpan(1));
            builder.AppendLine("/// </summary>");
        }
    }
}
