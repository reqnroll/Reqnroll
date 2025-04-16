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
                slotAttribute.ConstructorArguments.Length < 1 ||
                slotAttribute.ConstructorArguments[0].Value is not ushort slotSyntaxKindValue)
            {
                continue;
            }

            string? description = null;
            if (slotAttribute.ConstructorArguments.Length > 1)
            {
                description = slotAttribute.ConstructorArguments[1].Value as string;
            }

            SyntaxNodeType nodeType;

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
                    }
                    else
                    {
                        continue;
                    }

                    break;
            }

            slots.Add(
                new BareSyntaxSlotPropertyInfo(
                    nodeType,
                    property.Name,
                    slotIndex,
                    property.Type.Name,
                    slotSyntaxKindValue,
                    description));
        }

        return slots.ToImmutable();
    }
}
