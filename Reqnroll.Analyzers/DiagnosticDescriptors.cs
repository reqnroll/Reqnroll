namespace Reqnroll.Analyzers;
internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor ExpressionSyntaxNotValid = new(
        "REQ0001",
        "Expression syntax not valid",
        "Expression syntax not valid",
        "Usage",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor ExpressionParameterCountMismatch = new(
        "REQ0002",
        "Expression parameter count mismatch",
        "Expression parameter count mismatch",
        "Usage",
        DiagnosticSeverity.Warning,
        true);
}
