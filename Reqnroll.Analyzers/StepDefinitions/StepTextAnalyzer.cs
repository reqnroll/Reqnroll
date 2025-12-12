using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Reqnroll.Analyzers.StepDefinitions;

using static DiagnosticResources;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StepTextAnalyzer : DiagnosticAnalyzer
{
    #pragma warning disable IDE0090 // Use 'new(...)' - full constructor syntax enables analyzer release tracking.
    internal static readonly DiagnosticDescriptor StepTextCannotBeNullOrEmptyRule = 
        new DiagnosticDescriptor(
            "RR1000",
            CreateResourceString(nameof(StepTextCannotBeNullOrEmptyTitle)),
            CreateResourceString(nameof(StepTextCannotBeNullOrEmptyMessage)),
            DiagnosticCategory.Usage,
            DiagnosticSeverity.Error,
            true);

    internal static readonly DiagnosticDescriptor StepTextShouldNotHaveLeadingWhitespaceRule = 
        new DiagnosticDescriptor(
            "RR1001",
            CreateResourceString(nameof(StepTextShouldNotHaveLeadingWhitespaceTitle)),
            CreateResourceString(nameof(StepTextShouldNotHaveLeadingWhitespaceMessage)),
            DiagnosticCategory.Usage,
            DiagnosticSeverity.Warning,
            true);

    internal static readonly DiagnosticDescriptor StepTextShouldNotHaveTrailingWhitespaceRule =
        new DiagnosticDescriptor(
            "RR1002",
            CreateResourceString(nameof(StepTextShouldNotHaveTrailingWhitespaceTitle)),
            CreateResourceString(nameof(StepTextShouldNotHaveTrailingWhitespaceMessage)),
            DiagnosticCategory.Usage,
            DiagnosticSeverity.Warning,
            true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            StepTextCannotBeNullOrEmptyRule,
            StepTextShouldNotHaveLeadingWhitespaceRule,
            StepTextShouldNotHaveTrailingWhitespaceRule);

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
            context.ReportDiagnostic(Diagnostic.Create(StepTextCannotBeNullOrEmptyRule, firstArgument.GetLocation()));
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
        if (string.IsNullOrWhiteSpace(stepText))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(StepTextCannotBeNullOrEmptyRule, stepTextArgument.GetLocation()));

            return;
        }

        if (char.IsWhiteSpace(stepText[0]))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(StepTextShouldNotHaveLeadingWhitespaceRule, stepTextArgument.GetLocation()));
        }

        if (char.IsWhiteSpace(stepText[stepText.Length - 1]))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(StepTextShouldNotHaveTrailingWhitespaceRule, stepTextArgument.GetLocation()));
        }
    }
}
