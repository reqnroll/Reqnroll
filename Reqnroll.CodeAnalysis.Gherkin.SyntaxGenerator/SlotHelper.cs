using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal static class SlotHelper
{
    public static ImmutableArray<BareSyntaxSlotPropertyInfo> GetSlotProperties(
        ITypeSymbol symbol,
        CancellationToken cancellationToken)
    {
        var slots = ImmutableArray.CreateBuilder<BareSyntaxSlotPropertyInfo>();

        var partialProperties = symbol.GetMembers().OfType<IPropertySymbol>()
            .Where(property => property.DeclaredAccessibility == Accessibility.Public &&
                property.DeclaringSyntaxReferences
                    .Any(reference => ((PropertyDeclarationSyntax)reference.GetSyntax(cancellationToken))
                        .Modifiers.Any(token => token.IsKind(SyntaxKind.PartialKeyword))))
            .ToList();

        for (var slotIndex = 0; slotIndex < partialProperties.Count; slotIndex++)
        {
            var property = partialProperties[slotIndex];

            var attributes = property.GetAttributes();

            var slotAttribute = attributes
                .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == SyntaxTypes.SyntaxSlotAttribute);

            if (slotAttribute == null ||
                slotAttribute.ConstructorArguments.Length < 1)
            {
                continue;
            }

            ComparableArray<ushort> slotSyntaxKinds;

            var kindsArgument = slotAttribute.ConstructorArguments[0];

            if (kindsArgument.Kind == TypedConstantKind.Array)
            {
                var values = kindsArgument.Values.Select(value => value.Value).ToArray();

                if (values.Any(value => value is not ushort))
                {
                    continue;
                }

                var slotSyntaxKindValues = values.Select(value => (ushort)value!).ToImmutableArray();

                slotSyntaxKinds = new ComparableArray<ushort>(slotSyntaxKindValues);
            }
            else
            {
                if (slotAttribute.ConstructorArguments[0].Value is not ushort slotSyntaxKindValue)
                {
                    continue;
                }

                slotSyntaxKinds = ComparableArray.Create(slotSyntaxKindValue);
            }

            string? description = null;
            if (slotAttribute.ConstructorArguments.Length > 1)
            {
                description = slotAttribute.ConstructorArguments[1].Value as string;
            }

            SyntaxNodeType nodeType;
            bool isOptional = false;

            switch (property.Type.ToDisplayString())
            {
                case SyntaxTypes.SyntaxToken:
                    nodeType = SyntaxNodeType.SyntaxToken;
                    break;
                case SyntaxTypes.SyntaxTokenList:
                    nodeType = SyntaxNodeType.SyntaxTokenList;
                    break;
                default:
                    if (property.Type.IsSyntaxNode())
                    {
                        nodeType = SyntaxNodeType.SyntaxNode;
                        isOptional = property.Type.NullableAnnotation == NullableAnnotation.Annotated;
                    }
                    else if (property.Type.IsSyntaxList())
                    {
                        nodeType = SyntaxNodeType.SyntaxList;
                    }
                    else
                    {
                        continue;
                    }

                    break;
            }

            var parameterGroups = attributes
                .Where(attr => attr.AttributeClass?.ToDisplayString() == SyntaxTypes.ParameterGroupAttribute)
                .Select(attr => (string)attr.ConstructorArguments[0].Value!);

            slots.Add(
                new BareSyntaxSlotPropertyInfo(
                    nodeType,
                    property.Name,
                    slotIndex,
                    property.Type.Name,
                    slotSyntaxKinds,
                    description,
                    isOptional,
                    ComparableArray.CreateRange(parameterGroups)));
        }

        return slots.ToImmutable();
    }
}
