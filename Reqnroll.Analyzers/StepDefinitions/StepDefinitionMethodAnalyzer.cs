using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reqnroll.Analyzers.StepDefinitions;

public abstract class StepDefinitionMethodAnalyzer : DiagnosticAnalyzer
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

        AnalyzeStepDefinitionMethod(context, methodSymbol);
    }

    protected abstract void AnalyzeStepDefinitionMethod(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol);
}
