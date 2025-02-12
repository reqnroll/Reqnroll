using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

/// <summary>
/// Represents diagnostic information which is not bound to an absolute location.
/// </summary>
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal class InternalDiagnostic
{
    public Diagnostic CreateDiagnosticWithoutLocation()
    {
        throw new NotImplementedException();
    }

    private string? GetDebuggerDisplay()
    {
        return ToString();
    }
}
