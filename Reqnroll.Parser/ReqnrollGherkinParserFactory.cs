using Gherkin;
using System.Globalization;

namespace Reqnroll.Parser
{
    public class ReqnrollGherkinParserFactory : IGherkinParserFactory
    {
        public IGherkinParser Create(IGherkinDialectProvider dialectProvider) => new ReqnrollGherkinParser(dialectProvider);

        public IGherkinParser Create(CultureInfo cultureInfo) => new ReqnrollGherkinParser(cultureInfo);
    }
}