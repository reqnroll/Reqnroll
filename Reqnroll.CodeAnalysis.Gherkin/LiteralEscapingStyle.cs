using Reqnroll.CodeAnalysis.Gherkin.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Reqnroll.CodeAnalysis.Gherkin;

internal abstract class LiteralEscapingStyle
{
    public static LiteralEscapingStyle Default { get; } = new DefaultLiteralEscapingStyle();

    public static LiteralEscapingStyle Table { get; } = new TableLiteralEscapingStyle();

    public string Escape(string value)
    {
        if (!RequiresEscaping(value))
        {
            return value;
        }

        var sb = new StringBuilder();

        foreach (var c in value)
        {
            if (TryEncode(c, out var encoded))
            {
                sb.Append(encoded);
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    protected abstract bool TryEncode(char c, [NotNullWhen(true)] out string? encoded);

    protected bool RequiresEscaping(string value) => value.Any(RequiresEscaping);

    protected abstract bool RequiresEscaping(char c);

    public abstract string Unescape(SourceTextSpan value);
}
