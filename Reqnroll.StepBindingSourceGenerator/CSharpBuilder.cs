using System.Text;

namespace Reqnroll.StepBindingSourceGenerator;

internal class CSharpBuilder
{
    private readonly StringBuilder _builder = new();

    private string? _linePrefix = null;

    private bool _onNewLine = true;

    private const string BlockIndent = "    ";

    public CSharpBuilder AppendBodyBlock(Action<CSharpBuilder> bodyAction) => AppendBlock('{', bodyAction, '}');

    public CSharpBuilder AppendBlock(char start, Action<CSharpBuilder> bodyAction, char end)
    {
        BeginBlock(start);
        bodyAction(this);
        return EndBlock(end);
    }

    public CSharpBuilder AppendLine()
    {
        _builder.AppendLine();
        _onNewLine = true;

        return this;
    }

    public CSharpBuilder AppendLine(char value) => Append(value).AppendLine();

    public CSharpBuilder AppendLine(string value) => Append(value).AppendLine();

    public CSharpBuilder Append(string value)
    {
        if (_onNewLine)
        {
            _builder.Append(_linePrefix);
            _onNewLine = false;
        }

        _builder.Append(value);
        return this;
    }

    public CSharpBuilder Append(char c)
    {
        if (_onNewLine)
        {
            _builder.Append(_linePrefix);
            _onNewLine = false;
        }

        _builder.Append(c);
        return this;
    }

    public CSharpBuilder BeginBlock()
    {
        if (_linePrefix is null)
        {
            _linePrefix = BlockIndent;
        }
        else
        {
            _linePrefix += BlockIndent;
        }

        return this;
    }

    public CSharpBuilder BeginBlock(string delimiter) => AppendLine(delimiter).BeginBlock();

    public CSharpBuilder BeginBlock(char delimiter) => AppendLine(delimiter).BeginBlock();

    public CSharpBuilder EndBlock()
    {
        if (_linePrefix is null)
        {
            throw new InvalidOperationException("Not currently in a block.");
        }

        if (_linePrefix.Length == BlockIndent.Length)
        {
            _linePrefix = null;
        }
        else
        {
            _linePrefix = _linePrefix.Substring(0, _linePrefix.Length - BlockIndent.Length);
        }

        return this;
    }

    public CSharpBuilder EndBlock(char delimiter) => EndBlock().AppendLine(delimiter);

    public CSharpBuilder EndBlock(string delimiter) => EndBlock().AppendLine(delimiter);

    public override string ToString() => _builder.ToString();
}
