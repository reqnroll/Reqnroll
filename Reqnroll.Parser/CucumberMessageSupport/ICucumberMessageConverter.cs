using System;
using System.Collections.Generic;
using System.Text;
using Gherkin.CucumberMessages.Types;

namespace Reqnroll.Parser.CucmberMessageSupport
{
    public interface ICucumberMessagesConverters
    {
        public GherkinDocument ConvertToCucumberMessagesGherkinDocument(ReqnrollDocument gherkinDocument);
        public Source ConvertToCucumberMessagesSource(ReqnrollDocument gherkinDocument);
        public IEnumerable<Pickle> ConvertToCucumberMessagesPickles(GherkinDocument gherkinDocument);
    }
}
