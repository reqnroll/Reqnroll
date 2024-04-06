using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Reqnroll.FeatureSourceGenerator;

public class CSharpSourceBuilder
{
    private static readonly UTF8Encoding Encoding = new(false);

    private readonly StringBuilder _buffer = new();

    private bool _isFreshLine = true;

    private const string Indent = "    ";

    private CodeBlock _context = new();

    /// <summary>
    /// Gets the depth of the current block.
    /// </summary>
    public int Depth => _context.Depth;

    public CSharpSourceBuilder Append(char c)
    {
        AppendIndentIfIsFreshLine();

        _buffer.Append(c);
        return this;
    }

    public CSharpSourceBuilder Append(string text)
    {
        AppendIndentIfIsFreshLine();

        _buffer.Append(text);
        return this;
    }

    public CSharpSourceBuilder AppendConstantList(IEnumerable<object?> values)
    {
        var first = true;

        foreach (var value in values)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                Append(", ");
            }

            AppendConstant(value);
        }

        return this;
    }

    public CSharpSourceBuilder AppendConstant(object? value)
    {
        return value switch
        {
            null => Append("null"),
            string s => AppendConstant(s),
            _ => throw new NotSupportedException($"Values of type {value.GetType().FullName} cannot be encoded as a constant in C#.")
        };
    }

    private CSharpSourceBuilder AppendConstant(string? s)
    {
        if (s == null)
        {
            return Append("null");
        }

        _buffer.Append('"').Append(s).Append('"');
        return this;
    }

    private void AppendIndentIfIsFreshLine()
    {
        if (_isFreshLine)
        {
            AppendIndentToDepth();

            _isFreshLine = false;
        }
    }

    private void AppendIndentToDepth()
    {
        for (var i = 0; i < Depth; i++)
        {
            _buffer.Append(Indent);
        }
    }

    internal void Reset() => _buffer.Clear();

    public CSharpSourceBuilder AppendDirective(string directive)
    {
        if (!_isFreshLine)
        {
            throw new InvalidOperationException(ExceptionMessages.CSharpSourceBuilderCannotAppendDirectiveUnlessAtStartOfLine);
        }

        _buffer.Append(directive);

        _buffer.AppendLine();

        _isFreshLine = true;
        return this;
    }

    public CSharpSourceBuilder AppendLine()
    {
        AppendIndentIfIsFreshLine();

        _buffer.AppendLine();

        _isFreshLine = true;
        return this;
    }

    public CSharpSourceBuilder AppendLine(string text)
    {
        AppendIndentIfIsFreshLine();

        _buffer.AppendLine(text);

        _isFreshLine = true;
        return this;
    }

    /// <summary>
    /// Starts a new code block.
    /// </summary>
    public CSharpSourceBuilder BeginBlock()
    {
        AppendLine();
        _context = new CodeBlock(_context);
        return this;
    }

    /// <summary>
    /// Appends the specified text and starts a new block.
    /// </summary>
    /// <param name="text">The text to append.</param>
    public CSharpSourceBuilder BeginBlock(string text) => Append(text).BeginBlock();

    /// <summary>
    /// Ends the current block and begins a new line.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <para>The builder is not currently in a block (the <see cref="Depth"/> is zero.)</para>
    /// </exception>
    public CSharpSourceBuilder EndBlock()
    {
        if (_context.Parent == null)
        {
            throw new InvalidOperationException(ExceptionMessages.CSharpSourceBuilderNotInCodeBlock);
        }

        _context = _context.Parent;
        return AppendLine();
    }

    /// <summary>
    /// Ends the current block, appends the specified text and begins a new line.
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <exception cref="InvalidOperationException">
    /// <para>The builder is not currently in a block (the <see cref="Depth"/> is zero.)</para>
    /// </exception>
    public CSharpSourceBuilder EndBlock(string text)
    {
        if (_context.Parent == null)
        {
            throw new InvalidOperationException(ExceptionMessages.CSharpSourceBuilderNotInCodeBlock);
        }

        _context = _context.Parent;
        return AppendLine(text);
    }

    /// <summary>
    /// Gets the value of this instance as a string.
    /// </summary>
    /// <returns>A string containing all text appended to the builder.</returns>
    public override string ToString() => _buffer.ToString();

    private class CodeBlock
    {
        public CodeBlock()
        {
            Depth = 0;
        }

        public CodeBlock(CodeBlock parent)
        {
            Parent = parent;
            Depth = parent.Depth + 1;
        }

        public int Depth { get; }

        public CodeBlock? Parent { get; }

        public bool InSection { get; set; }

        public bool HasSection { get; set; }
    }

    public SourceText ToSourceText()
    {
        return SourceText.From(_buffer.ToString(), Encoding);
    }
}
