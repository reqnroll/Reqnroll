using System.Collections.Immutable;

namespace Reqnroll.Analyzers;
public abstract class ParseResult<T>
{
    public static implicit operator ParseResult<T>(T value) => new SuccessParseResult<T>(value);
    public static implicit operator ParseResult<T>(ParsingError error) => new FailedParseResult<T>(ImmutableArray.Create(error));
    public static implicit operator ParseResult<T>(ImmutableArray<ParsingError> errors) => new FailedParseResult<T>(errors);
}

public sealed class SuccessParseResult<T>(T value) : ParseResult<T>
{
    public T Value { get; } = value;
}

public sealed class FailedParseResult<T>(ImmutableArray<ParsingError> errors) : ParseResult<T>
{
    public ImmutableArray<ParsingError> Errors { get; } = errors;
}

public record ParsingError(string Description, Position Position);

public record struct Position(int Start, int End); 
