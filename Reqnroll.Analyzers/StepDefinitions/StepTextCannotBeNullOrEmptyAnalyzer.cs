using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Reqnroll.Analyzers.StepDefinitions;

using static DiagnosticResources;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StepTextCannotBeNullOrEmptyAnalyzer : DiagnosticAnalyzer
{
    internal const string RuleId = "RR1001";

    #pragma warning disable IDE0090 // Use 'new(...)' - full constructor syntax enables analyzer release tracking.
    internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        RuleId,
        CreateResourceString(nameof(StepTextCannotBeNullOrEmptyTitle)),
        CreateResourceString(nameof(StepTextCannotBeNullOrEmptyMessage)),
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Error,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

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

        // Check the arguments passed to the WhenAttribute constructor
        var arguments = attributeSyntax.ArgumentList?.Arguments;
        if (arguments == null || arguments.Value.Count == 0)
        {
            return;
        }

        var firstArgument = arguments.Value[0];
        var constantValue = context.SemanticModel.GetConstantValue(firstArgument.Expression);

        if (!constantValue.HasValue || string.IsNullOrWhiteSpace(constantValue.Value?.ToString()))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, firstArgument.GetLocation()));
        }
    }
}
