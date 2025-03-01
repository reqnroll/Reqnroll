using Microsoft.CodeAnalysis;

namespace Reqnroll.StepBindingSourceGenerator;

internal static class DiagnosticDescriptors
{
    private static LocalizableResourceString Resource(string name) => 
        new(name, DiagnosticResources.ResourceManager, typeof(DiagnosticResources));

    public static DiagnosticDescriptor ErrorStepTextCannotBeEmpty { get; } = new DiagnosticDescriptor(
        "Reqnroll.1000",
        Resource("ErrorStepTextCannotBeEmptyTitle"),
        Resource("ErrorStepTextCannotBeEmptyMessage"),
        "Reqnroll.Binding",
        DiagnosticSeverity.Error,
        true);
}
