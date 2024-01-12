using Gherkin;
using Gherkin.Ast;

namespace Reqnroll.Parser
{
    public class SemanticParserException : ParserException
    {
        public SemanticParserException(string message) : base(message)
        {
        }

        public SemanticParserException(string message, Location location) : base(message, location)
        {
        }
    }
}