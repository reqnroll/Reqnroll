using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.PayloadProcessing.Gherkin;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.RuntimeSupport
{
    public class IdReWriter
    {
        public void ReWriteIds(GherkinDocument gherkinDocument, IEnumerable<Pickle> pickles, IIdGenerator idGenerator, out GherkinDocument newGherkinDocument, out IEnumerable<Pickle> newPickles)
        {
            var targetIdStyle = CucumberConfiguration.Current.IDGenerationStyle;
            var gherkinDocumentIDStyleReWriter = new GherkinDocumentIDReWriter(idGenerator);
            newGherkinDocument = gherkinDocumentIDStyleReWriter.ReWriteIds(gherkinDocument, targetIdStyle);
            var idMap = gherkinDocumentIDStyleReWriter.IdMap;

            var pickleIDStyleReWriter = new PickleIDReWriter(idGenerator);
            newPickles = pickleIDStyleReWriter.ReWriteIds(pickles, idMap, targetIdStyle);
        }
    }
}
