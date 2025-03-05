using Microsoft.CodeAnalysis;

namespace Reqnroll.StepBindingSourceGenerator;

internal static class DiagnosticDescriptors
{
    private static LocalizableResourceString Resource(string name) => 
        new(name, DiagnosticResources.ResourceManager, typeof(DiagnosticResources));

    public static DiagnosticDescriptor ErrorStepMethodMustReturnVoidOrTask { get; } = new DiagnosticDescriptor(
        "Reqnroll1000",
        Resource("ErrorStepMethodMustReturnVoidOrTaskTitle"),
        Resource("ErrorStepMethodMustReturnVoidOrTaskMessage"),
        "Reqnroll.Binding",
        DiagnosticSeverity.Error,
        true);

    public static DiagnosticDescriptor ErrorAsyncStepMethodMustReturnTask { get; } = new DiagnosticDescriptor(
        "Reqnroll1001",
        Resource("ErrorAsyncStepMethodMustReturnTaskTitle"),
        Resource("ErrorAsyncStepMethodMustReturnTaskMessage"),
        "Reqnroll.Binding",
        DiagnosticSeverity.Error,
        true);

    public static DiagnosticDescriptor ErrorStepTextCannotBeEmpty { get; } = new DiagnosticDescriptor(
        "Reqnroll1002",
        Resource("ErrorStepTextCannotBeEmptyTitle"),
        Resource("ErrorStepTextCannotBeEmptyMessage"),
        "Reqnroll.Binding",
        DiagnosticSeverity.Error,
        true);

    public static DiagnosticDescriptor WarningStepTextHasLeadingWhitespace { get; } = new DiagnosticDescriptor(
        "Reqnroll1003",
        Resource("WarningStepTextHasLeadingWhitespaceTitle"),
        Resource("WarningStepTextHasLeadingWhitespaceMessage"),
        "Reqnroll.Binding",
        DiagnosticSeverity.Warning,
        true);

    public static DiagnosticDescriptor WarningStepTextHasTrailingWhitespace { get; } = new DiagnosticDescriptor(
        "Reqnroll1004",
        Resource("WarningStepTextHasTrailingWhitespaceTitle"),
        Resource("WarningStepTextHasTrailingWhitespaceMessage"),
        "Reqnroll.Binding",
        DiagnosticSeverity.Warning,
        true);
}
