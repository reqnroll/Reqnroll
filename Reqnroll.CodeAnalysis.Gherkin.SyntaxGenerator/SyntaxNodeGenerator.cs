﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

[Generator(LanguageNames.CSharp)]
public class SyntaxNodeGenerator : IIncrementalGenerator
{
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
                    member => (ushort)member.ConstantValue!,
                    member => new SyntaxKindInfo((ushort)member.ConstantValue!, member.Name));
            })
            .Collect()
            .Select((items, _) => items.Single())
            .WithComparer(ImmutableDictionaryComparer<ushort, SyntaxKindInfo>.Instance);

        var bareSyntaxNodeClasses = context.SyntaxProvider.ForAttributeWithMetadataName(
            SyntaxTypes.SyntaxNodeAttribute,
            static (syntax, _) => syntax is ClassDeclarationSyntax cds &&
                cds.Modifiers.Any(token => token.IsKind(SyntaxKind.PartialKeyword)),
            static (context, cancellationToken) =>
            {
                var symbol = (ITypeSymbol)context.TargetSymbol;

                if (symbol.BaseType?.ToDisplayString() != SyntaxTypes.SyntaxNode)
                {
                    return null;
                }

                var slots = SlotHelper.GetSlotProperties(symbol, cancellationToken);

                var syntaxKindValue = (ushort)context.Attributes.First().ConstructorArguments[0].Value!;

                return new BareSyntaxNodeClassInfo(
                    context.TargetSymbol.ContainingNamespace.ToDisplayString(),
                    context.TargetSymbol.Name,
                    syntaxKindValue,
                    slots);
            });

        var syntaxNodeClasses = bareSyntaxNodeClasses.Where(syntax => syntax != null).Combine(syntaxKinds)
            .Select((tuple, _) =>
            {
                var (syntaxClass, syntaxKinds) = tuple;

                var syntaxKind = syntaxKinds[syntaxClass!.SyntaxKind];

                return new SyntaxNodeClassInfo(
                    syntaxClass.ClassNamespace,
                    syntaxClass.ClassName,
                    syntaxKind,
                    syntaxClass.SlotProperties
                        .Select(info => new SyntaxSlotPropertyInfo(
                            info.NodeType,
                            info.Name,
                            info.Index,
                            info.TypeName,
                            syntaxKinds[info.SyntaxKind],
                            info.Description))
                        .ToImmutableArray());
            });

        context.RegisterSourceOutput(syntaxNodeClasses, static (context, classInfo) =>
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

internal record SyntaxKindInfo(int Value, string Name);

internal record BareSyntaxNodeClassInfo(
    string ClassNamespace,
    string ClassName,
    ushort SyntaxKind,
    ComparableArray<BareSyntaxSlotPropertyInfo> SlotProperties);

internal record SyntaxNodeClassInfo(
    string ClassNamespace,
    string ClassName,
    SyntaxKindInfo SyntaxKind,
    ComparableArray<SyntaxSlotPropertyInfo> SlotProperties);

internal record BareSyntaxSlotPropertyInfo(
    SyntaxNodeType NodeType,
    string Name,
    int Index,
    string TypeName,
    ushort SyntaxKind,
    string? Description);

internal record SyntaxSlotPropertyInfo(
    SyntaxNodeType NodeType,
    string Name,
    int Index,
    string TypeName,
    SyntaxKindInfo SyntaxKind,
    string? Description)
{
    public bool IsInternalNodeNullable => NodeType == SyntaxNodeType.SyntaxNode || NodeType == SyntaxNodeType.SyntaxTokenList;
}

internal enum SyntaxNodeType
{
    SyntaxToken,
    SyntaxTokenList,
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
