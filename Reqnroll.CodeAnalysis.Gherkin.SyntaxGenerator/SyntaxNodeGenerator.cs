using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

[Generator(LanguageNames.CSharp)]
public class SyntaxNodeGenerator : IIncrementalGenerator
{
    const string SyntaxTokenType = "Reqnroll.CodeAnalysis.Gherkin.Syntax.SyntaxToken";
    const string SyntaxKindDescriptionAttributeType = "Reqnroll.CodeAnalysis.Gherkin.Syntax.SyntaxKindDescriptionAttribute";
    const string SyntaxSlotAttributeType = "Reqnroll.CodeAnalysis.Gherkin.Syntax.SyntaxSlotAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxKinds = context.SyntaxProvider.CreateSyntaxProvider(
            static (syntax, _) => syntax is EnumDeclarationSyntax declarationSyntax &&
                declarationSyntax.Identifier.ValueText == "SyntaxKind",
            static (context, _) =>
            {
                var declarationSyntax = (EnumDeclarationSyntax)context.Node;

                var enumSymbol = context.SemanticModel.GetDeclaredSymbol(declarationSyntax)!;

                var enumValues = enumSymbol.GetMembers().OfType<IFieldSymbol>();

                return enumValues.ToImmutableDictionary(
                    member =>
                    {
                        return (ushort)member.ConstantValue!;
                    },
                    member =>
                    {
                        var descriptionAttribute = member.GetAttributes()
                            .FirstOrDefault(attr => attr.AttributeClass!.ToDisplayString() == 
                                SyntaxKindDescriptionAttributeType);

                        return new SyntaxKindInfo(
                            (ushort)member.ConstantValue!,
                            member.Name,
                            (string?)descriptionAttribute?.ConstructorArguments[0].Value ??
                                $"a {NamingHelper.PascalCaseToLowercaseWords(member.Name)}");
                    });
            })
            .Collect()
            .Select((items, _) => items.Single())
            .WithComparer(ImmutableDictionaryComparer<ushort, SyntaxKindInfo>.Instance);

        var syntaxClasses = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Reqnroll.CodeAnalysis.Gherkin.Syntax.SyntaxNodeAttribute",
            static (syntax, _) => syntax is ClassDeclarationSyntax cds && 
                cds.Modifiers.Any(token => token.IsKind(SyntaxKind.PartialKeyword)),
            static (context, cancellationToken) =>
            {
                var symbol = (ITypeSymbol)context.TargetSymbol;

                var partialProperties = symbol.GetMembers().OfType<IPropertySymbol>()
                    .Where(property => property.DeclaredAccessibility == Accessibility.Public &&
                        property.DeclaringSyntaxReferences
                            .Any(reference => ((PropertyDeclarationSyntax)reference.GetSyntax(cancellationToken))
                                .Modifiers.Any(token => token.IsKind(SyntaxKind.PartialKeyword))))
                    .ToList();

                var slots = new List<SyntaxSlotInfo>();
                for (var slotIndex = 0; slotIndex < partialProperties.Count; slotIndex++)
                {
                    var property = partialProperties[slotIndex];

                    var attributes = property.GetAttributes();

                    var slotAttribute = attributes
                        .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == SyntaxSlotAttributeType);

                    if (slotAttribute == null)
                    {
                        continue;
                    }

                    var slotSyntaxKindValue = (ushort)slotAttribute.ConstructorArguments[0].Value!;

                    SyntaxNodeType nodeType;

                    if (property.Type.ToDisplayString() == SyntaxTokenType)
                    {
                        nodeType = SyntaxNodeType.SyntaxToken;
                    }
                    else if (property.Type.IsSyntaxNode())
                    {
                        nodeType = SyntaxNodeType.SyntaxNode;
                    }
                    else
                    {
                        continue;
                    }

                    slots.Add(
                        new SyntaxSlotInfo(
                            nodeType,
                            property.Name,
                            slotIndex,
                            property.Type.Name,
                            slotSyntaxKindValue));
                }

                var syntaxKindValue = (ushort)context.Attributes.First().ConstructorArguments[0].Value!;

                return new SyntaxNodeClassInfo(
                    context.TargetSymbol.ContainingNamespace.ToDisplayString(),
                    context.TargetSymbol.Name,
                    syntaxKindValue,
                    ImmutableArray.CreateRange(slots));
            });

        var syntaxClasses2 = syntaxClasses.Combine(syntaxKinds)
            .Select((tuple, _) =>
            {
                var (syntaxClass, syntaxKinds) = tuple;

                var syntaxKind = syntaxKinds[syntaxClass.SyntaxKind];

                return new SyntaxNodeClassInfo2(
                    syntaxClass.ClassNamespace,
                    syntaxClass.ClassName,
                    syntaxKind,
                    syntaxClass.SlotProperties
                        .Select(info => new SyntaxSlotInfo2(
                            info.NodeType,
                            info.Name,
                            info.Index,
                            info.TypeName,
                            syntaxKinds[info.SyntaxKind]))
                        .ToImmutableArray());
            });

        context.RegisterSourceOutput(syntaxClasses2, static (context, classInfo) =>
        {
            var syntaxNodeEmitter = new SyntaxNodeClassEmitter(classInfo);
            context.AddSource(
                $"Syntax/{classInfo.ClassName}.g.cs",
                syntaxNodeEmitter.EmitSyntaxNodeClass());

            var internalSyntaxNodeEmitter = new InternalNodeClassEmitter(classInfo);
            context.AddSource(
                $"Syntax/{classInfo.ClassName}.{InternalNodeClassEmitter.ClassName}.g.cs",
                internalSyntaxNodeEmitter.EmitRawSyntaxNodeClass());

            var factoryMethodEmitter = new SyntaxFactoryMethodEmitter(classInfo);
            context.AddSource(
                $"SyntaxFactory.{classInfo.ClassName}.g.cs",
                factoryMethodEmitter.EmitSyntaxFactoryMethod());

            var internalFactoryMethodEmitter = new InternalSyntaxFactoryMethodEmitter(classInfo);
            context.AddSource(
                $"Syntax/InternalSyntaxFactory.{classInfo.ClassName}.g.cs",
                internalFactoryMethodEmitter.EmitInternalSyntaxFactoryMethod());
        });
    }
}

internal record SyntaxKindInfo(int Value, string Name, string Description);

internal record SyntaxNodeClassInfo(
    string ClassNamespace,
    string ClassName,
    ushort SyntaxKind,
    ComparableArray<SyntaxSlotInfo> SlotProperties);

internal record SyntaxNodeClassInfo2(
    string ClassNamespace,
    string ClassName,
    SyntaxKindInfo SyntaxKind,
    ComparableArray<SyntaxSlotInfo2> SlotProperties);

internal record SyntaxSlotInfo(
    SyntaxNodeType NodeType,
    string Name,
    int Index,
    string TypeName,
    ushort SyntaxKind);

internal record SyntaxSlotInfo2(
    SyntaxNodeType NodeType,
    string Name,
    int Index,
    string TypeName,
    SyntaxKindInfo SyntaxKind);

internal enum SyntaxNodeType
{
    SyntaxToken,
    SyntaxNode
}

internal static class SymbolExtensions
{
    public static bool IsSyntaxNode(this ITypeSymbol symbol)
    {
        if (symbol.BaseType == null)
        {
            return false;
        }

        if (symbol.BaseType.ToDisplayString() == "Reqnroll.CodeAnalysis.Gherkin.Syntax.SyntaxNode")
        {
            return true;
        }

        return symbol.BaseType.IsSyntaxNode();
    }
}
