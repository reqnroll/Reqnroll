using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reqnroll.Analyzers.StepDefinitions;

public abstract class StepMethodAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodSyntax = (MethodDeclarationSyntax)context.Node;

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax, context.CancellationToken);
        if (methodSymbol == null)
        {
            return;
        }

        if (!methodSymbol.GetAttributes().Any(AttributeHelper.IsStepAttribute))
        {
            return;
        }

        AnalyzeStepMethod(context, methodSymbol);
    }

    protected abstract void AnalyzeStepMethod(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol);
}
