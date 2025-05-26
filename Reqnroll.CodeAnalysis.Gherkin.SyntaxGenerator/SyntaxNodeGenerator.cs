using Microsoft.CodeAnalysis;
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

                ImmutableArray<BareSyntaxSlotPropertyInfo> slots;

                switch (symbol.BaseType?.ToDisplayString())
                {
                    case SyntaxTypes.StructuredTriviaSyntax:
                        return null;

                    case SyntaxTypes.SyntaxNode:
                        slots = SlotHelper.GetSlotProperties(symbol, cancellationToken);
                        break;

                    default:
                        if (!symbol.IsSyntaxNode())
                        {
                            return null;
                        }

                        slots = SlotHelper.GetSlotProperties(symbol, cancellationToken);

                        break;
                }

                var syntaxNodeAttribute = context.Attributes[0];

                var syntaxKindValue = syntaxNodeAttribute.ConstructorArguments.Length == 0 ?
                    (ushort)0 :
                    syntaxNodeAttribute.ConstructorArguments[0].Value as ushort? ?? 0;

                return new BareSyntaxNodeClassInfo(
                    symbol.ContainingNamespace.ToDisplayString(),
                    symbol.Name,
                    syntaxKindValue,
                    symbol.BaseType?.Name!,
                    slots,
                    symbol.IsAbstract);
            });

        var syntaxNodeClasses = bareSyntaxNodeClasses
            .Where(syntax => syntax != null && !syntax.IsAbstract)
            .Combine(syntaxKinds)
            .Select((tuple, _) =>
            {
                var (syntaxClass, syntaxKinds) = tuple;

                var syntaxKind = syntaxKinds[syntaxClass!.SyntaxKind];

                return new SyntaxNodeClassInfo(
                    syntaxClass.ClassNamespace,
                    syntaxClass.ClassName,
                    syntaxKind,
                    syntaxClass.BaseClassName,
                    syntaxClass.SlotProperties
                        .Select(info => new SyntaxSlotPropertyInfo(
                            info.NodeType,
                            info.Name,
                            info.Index,
                            info.TypeName,
                            ComparableArray.CreateRange(info.SyntaxKinds.Select(kind => syntaxKinds[kind])),
                            info.Description, 
                            !info.IsOptional,
                            info.IsInherited,
                            info.ParameterGroups))
                        .ToImmutableArray());
            });

        var baseSyntaxNodeClasses = bareSyntaxNodeClasses
            .Where(syntax => syntax != null && syntax.IsAbstract)
            .Combine(syntaxKinds)
            .Select((tuple, _) =>
            {
                var (syntaxClass, syntaxKinds) = tuple;

                var syntaxKind = syntaxKinds[syntaxClass!.SyntaxKind];

                return new SyntaxNodeClassInfo(
                    syntaxClass.ClassNamespace,
                    syntaxClass.ClassName,
                    syntaxKind,
                    syntaxClass.BaseClassName,
                    syntaxClass.SlotProperties
                        .Select(info => new SyntaxSlotPropertyInfo(
                            info.NodeType,
                            info.Name,
                            info.Index,
                            info.TypeName,
                            ComparableArray.CreateRange(info.SyntaxKinds.Select(kind => syntaxKinds[kind])),
                            info.Description,
                            !info.IsOptional,
                            info.IsInherited,
                            info.ParameterGroups))
                        .ToImmutableArray());
            });

        context.RegisterSourceOutput(syntaxNodeClasses, static (context, classInfo) =>
        {
            context.AddSource(
                $"Syntax/{classInfo.ClassName}.g.cs",
                new SyntaxNodeClassEmitter(classInfo).EmitSyntaxNodeClass());

            context.AddSource(
                $"Syntax/{classInfo.ClassName}.{InternalNodeClassEmitter.ClassName}.g.cs",
                new InternalNodeClassEmitter(classInfo).EmitInternalSyntaxNodeClass());

            context.AddSource(
                $"SyntaxFactory.{classInfo.ClassName}.g.cs",
                new SyntaxFactoryMethodsEmitter(classInfo).EmitSyntaxFactoryMethod());

            context.AddSource(
                $"Syntax/InternalSyntaxFactory.{classInfo.ClassName}.g.cs",
                new InternalSyntaxFactoryMethodEmitter(classInfo).EmitInternalSyntaxFactoryMethod());
        });

        context.RegisterSourceOutput(baseSyntaxNodeClasses, static (context, classInfo) =>
        {
            context.AddSource(
                $"Syntax/{classInfo.ClassName}.g.cs",
                new BaseSyntaxNodeClassEmitter(classInfo).EmitSyntaxNodeClass());

            context.AddSource(
                $"Syntax/{classInfo.ClassName}.{InternalNodeClassEmitter.ClassName}.g.cs",
                new BaseInternalNodeClassEmitter(classInfo).EmitRawSyntaxNodeClass());
        });
    }
}
