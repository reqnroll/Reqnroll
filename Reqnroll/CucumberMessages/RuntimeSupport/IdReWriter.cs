using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.PayloadProcessing.Gherkin;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.RuntimeSupport
{
    /// <summary>
    /// This class rewrites the GherkinDocument and Pickles collection to match the ID style configured for this test run.
    /// 
    /// When the Gherkin Doc and Pickles are first created during the compilation phase, we use incrementing integer IDs. 
    /// If the current test run wants to use UUIDs, then we re-write the documents with UUIDs and ensure all the references
    /// to those IDs are patched up.
    /// </summary>
    internal class IdReWriter
    {
        internal void ReWriteIds(GherkinDocument gherkinDocument, IEnumerable<Pickle> pickles, IIdGenerator idGenerator, out GherkinDocument newGherkinDocument, out IEnumerable<Pickle> newPickles)
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
