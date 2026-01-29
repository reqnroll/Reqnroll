using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Reqnroll.Analyzers.StepDefinitions;

using static DiagnosticResources;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StepDefinitionMethodReturnTypeAnalyzer : StepDefinitionMethodAnalyzer
{
    #pragma warning disable IDE0090 // Use 'new(...)' - full constructor syntax enables analyzer release tracking.
    internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        "Reqnroll1021",
        CreateResourceString(nameof(StepDefinitionMethodShouldReturnVoidOrTaskOrValueTaskTitle)),
        CreateResourceString(nameof(StepDefinitionMethodShouldReturnVoidOrTaskOrValueTaskMessage)),
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: DocumentationHelper.GetRuleDocumentationUrl("Reqnroll1021"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(Rule);

    protected override void AnalyzeStepDefinitionMethod(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol)
    {
        var methodSyntax = (MethodDeclarationSyntax)context.Node;

        if (methodSymbol.ReturnsVoid 
            || methodSymbol.ReturnType.ToDisplayString() == "System.Threading.Tasks.Task"
            || methodSymbol.ReturnType.ToDisplayString() == "System.Threading.Tasks.ValueTask")
        {
            return;
        }

        var methodName = $"{methodSymbol.ContainingType.Name}.{methodSymbol.Name}";

        context.ReportDiagnostic(
            Diagnostic.Create(Rule, methodSyntax.Identifier.GetLocation(), methodName, "void"));
    }
}
