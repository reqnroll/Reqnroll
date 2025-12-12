using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Reqnroll.Analyzers.StepDefinitions;

using static DiagnosticResources;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsyncStepMethodReturnTypeAnalyzer : StepMethodAnalyzer
{
    #pragma warning disable IDE0090 // Use 'new(...)' - full constructor syntax enables analyzer release tracking.
    internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        "RR1004",
        CreateResourceString(nameof(AsyncStepMethodMustReturnTaskTitle)),
        CreateResourceString(nameof(AsyncStepMethodMustReturnTaskMessage)),
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Error,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void AnalyzeStepMethod(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol)
    {
        var methodSyntax = (MethodDeclarationSyntax)context.Node;

        if (methodSymbol.IsAsync && methodSymbol.ReturnsVoid)
        {
            var methodName = $"{methodSymbol.ContainingType.Name}.{methodSymbol.Name}";

            context.ReportDiagnostic(
                Diagnostic.Create(Rule, methodSyntax.Identifier.GetLocation(), methodName, "async"));
        }
    }
}
