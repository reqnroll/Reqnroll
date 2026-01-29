using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Reqnroll.Analyzers.StepDefinitions;

using static DiagnosticResources;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StepDefinitionExpressionAnalyzer : DiagnosticAnalyzer
{
    #pragma warning disable IDE0090 // Use 'new(...)' - full constructor syntax enables analyzer release tracking.
    internal static readonly DiagnosticDescriptor StepDefinitionExpressionCannotBeNullRule = 
        new DiagnosticDescriptor(
            "Reqnroll1001",
            CreateResourceString(nameof(StepDefinitionExpressionCannotBeNullTitle)),
            CreateResourceString(nameof(StepDefinitionExpressionCannotBeNullMessage)),
            DiagnosticCategory.Usage,
            DiagnosticSeverity.Error,
            true);

    internal static readonly DiagnosticDescriptor StepDefinitionExpressionCannotBeEmptyOrWhitespaceRule =
        new DiagnosticDescriptor(
            "Reqnroll1002",
            CreateResourceString(nameof(StepDefinitionExpressionCannotBeEmptyOrWhitespaceTitle)),
            CreateResourceString(nameof(StepDefinitionExpressionCannotBeEmptyOrWhitespaceMessage)),
            DiagnosticCategory.Usage,
            DiagnosticSeverity.Error,
            true);

    internal static readonly DiagnosticDescriptor StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceRule = 
        new DiagnosticDescriptor(
            "Reqnroll1003",
            CreateResourceString(nameof(StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceTitle)),
            CreateResourceString(nameof(StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceMessage)),
            DiagnosticCategory.Usage,
            DiagnosticSeverity.Error,
            true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            StepDefinitionExpressionCannotBeNullRule,
            StepDefinitionExpressionCannotBeEmptyOrWhitespaceRule,
            StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;        
        var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;

        if (attributeSymbol == null || !AttributeHelper.IsStepAttribute(attributeSymbol))
        {
            return;
        }

        var arguments = attributeSyntax.ArgumentList?.Arguments;
        if (arguments == null || arguments.Value.Count == 0)
        {
            return;
        }

        var firstArgument = arguments.Value[0];
        var constantValue = context.SemanticModel.GetConstantValue(firstArgument.Expression);

        if (!constantValue.HasValue || constantValue.Value == null)
        {
            context.ReportDiagnostic(Diagnostic.Create(StepDefinitionExpressionCannotBeNullRule, firstArgument.GetLocation()));
        }

        if (constantValue.Value is not string stepText)
        {
            return;
        }

        AnalyzeStepText(context, stepText, firstArgument);
    }

    private static void AnalyzeStepText(
        SyntaxNodeAnalysisContext context,
        string stepText,
        AttributeArgumentSyntax stepTextArgument)
    {
        if (stepText == null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(StepDefinitionExpressionCannotBeNullRule, stepTextArgument.GetLocation()));

            return;
        }
        
        if (string.IsNullOrWhiteSpace(stepText))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(StepDefinitionExpressionCannotBeEmptyOrWhitespaceRule, stepTextArgument.GetLocation()));

            return;
        }

        if (char.IsWhiteSpace(stepText[0]) || char.IsWhiteSpace(stepText[stepText.Length - 1]))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceRule, stepTextArgument.GetLocation()));
        }
    }
}
