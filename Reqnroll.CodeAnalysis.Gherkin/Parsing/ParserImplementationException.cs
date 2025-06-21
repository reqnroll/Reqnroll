using Gherkin;
using System.Runtime.ExceptionServices;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

/// <summary>
/// An exception used to indicate a problem with the parser implementation.
/// </summary>
/// <param name="exception">The exception caused this exception to be thrown.</param>
internal class ParserImplementationException(ExceptionDispatchInfo exception) : 
    ParserException(exception.SourceException.Message)
{
    /// <summary>
    /// Gets the exception that caused this exception to be thrown.
    /// </summary>
    public ExceptionDispatchInfo Exception { get; } = exception;
}
