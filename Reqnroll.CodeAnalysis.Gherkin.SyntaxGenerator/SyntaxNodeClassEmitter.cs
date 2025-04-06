﻿
namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal class SyntaxNodeClassEmitter(SyntaxNodeClassInfo2 classInfo)
{
    public string EmitSyntaxNodeClass()
    {
        var builder = new CSharpBuilder();

        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");

        builder.Append("namespace ").Append(classInfo.ClassNamespace).AppendLine(';');

        builder.Append("public partial class ").AppendLine(classInfo.ClassName);
        builder.AppendBodyBlock(builder =>
        {
            AppendOrphanConstructorTo(builder);
            builder.AppendLine();

            AppendParentalConstructorTo(builder);
            builder.AppendLine();

            AppendInternalNodePropertyTo(builder);
            builder.AppendLine();

            foreach (var property in classInfo.SlotProperties)
            {
                AppendSlotPropertyTo(builder, property);
                builder.AppendLine();
            }

            AppendGetSlotAsSyntaxNodeMethodTo(builder);
            builder.AppendLine();

            var first = true;
            foreach (var property in classInfo.SlotProperties)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.AppendLine();
                }

                AppendWithMethodTo(builder, property);
            }
        });

        return builder.ToString();
    }

    private void AppendWithMethodTo(CSharpBuilder builder, SyntaxSlotInfo2 property)
    {
        var argumentName = NamingHelper.PascalCaseToCamelCase(property.Name);
        var descriptiveName = NamingHelper.PascalCaseToLowercaseWords(property.Name);

        builder.AppendLine("/// <summary>");
        builder.Append("/// Creates a new <see cref=\"").Append(classInfo.ClassName).AppendLine("\" /> instance that is");
        builder.Append("/// a copy of this syntax with the specified ").Append(descriptiveName).AppendLine('.');
        builder.AppendLine("/// </summary>");
        builder
            .Append("/// <param name=\"")
            .Append(argumentName)
            .Append("\">The ")
            .Append(descriptiveName)
            .AppendLine(" that should be used to create the syntax.</param>");

        builder
            .Append("public ")
            .Append(classInfo.ClassName)
            .Append(" With")
            .Append(property.Name)
            .Append('(')
            .Append(property.TypeName);

        if (property.NodeType == SyntaxNodeType.SyntaxNode)
        {
            builder.Append('?');
        }

        builder
            .Append(' ')
            .Append(argumentName)
            .AppendLine(")");

        builder.AppendBodyBlock(builder =>
        {
            builder
                .Append("if (")
                .Append(argumentName)
                .Append(" == ")
                .Append(property.Name)
                .AppendLine(") return this;");

            builder.AppendLine();

            builder.Append("return SyntaxFactory.").Append(classInfo.SyntaxKind.Name).AppendLine('(');
            builder.BeginBlock();

            var first = true;
            foreach (var slotProperty in classInfo.SlotProperties)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.AppendLine(',');
                }

                if (slotProperty == property)
                {
                    builder.Append(argumentName);
                }
                else
                {
                    builder.Append(slotProperty.Name);
                }
            }

            builder.AppendLine(");");

            builder.EndBlock();
        });
    }

    private void AppendInternalNodePropertyTo(CSharpBuilder builder)
    {
        builder
            .Append("internal new ")
            .Append(InternalNodeClassEmitter.ClassName)
            .Append(" InternalNode => (")
            .Append(InternalNodeClassEmitter.ClassName)
            .AppendLine(")base.InternalNode;");
    }

    private void AppendGetSlotAsSyntaxNodeMethodTo(CSharpBuilder builder)
    {
        builder.AppendLine("internal override SyntaxNode? GetSlotAsSyntaxNode(int index)");
        builder.AppendBodyBlock(builder =>
        {
            builder.AppendLine("return index switch");
            builder.AppendBlock("{", builder =>
            {
                foreach (var property in classInfo.SlotProperties
                    .Where(property => property.NodeType == SyntaxNodeType.SyntaxNode))
                {
                    builder.Append(property.Index.ToString()).Append(" => ").Append(property.Name).AppendLine(",");
                }

                builder.AppendLine("_ => null");
            }, "};");
        });
    }

    private void AppendSlotPropertyTo(CSharpBuilder builder, SyntaxSlotInfo2 property)
    {
        switch (property.NodeType)
        {
            case SyntaxNodeType.SyntaxToken:
                builder
                    .Append("public partial SyntaxToken ")
                    .Append(property.Name)
                    .Append(" => new (this, InternalNode.")
                    .Append(NamingHelper.PascalCaseToCamelCase(property.Name))
                    .Append(", Position + InternalNode.GetSlotOffset(")
                    .Append(property.Index.ToString())
                    .AppendLine("));");
                break;

            case SyntaxNodeType.SyntaxNode:
                builder
                    .Append("private ")
                    .Append(property.TypeName)
                    .Append("? _")
                    .Append(NamingHelper.PascalCaseToCamelCase(property.Name))
                    .AppendLine(";");

                builder
                    .Append("public partial ")
                    .Append(property.TypeName)
                    .Append("? ")
                    .Append(property.Name)
                    .Append(" => GetSyntaxNode(ref _")
                    .Append(NamingHelper.PascalCaseToCamelCase(property.Name))
                    .Append(", ")
                    .Append(property.Index.ToString())
                    .AppendLine(");");
                break;

            default:
                throw new InvalidOperationException();
        }
    }

    private void AppendParentalConstructorTo(CSharpBuilder builder)
    {
        builder
            .Append("internal ")
            .Append(classInfo.ClassName)
            .Append('(')
            .Append(InternalNodeClassEmitter.ClassName)
            .AppendLine(" node, SyntaxNode? parent, int position)");
        builder.BeginBlock();
        builder.AppendLine(": base(node, parent, position) {}");
        builder.EndBlock();
    }

    private void AppendOrphanConstructorTo(CSharpBuilder builder)
    {
        builder
            .Append("internal ")
            .Append(classInfo.ClassName)
            .Append('(')
            .Append(InternalNodeClassEmitter.ClassName)
            .AppendLine(" node)");
        builder.BeginBlock();
        builder.AppendLine(": base(node) {}");
        builder.EndBlock();
    }
}
