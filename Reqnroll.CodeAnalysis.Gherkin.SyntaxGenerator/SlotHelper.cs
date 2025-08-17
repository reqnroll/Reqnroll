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
        var slots = new List<(IPropertySymbol Property, AttributeData Attribute)>();

        // By default we consider the properties of base types as appearing "before" more specific types.
        // Push the type hierarchy to a stack, so the most base type appears first.
        var hierarchy = new Stack<ITypeSymbol>();
        var typeSymbol = symbol;
        while (typeSymbol != null)
        {
            hierarchy.Push(typeSymbol);
            typeSymbol = typeSymbol.BaseType;
        }

        while (hierarchy.Count > 0)
        {
            typeSymbol = hierarchy.Pop();
            slots.AddRange(EnumerateSlotProperties(typeSymbol, cancellationToken));
        }

        // Re-order the slots based on any "LocatedAfter" values.
        foreach (var (property, attribute) in slots.ToList())
        {
            var locatedAfter = attribute.NamedArguments.FirstOrDefault(arg => arg.Key == "LocatedAfter").Value;

            if (locatedAfter.Kind != TypedConstantKind.Primitive)
            {
                continue;
            }

            var locatedAfterName = (string)locatedAfter.Value!;
            var locatedAfterProperty = slots.FirstOrDefault(slot => slot.Property.Name == locatedAfterName);

            if (locatedAfterProperty == default)
            {
                continue;
            }

            // Remove and re-insert the property after the designated property.
            var destinationIndex = slots.IndexOf(locatedAfterProperty) + 1;
            slots.Remove((property, attribute));

            if (destinationIndex > slots.Count)
            {
                slots.Add((property, attribute));
            }
            else
            {
                slots.Insert(destinationIndex, (property, attribute));
            }
        }

        // Create syntax property info.
        return slots
            .Select((tuple, index) => CreateBaseSyntaxSlotPropertyInfo(index, symbol, tuple.Property, tuple.Attribute)!)
            .Where(info => info != default)
            .ToImmutableArray();
    }

    private static BareSyntaxSlotPropertyInfo? CreateBaseSyntaxSlotPropertyInfo(
        int index,
        ITypeSymbol symbol,
        IPropertySymbol property,
        AttributeData slotAttribute)
    {
        ComparableArray<ushort> slotSyntaxKinds;

        var kindsArgument = slotAttribute.ConstructorArguments[0];

        if (kindsArgument.Kind == TypedConstantKind.Array)
        {
            var values = kindsArgument.Values.Select(value => value.Value).ToArray();

            if (values.Any(value => value is not ushort))
            {
                return default;
            }

            var slotSyntaxKindValues = values.Select(value => (ushort)value!).ToImmutableArray();

            slotSyntaxKinds = new ComparableArray<ushort>(slotSyntaxKindValues);
        }
        else
        {
            if (slotAttribute.ConstructorArguments[0].Value is not ushort slotSyntaxKindValue)
            {
                return default;
            }

            slotSyntaxKinds = ComparableArray.Create(slotSyntaxKindValue);
        }

        string? description = null;
        if (slotAttribute.ConstructorArguments.Length > 1)
        {
            description = slotAttribute.ConstructorArguments[1].Value as string;
        }

        SyntaxNodeType nodeType;
        string typeName = property.Type.Name;
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
                    typeName = $"SyntaxList<{((INamedTypeSymbol)property.Type).TypeArguments[0].Name}>";
                    isOptional = true;
                }
                else if (property.Type.IsTableCellSyntaxList())
                {
                    nodeType = SyntaxNodeType.SyntaxList;
                    typeName = "TableCellSyntaxList";
                    isOptional = true;
                }
                else
                {
                    return null;
                }

                break;
        }

        var attributes = property.GetAttributes();
        var parameterGroups = attributes
            .Where(attr => attr.AttributeClass?.ToDisplayString() == SyntaxTypes.ParameterGroupAttribute)
            .Select(attr => (string)attr.ConstructorArguments[0].Value!);

        return new BareSyntaxSlotPropertyInfo(
            nodeType,
            property.Name,
            index,
            typeName,
            slotSyntaxKinds,
            description,
            isOptional,
            !SymbolEqualityComparer.Default.Equals(property.ContainingType, symbol),
            property.IsAbstract,
            ComparableArray.CreateRange(parameterGroups));
    }

    private static IEnumerable<(IPropertySymbol Property, AttributeData Attribute)> EnumerateSlotProperties(
        ITypeSymbol symbol,
        CancellationToken cancellationToken)
    {
        foreach (var property in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (property.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            var attributes = property.GetAttributes();
            var slotAttribute = attributes
                .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == SyntaxTypes.SyntaxSlotAttribute);

            if (slotAttribute != null && slotAttribute.ConstructorArguments.Length > 0)
            {
                yield return (property, slotAttribute);
            }
        }
    }
}
