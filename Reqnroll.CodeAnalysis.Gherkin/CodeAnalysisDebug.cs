using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin;

internal static class CodeAnalysisDebug
{
    /// <summary>
    /// Checks for a condition; if the condition is false, an exception is thrown.
    /// </summary>
    /// <param name="condition">An expression to evaluate.</param>
    /// <param name="message">A message to include if the expression specified in 
    /// <paramref name="condition"/> is <c>true</c>.</param>
    [Conditional("DEBUG")]
    public static void Assert(bool condition, string message) => Debug.Assert(condition, message);
}
