using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

public class CSharpSourceTextWriter
{
    class Buffer
    {
        private readonly StringBuilder _sb = new();

        public int Line { get; private set; }

        public int Character { get; private set; }

        public void Append(char c)
        {
            Debug.Assert(!Environment.NewLine.Contains(c), "Use WriteLine to append a line.");
            _sb.Append(c);
            Character++;
        }

        public void Append(string s)
        {
            Debug.Assert(!s.Contains(Environment.NewLine), "Use WriteLine to append a line.");
            _sb.Append(s);
            Character++;
        }

        public void AppendLine()
        {
            _sb.AppendLine();
            Character = 0;
            Line++;
        }

        public void AppendLine(char c)
        {
            _sb.Append(c);
            _sb.AppendLine();
            Character = 0;
            Line++;
        }

        public void AppendLine(string s)
        {
            _sb.AppendLine(s);
            Character = 0;
            Line++;
        }

        public override string ToString() => _sb.ToString();
    }

    class CodeBlock
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

    private static readonly UTF8Encoding Encoding = new(false);

    private readonly Buffer _buffer = new();

    private const string Indent = "    ";

    private CodeBlock _context = new();

    /// <summary>
    /// Gets the depth of the current block.
    /// </summary>
    public int Depth => _context.Depth;

    /// <summary>
    /// Gets the position of the writer.
    /// </summary>
    public LinePosition Position => new(_buffer.Line, _buffer.Character);

    /// <summary>
    /// Gets the number of characters which will be appended to the next new line in the current block.
    /// </summary>
    public int NewLineOffset => Indent.Length * Depth;

    public CSharpSourceTextWriter Write(char c)
    {
        WriteIndentIfIsFreshLine();

        _buffer.Append(c);
        return this;
    }

    public CSharpSourceTextWriter Write(string text)
    {
        WriteIndentIfIsFreshLine();

        _buffer.Append(text);
        return this;
    }

