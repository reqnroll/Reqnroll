﻿namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal class InternalSyntaxFactoryMethodEmitter(SyntaxNodeClassInfo2 classInfo)
{
    public string EmitInternalSyntaxFactoryMethod()
    {
        var builder = new CSharpBuilder();

        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");

        builder.AppendLine();

        builder.AppendLine("namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;");

        builder.AppendLine("internal static partial class InternalSyntaxFactory");
        builder.AppendBodyBlock(builder =>
        {
            AppendFactoryMethodTo(builder);
        });

        return builder.ToString();
    }

    private void AppendFactoryMethodTo(CSharpBuilder builder)
    {
        builder
            .Append("public static ")
            .Append(classInfo.ClassName)
            .Append('.')
            .Append(InternalNodeClassEmitter.ClassName)
            .Append(' ')
            .Append(classInfo.SyntaxKind.Name)
            .AppendLine('(');

        builder.BeginBlock();

        var first = true;

        foreach (var property in classInfo.SlotProperties)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.AppendLine(',');
            }

            builder.Append("InternalNode");

            if (property.IsInternalNodeNullable)
            {
                builder.Append('?');
            }

            builder.Append(' ').Append(NamingHelper.PascalCaseToCamelCase(property.Name));
        }

        builder.AppendLine(')');
        builder.EndBlock();

        builder.AppendBodyBlock(builder =>
        {
            builder.AppendLine("return new(");

            builder.BeginBlock();

            var first = true;

            foreach (var property in classInfo.SlotProperties)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.AppendLine(',');
                }

                builder.Append(NamingHelper.PascalCaseToCamelCase(property.Name));
            }

            builder.AppendLine(");");

            builder.EndBlock();
        });
    }
}
