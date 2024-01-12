using Gherkin;
using System.IO;

namespace Reqnroll.Parser
{
    public interface IGherkinParser
    {
        ReqnrollDocument Parse(TextReader featureFileReader, ReqnrollDocumentLocation documentLocation);

        IGherkinDialectProvider DialectProvider { get; }
    }
}