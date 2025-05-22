using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents diagnostic information which is not bound to an absolute location.
/// </summary>
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal class InternalDiagnostic
{
    private readonly DiagnosticDescriptor _descriptor;

    private readonly object?[]? _arguments;

    private InternalDiagnostic(DiagnosticDescriptor descriptor, object?[]? arguments)
    {
        _descriptor = descriptor;
        _arguments = arguments;
    }

    public static InternalDiagnostic Create(DiagnosticDescriptor descriptor) => new(descriptor, null);

    public static InternalDiagnostic Create(DiagnosticDescriptor descriptor, params object?[] arguments) => new(descriptor, arguments);

    public Diagnostic CreateDiagnostic(GherkinSyntaxTree syntaxTree, TextSpan textSpan)
    {
        var location = syntaxTree.GetLocation(textSpan);

        return Diagnostic.Create(_descriptor, location, _arguments);
    }

    public Diagnostic CreateDiagnostic() => Diagnostic.Create(_descriptor, null, _arguments);

    private string? GetDebuggerDisplay() => ToString();

    public override string ToString() => ToString(null);

    public string ToString(IFormatProvider? formatProvider)
    {
        return $"{_descriptor.DefaultSeverity}: {_descriptor.Id}: {GetMessage(formatProvider)}";
    }

    private string GetMessage(IFormatProvider? formatProvider)
    {
        var format = _descriptor.MessageFormat.ToString(formatProvider);

        if (_arguments != null && _arguments.Length > 0)
        {
            return string.Format(format, _arguments);
        }
        else
        {
            return format;
        }
    }
}
