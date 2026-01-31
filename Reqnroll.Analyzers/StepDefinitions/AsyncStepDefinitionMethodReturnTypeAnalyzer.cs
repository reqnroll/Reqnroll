using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Reqnroll.Analyzers.StepDefinitions;

using static DiagnosticResources;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsyncStepDefinitionMethodReturnTypeAnalyzer : StepDefinitionMethodAnalyzer
{
    #pragma warning disable IDE0090 // Use 'new(...)' - full constructor syntax enables analyzer release tracking.
    internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticIds.AsyncStepDefinitionMethodMustReturnTask,
        CreateResourceString(nameof(AsyncStepDefinitionMethodMustReturnTaskTitle)),
        CreateResourceString(nameof(AsyncStepDefinitionMethodMustReturnTaskMessage)),
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: DocumentationHelper.GetRuleDocumentationUrl(DiagnosticIds.AsyncStepDefinitionMethodMustReturnTask));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void AnalyzeStepDefinitionMethod(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol)
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
