using Gherkin;
using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace Reqnroll.Parser
{
    public class ReqnrollGherkinParserFactory : IGherkinParserFactory
    {
        readonly ConcurrentDictionary<string, ReqnrollGherkinDialectProvider> _CachedDialectProvider = new ConcurrentDictionary<string, ReqnrollGherkinDialectProvider>();
        static readonly Func<string, ReqnrollGherkinDialectProvider> _DialectProviderFactory = (defaultLanguage) => new ReqnrollGherkinDialectProvider(defaultLanguage);

        public IGherkinParser Create(IGherkinDialectProvider dialectProvider) => new ReqnrollGherkinParser(dialectProvider);

        public IGherkinParser Create(CultureInfo cultureInfo) => new ReqnrollGherkinParser(_CachedDialectProvider.GetOrAdd(cultureInfo.Name, _DialectProviderFactory));
    }
}