    public CSharpSourceTextWriter WriteLiteralList<T>(IEnumerable<T> values)
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
                Write(", ");
            }

            WriterLiteral(value);
        }

        return this;
    }

    public CSharpSourceTextWriter WriterLiteral(object? value)
    {
        return value switch
        {
            null => Write("null"),
            string s => WriteLiteral(s),
            ImmutableArray<string> array => WriteLiteralArray(array),
            ImmutableArray<object?> array => WriteLiteralArray(array),
            _ => throw new NotSupportedException($"Values of type {value.GetType().FullName} cannot be encoded as a literal value in C#.")
        };
    }

    public CSharpSourceTextWriter WriteLiteralArray<T>(ImmutableArray<T> array)
    {
        if (!CSharpSyntax.TypeAliases.TryGetValue(typeof(T), out var typeName))
        {
            typeName = $"global::{typeof(T).FullName}";
        }

        Write("new ").Write(typeName);

        if (array.Length == 0) 
        {
            return Write("[0]");
        }

        return Write("[] { ").WriteLiteralList(array).Write(" }");
    }

    public CSharpSourceTextWriter WriteLiteral(string? s)
    {
        if (s == null)
        {
            return Write("null");
        }

        var escapedValue = CSharpSyntax.FormatLiteral(s);

        Write(escapedValue);
        return this;
    }

    private void WriteIndentIfIsFreshLine()
    {
        if (_buffer.Character == 0)
        {
            for (var i = 0; i < Depth; i++)
            {
                _buffer.Append(Indent);
            }
        }
    }

    public CSharpSourceTextWriter WriteLineDirective(LinePosition start, LinePosition end, int offset, string filePath)
    {
        return WriteDirective($"#line ({start.Line}, {start.Character}) - ({end.Line}, {end.Character}) {offset} \"{filePath}\"");
    }

    public CSharpSourceTextWriter WriteDirective(string directive)
    {
        if (_buffer.Character != 0)
        {
            throw new InvalidOperationException(CSharpSourceTextWriterResources.CannotAppendDirectiveUnlessAtStartOfLine);
        }

        _buffer.AppendLine(directive);

        return this;
    }

    public CSharpSourceTextWriter WriteLine()
    {
        WriteIndentIfIsFreshLine();

        _buffer.AppendLine();

        return this;
    }

    public CSharpSourceTextWriter WriteLine(string text)
    {
        WriteIndentIfIsFreshLine();

        _buffer.AppendLine(text);

        return this;
    }

    /// <summary>
    /// Starts a new code block.
    /// </summary>
    public CSharpSourceTextWriter BeginBlock()
    {
        _context = new CodeBlock(_context);
        return this;
    }

    /// <summary>
    /// Appends the specified text and starts a new block.
    /// </summary>
    /// <param name="text">The text to append.</param>
    public CSharpSourceTextWriter BeginBlock(string text) => WriteLine(text).BeginBlock();

    /// <summary>
    /// Ends the current block and begins a new line.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <para>The builder is not currently in a block (the <see cref="Depth"/> is zero.)</para>
    /// </exception>
    public CSharpSourceTextWriter EndBlock()
    {
        if (_context.Parent == null)
        {
            throw new InvalidOperationException(CSharpSourceTextWriterResources.NotInCodeBlock);
        }

        _context = _context.Parent;
        return WriteLine();
    }

    /// <summary>
    /// Ends the current block, appends the specified text and begins a new line.
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <exception cref="InvalidOperationException">
    /// <para>The builder is not currently in a block (the <see cref="Depth"/> is zero.)</para>
    /// </exception>
    public CSharpSourceTextWriter EndBlock(string text)
    {
        if (_context.Parent == null)
        {
            throw new InvalidOperationException(CSharpSourceTextWriterResources.NotInCodeBlock);
        }

        _context = _context.Parent;
        return WriteLine(text);
    }

    /// <summary>
    /// Gets the value of this instance as a string.
    /// </summary>
    /// <returns>A string containing all text appended to the builder.</returns>
    public override string ToString() => _buffer.ToString();

    public SourceText ToSourceText()
    {
        return SourceText.From(_buffer.ToString(), Encoding);
    }

    public CSharpSourceTextWriter WriteAttributeBlock(AttributeDescriptor attribute)
    {
        Write('[');
        WriteTypeReference(attribute.Type);

        if (attribute.NamedArguments.Count > 0 || attribute.PositionalArguments.Length > 0)
        {
            Write('(');

            WriteLiteralList(attribute.PositionalArguments);

            var firstProperty = true;
            foreach (var (name, value) in attribute.NamedArguments)
            {
                if (firstProperty)
                {
                    if (!attribute.PositionalArguments.IsEmpty)
                    {
                        Write(", ");
                    }

                    firstProperty = false;
                }
                else
                {
                    Write(", ");
                }

                Write(name).Write(" = ").WriterLiteral(value);
            }

            Write(')');
        }

        Write(']');

        return this;
    }

    public CSharpSourceTextWriter WriteTypeReference(TypeIdentifier type)
    {
        return type switch
        {
            LocalTypeIdentifier localType => WriteTypeReference(localType),
            QualifiedTypeIdentifier qualifiedType => WriteTypeReference(qualifiedType),
            ArrayTypeIdentifier arrayType => WriteTypeReference(arrayType),
            NestedTypeIdentifier nestedType => WriteTypeReference(nestedType),
            _ => throw new NotImplementedException($"Appending references of type {type.GetType().Name} is not implemented.")
        };
    }

    public CSharpSourceTextWriter WriteTypeReference(LocalTypeIdentifier type)
    {
        return type switch
        {
            SimpleTypeIdentifier simpleType => WriteTypeReference(simpleType),
            GenericTypeIdentifier genericType => WriteTypeReference(genericType),
            _ => throw new NotImplementedException($"Appending references of type {type.GetType().Name} is not implemented.")
        };
    }

    public CSharpSourceTextWriter WriteTypeReference(NestedTypeIdentifier type)
    {
        return WriteTypeReference(type.EncapsulatingType).Write('.').WriteTypeReference(type.LocalType);
    }

    public CSharpSourceTextWriter WriteTypeReference(SimpleTypeIdentifier type)
    {
        Write(type.Name);

        if (type.IsNullable)
        {
            Write('?');
        }

        return this;
    }

    public CSharpSourceTextWriter WriteTypeReference(GenericTypeIdentifier type)
    {
        Write(type.Name);

        Write('<');

        WriteTypeReference(type.TypeArguments[0]);

        for (var i = 1;  i < type.TypeArguments.Length; i++)
        {
            Write(',');
            WriteTypeReference(type.TypeArguments[i]);
        }

        Write('>');

        if (type.IsNullable)
        {
            Write('?');
        }

        return this;
    }

    public CSharpSourceTextWriter WriteTypeReference(QualifiedTypeIdentifier type)
    {
        Write("global::").Write(type.Namespace).Write('.');

        WriteTypeReference(type.LocalType);

        return this;
    }

    public CSharpSourceTextWriter WriteTypeReference(ArrayTypeIdentifier type)
    {
        WriteTypeReference(type.ItemType).Write("[]");

        if (type.IsNullable)
        {
            Write('?');
        }

        return this;
    }
}
