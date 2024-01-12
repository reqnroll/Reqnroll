using Gherkin;
using System.Globalization;

namespace Reqnroll.Parser
{
    public interface IGherkinParserFactory
    {
        IGherkinParser Create(IGherkinDialectProvider dialectProvider);
        IGherkinParser Create(CultureInfo cultureInfo);
    }
}
