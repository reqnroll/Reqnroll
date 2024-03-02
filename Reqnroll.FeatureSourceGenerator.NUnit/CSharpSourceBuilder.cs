using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Reqnroll.FeatureSourceGenerator.NUnit;

internal class CSharpSourceBuilder
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

    public CSharpSourceBuilder Append(string text)
    {
        AppendIndentIfIsFreshLine();

        _buffer.Append(text);
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
    public void BeginBlock()
    {
        AppendLine();
        _context = new CodeBlock(_context);
    }

    /// <summary>
    /// Appends the specified text and starts a new block.
    /// </summary>
    /// <param name="text">The text to append.</param>
    public void BeginBlock(string text) => Append(text).BeginBlock();

    /// <summary>
    /// Ends the current block and begins a new line.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <para>The builder is not currently in a block (the <see cref="Depth"/> is zero.)</para>
    /// </exception>
    public void EndBlock()
    {
        if (_context.Parent == null)
        {
            throw new InvalidOperationException(ExceptionMessages.CSharpSourceBuilderNotInCodeBlock);
        }

        _context = _context.Parent;
        AppendLine();
    }

    /// <summary>
    /// Ends the current block, appends the specified text and begins a new line.
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <exception cref="InvalidOperationException">
    /// <para>The builder is not currently in a block (the <see cref="Depth"/> is zero.)</para>
    /// </exception>
    public void EndBlock(string text)
    {
        if (_context.Parent == null)
        {
            throw new InvalidOperationException(ExceptionMessages.CSharpSourceBuilderNotInCodeBlock);
        }

        _context = _context.Parent;
        AppendLine(text);
    }

    /// <summary>
    /// Gets the value of this instance as a string.
    /// </summary>
    /// <returns>A string containing all text appended to the builder.</returns>
    public override string ToString() => _buffer.ToString();

    /// <summary>
    /// Marks the start of a section of code that should be separated from surrounding sections.
    /// </summary>
    public void BeginSection()
    {
        if (_context.InSection)
        {
            throw new InvalidOperationException(ExceptionMessages.CSharpSourceBuilderAlreadyInCodeSection);
        }

        _context.InSection = true;

        if (_context.HasSection)
        {
            AppendLine();
        }
        else
        {
            _context.HasSection = true;
        }
    }

    /// <summary>
    /// Marks the end of a section of code that should be separated from surrounding sections.
    /// </summary>
    public void EndSection()
    {
        if (!_context.InSection)
        {
            throw new InvalidOperationException(ExceptionMessages.CSharpSourceBuilderNotInCodeSection);
        }

        _context.InSection = false;
    }

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
