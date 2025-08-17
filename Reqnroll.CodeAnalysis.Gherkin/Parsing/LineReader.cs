using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

/// <summary>
/// Provides a lightweight abstraction to perform character-by-character reading of a line.
/// </summary>
internal class LineReader
{
    private readonly TextLine _line;

    /// <summary>
    /// Initializes a new <see cref="LineReader"/> structure.
    /// </summary>
    /// <param name="line">The line of source text to read.</param>
    public LineReader(TextLine line)
    {
        CodeAnalysisDebug.AssertNotNull(line.Text, "line.Text");
        _line = line;

        Position = line.Start - 1;
    }

    public int Position { get; private set; }

    public char Current
    {
        get
        {
            CodeAnalysisDebug.Assert(
                Position >= _line.Start && Position < _line.End,
                "Position must be within the line's range.");

            return Source[Position];
        }
    }

    private SourceText Source => _line.Text!;

    public void Reset() => Position = _line.Start - 1;

    /// <summary>
    /// Advances the reader to the next character.
    /// </summary>
    /// <returns><c>true</c> if the next character was read, <c>false</c> if there are no more characters in the line.</returns>
    public bool MoveNext()
    {
        if (Position == _line.End)
        {
            return false;
        }

        Position++;

        return Position < _line.End;
    }

    /// <summary>
    /// Advances the reader past multiple characters. 
    /// </summary>
    /// <param name="count">The number of characters to skip.</param>
    /// <returns><c>true</c> if there are additional characters in the line; otherwise <c>false</c>.</returns>
    public bool Skip(int count) => SkipTo(Position + count);

    /// <summary>
    /// Advances the reader until a specific position is reached.
    /// </summary>
    /// <param name="position">The position to move to. If this is before the current position, the reader will not move.</param>
    /// <returns><c>true</c> if there are additional characters in the line; otherwise <c>false</c>.</returns>
    public bool SkipTo(int position)
    {
        if (position > Position)
        {
            CodeAnalysisDebug.Assert(position <= _line.End, "Position must be within the line's range.");

            Position = position;
        }

        return Position < _line.End;
    }

    /// <summary>
    /// Moves the reader to the specified position.
    /// </summary>
    /// <param name="position">The position to move to.</param>
    /// <returns><c>true</c> if the position is within the line; otherwise <c>false</c>.</returns>
    public bool Seek(int position)
    {
        Position = position;
        return position < _line.End && Position >= _line.Start;
    }

    /// <summary>
    /// Advances the reader until the specified character is found or the end of the line is reached.
    /// </summary>
    /// <param name="c">A character to find.</param>
    /// <returns><c>true</c> if a matching character was found; otherwise <c>false</c>.</returns>
    public bool SkipUntil(char c) => SkipUntil(x => x == c);

    /// <summary>
    /// Advances the reader until one of the specified characters are found.
    /// </summary>
    /// <param name="c1">A character to find.</param>
    /// <param name="c2">A character to find.</param>
    /// <returns><c>true</c> if a matching character was found; otherwise <c>false</c>.</returns>
    public bool SkipUntil(char c1, char c2) => SkipUntil(x => x == c1 || x == c2);
    
    /// <summary>
    /// Advances the reader until one of the specified characters are found.
    /// </summary>
    /// <param name="c1">A character to find.</param>
    /// <param name="c2">A character to find.</param>
    /// <param name="c3">A character to find.</param>
    /// <returns><c>true</c> if a matching character was found; otherwise <c>false</c>.</returns>
    public bool SkipUntil(char c1, char c2, char c3) => SkipUntil(x => x == c1 || x == c2 || x == c3);

    /// <summary>
    /// Advances the reader until a character is found that meets a condition.
    /// </summary>
    /// <param name="predicate">A test to perform on characters until a condition is met.</param>
    /// <returns><c>true</c> if a character was found that met the condition; otherwise <c>false</c>.</returns>
    public bool SkipUntil(Func<char, bool> predicate)
    {
        while (Position < _line.End)
        {
            var c = Source[Position];
            if (predicate(c))
            {
                return true;
            }

            Position++;
        }

        return false;
    }

    // <summary>
    /// Advances the reader until a character is found that does not meet a condition.
    /// </summary>
    /// <param name="predicate">A test to perform on characters while a condition is met.</param>
    /// <returns><c>true</c> if a character was found that does not meet the condition; otherwise <c>false</c>.</returns>
    public bool SkipWhile(Func<char, bool> predicate)
    {
        Position++;
        while (Position < _line.End)
        {
            var c = Source[Position];
            if (!predicate(c))
            {
                return true;
            }

            Position++;
        }

        return false;
    }

    /// <summary>
    /// Creates a new reader in the same state as the current reader which can be advanced independently of the current reader.
    /// </summary>
    /// <returns>A reader in the same state as the current reader.</returns>
    public LineReader Fork()
    {
        return new(_line)
        {
            Position = Position
        };
    }

    public override string ToString() => $"Current={Current}";
}
