using Microsoft.CodeAnalysis;

namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor SyntaxConstructorContainsInvalidSlotName = new(
        "RRCAGEN1",
        "Syntax constructor contains invalid slot name",
        "Syntax constructor on '{0}' references slot '{1}' which does not exist. Known slots: {2}",
        "SyntaxGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
