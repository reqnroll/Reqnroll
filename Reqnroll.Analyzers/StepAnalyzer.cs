using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Reqnroll.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StepAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.ExpressionSyntaxNotValid,
        DiagnosticDescriptors.ExpressionParameterCountMismatch);

    private static readonly ImmutableArray<string> AttributeTypeNames = ImmutableArray.Create(
        "Reqnroll.GivenAttribute",
        "Reqnroll.WhenAttribute",
        "Reqnroll.ThenAttribute",
        "Reqnroll.StepDefinitionAttribute");

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(analysisContext =>
        {
            var stepAttributeTypes = AttributeTypeNames
                .Select(name => analysisContext.Compilation.GetTypeByMetadataName(name))
                .Where(type => type != null)
                .Cast<INamedTypeSymbol>()
                .ToImmutableArray();

            analysisContext.RegisterSyntaxNodeAction(syntaxContext => 
                AnalyzeNode(syntaxContext, stepAttributeTypes), SyntaxKind.MethodDeclaration);
        });
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context, ImmutableArray<INamedTypeSymbol> stepAttributeTypes)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
        {
            return;
        }

        if (methodDeclaration.AttributeLists.Count == 0)
        {
            return;
        }

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);
        if (methodSymbol == null)
        {
            return;
        }

        var stepAttributes = methodSymbol.GetAttributes()
            .Where(attr => attr.AttributeClass != null)
            .Where(attr => stepAttributeTypes.Contains(attr.AttributeClass!));
        foreach (var attribute in stepAttributes)
        {
            // Ignore attributes with no text (how do these work?)
            if (attribute.ConstructorArguments.Length == 0)
            {
                continue;
            }

            // Extract the expression text - expected to be the first constructor argument.
            if (attribute.ConstructorArguments[0].Value is not string expressionText)
            {
                continue;
            }

            var parseResult = StepExpression.Parse(expressionText);

            if (parseResult is FailedParseResult<StepExpression> parseFailure)
            {
                // TODO: Raise syntax error in expression.
                continue;
            }

            var expression = ((SuccessParseResult<StepExpression>)parseResult).Value;

            if (expression.CaptureGroups.Length != methodSymbol.Parameters.Length)
            {
                // We want to highlight the text of the attribute, so we must obtain the location of the string syntax.
                var attributeSyntax = (AttributeSyntax)attribute.ApplicationSyntaxReference!.GetSyntax(context.CancellationToken);

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.ExpressionParameterCountMismatch,
                        attributeSyntax.ArgumentList!.Arguments[0].GetLocation()));
            }
        }
    }
}
