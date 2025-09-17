using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Reqnroll.StepBindingSourceGenerator;

internal static class StepBindingIncrementalGeneratorInitializationContextExtensions
{
    public static IncrementalValuesProvider<StepDefinitionSyntaxInfo> GetStepDefinitionSyntaxInfo(
        this IncrementalGeneratorInitializationContext context,
        string fullyQualifiedAttributeName,
        StepKeywordMatch keywordMatch)
    {
        return context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedAttributeName,
                static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax,
                (context, cancellationToken) => ExtractStepDeclaration(context, keywordMatch, cancellationToken))
            .SelectMany((definitions, _) => definitions);
    }

    /// <summary>
    /// This method extracts the essential data for each attribute. The objective is to do as little work as possible 
    /// as this method is going to be called every time a Given, When, Then or StepDefinition attribute is modified
    /// or the method they're attached to is modified.
    /// </summary>
    /// <param name="context">The context containing the syntax information for matched attributes.</param>
    /// <param name="matchedKeywords">The keyword associated with the matched attributes.</param>
    /// <param name="cancellationToken">A token used to signal when the process should be cancelled.</param>
    /// <returns>An array of the step definition data extracted from the matched attributes.</returns>
    private static ComparableArray<StepDefinitionSyntaxInfo> ExtractStepDeclaration(
        GeneratorAttributeSyntaxContext context,
        StepKeywordMatch matchedKeywords,
        CancellationToken cancellationToken)
    {
        var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;
        var methodSymbol = (IMethodSymbol?)context.SemanticModel.GetDeclaredSymbol(methodSyntax);

        // If the semantic model does not contain the method, we can't bind to it.
        // This usually indicates a problem with the general syntax of this method.
        if (methodSymbol == null)
        {
            return ComparableArray<StepDefinitionSyntaxInfo>.Empty;
        }

        var result = ImmutableArray.CreateBuilder<StepDefinitionSyntaxInfo>();

        foreach (var attribute in context.Attributes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If the attribute candidate is an error symbol, skip it.
            if (attribute.AttributeClass == null || attribute.AttributeClass.Kind == SymbolKind.ErrorType)
            {
                continue;
            }

            // If the method is not public, it's not suitable for step binding.
            if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            // Process parameters and ensure all are valid for steps.
            var parameters = ImmutableArray.CreateBuilder<ParameterInfo>();
            var hasInvalidParameter = false;
            foreach (var parameter in methodSymbol.Parameters)
            {
                if (parameter.IsOptional)
                {
                    hasInvalidParameter = true;
                    break;
                }

                parameters.Add(new ParameterInfo(parameter.Name, parameter.Type.ToDisplayString()));
            }

            if (hasInvalidParameter)
            {
                continue;
            }

            // Get the text pattern from the attribute (if any exists.)
            string? textPattern = null;
            if (attribute.ConstructorArguments.Length > 0)
            {
                textPattern = attribute.ConstructorArguments[0].Value as string;
            }

            var syntaxInfo = new StepDefinitionSyntaxInfo(
                methodSyntax,
                new MethodInfo(
                    methodSymbol.Name,
                    new QualifiedTypeName(methodSymbol.ContainingNamespace.Name, methodSymbol.ContainingType.Name),
                    methodSymbol.ReturnsVoid,
                    methodSymbol.ReturnType.ToDisplayString() == "System.Threading.Tasks.Task",
                    methodSymbol.IsAsync,
                    parameters.ToImmutable()),
                matchedKeywords,
                textPattern);

            result.Add(syntaxInfo);
        }

        return result.ToImmutable();
    }
}
