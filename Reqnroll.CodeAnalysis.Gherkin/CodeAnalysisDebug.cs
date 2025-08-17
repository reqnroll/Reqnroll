using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Reqnroll.CodeAnalysis.Gherkin;

[DebuggerStepThrough]
internal static class CodeAnalysisDebug
{
    /// <inheritdoc cref="Debug.Assert(bool, string)"/>
    [Conditional("DEBUG")]
    public static void Assert(bool condition, string message) => Debug.Assert(condition, message);

    /// <inheritdoc cref="Debug.Assert(bool, string, string, object[])"/>
    [Conditional("DEBUG")]
    public static void Assert(bool condition, string message, string detailMessageFormat, params object[] args) => 
        Debug.Assert(condition, message, detailMessageFormat, args);

    [Conditional("DEBUG")]
    public static void AssertNotNull(object? o, string name) => 
        Assert(o is not null, $"{name} cannot be null.");
}
