using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

[Generator(LanguageNames.CSharp)]
public class StructuredTriviaGenerator : IIncrementalGenerator
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

        var bareStructuredTriviaClasses = context.SyntaxProvider.ForAttributeWithMetadataName(
            SyntaxTypes.SyntaxNodeAttribute,
            static (syntax, _) => syntax is ClassDeclarationSyntax cds &&
                cds.Modifiers.Any(token => token.IsKind(SyntaxKind.PartialKeyword)),
            static (context, cancellationToken) =>
            {
                var symbol = (ITypeSymbol)context.TargetSymbol;

                if (symbol.BaseType?.ToDisplayString() != SyntaxTypes.StructuredTriviaSyntax)
                {
                    return null;
                }

                var slots = SlotHelper.GetSlotProperties(symbol, cancellationToken);

                var syntaxKindValue = (ushort)context.Attributes.First().ConstructorArguments[0].Value!;

                return new BareSyntaxNodeClassInfo(
                    symbol.ContainingNamespace.ToDisplayString(),
                    symbol.Name,
                    syntaxKindValue,
                    symbol.BaseType?.ToDisplayString() ?? "",
                    slots,
                    symbol.IsAbstract);
            });

        var structuredTriviaClasses = bareStructuredTriviaClasses.Where(syntax => syntax != null).Combine(syntaxKinds)
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
                            info.IsAbstract,
                            info.ParameterGroups))
                        .ToImmutableArray());
            });

        context.RegisterSourceOutput(structuredTriviaClasses, static (context, classInfo) =>
        {
            var syntaxNodeEmitter = new StructuredTriviaClassEmitter(classInfo);
            context.AddSource(
                $"Syntax/{classInfo.ClassName}.g.cs",
                syntaxNodeEmitter.EmitSyntaxNodeClass());

            var internalSyntaxNodeEmitter = new InternalStructuredTriviaClassEmitter(classInfo)
            {
                BaseClassName = "InternalStructuredTriviaSyntax"
            };
            context.AddSource(
                $"Syntax/{classInfo.ClassName}.{InternalStructuredTriviaClassEmitter.ClassName}.g.cs",
                internalSyntaxNodeEmitter.EmitRawSyntaxNodeClass());

            var factoryMethodEmitter = new SyntaxFactoryMethodsEmitter(classInfo);
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
