using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Reqnroll.Analyzers.StepDefinitions;

using static DiagnosticResources;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StepMethodMustReturnVoidOrTaskAnalyzer : DiagnosticAnalyzer
{
    private const string RuleId = "RR1002";

    #pragma warning disable IDE0090 // Use 'new(...)' - full constructor syntax enables analyzer release tracking.
    internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        RuleId,
        CreateResourceString(nameof(StepMethodMustReturnVoidOrTaskTitle)),
        CreateResourceString(nameof(StepMethodMustReturnVoidOrTaskMessage)),
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Error,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(Rule);

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

        var isStepDefintion = false;

        foreach (var attribute in methodSymbol.GetAttributes())
        {
            var stepAttributeTypes = new HashSet<string>
            {
                "Reqnroll.GivenAttribute",
                "Reqnroll.WhenAttribute",
                "Reqnroll.ThenAttribute",
                "Reqnroll.StepDefinitionAttribute"
            };

            if (attribute.AttributeClass == null)
            {
                continue;
            }

            if (stepAttributeTypes.Contains(attribute.AttributeClass.ToDisplayString()))
            {
                isStepDefintion = true;
                break;
            }
        }

        if (!isStepDefintion)
        {
            return;
        }

        if (methodSymbol.ReturnsVoid || methodSymbol.ReturnType.ToDisplayString() == "System.Threading.Tasks.Task")
        {
            return;
        }

        var methodName = $"{methodSymbol.ContainingType.Name}.{methodSymbol.Name}";

        context.ReportDiagnostic(
            Diagnostic.Create(Rule, methodSyntax.Identifier.GetLocation(), methodName, "void"));
    }
}
