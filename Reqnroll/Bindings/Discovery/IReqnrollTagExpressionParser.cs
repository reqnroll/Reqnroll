using Cucumber.TagExpressions;

namespace Reqnroll.Bindings.Discovery;

/// <summary>
/// Defines a parser for tag expressions.
/// </summary>
public interface IReqnrollTagExpressionParser
{
    /// <summary>
    /// Parses the specified text into an <see cref="ITagExpression"/>.
    /// </summary>
    /// <param name="text">The tag expression string to parse.</param>
    /// <returns>An <see cref="ITagExpression"/> representing the parsed expression.</returns>
    ITagExpression Parse(string text);
}