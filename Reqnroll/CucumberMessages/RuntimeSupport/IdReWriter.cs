using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.PayloadProcessing.Gherkin;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.RuntimeSupport
{
    /// <summary>
    /// This class rewrites the GherkinDocument and Pickles collection.
    /// 
    /// When the Gherkin Doc and Pickles are first created during the compilation phase, we use incrementing integer IDs. 
    /// 
    /// Rewriting of IDs is necessary when Messages have already been generated (at test-execution-time) to avoid ID collisions/re-use.
    /// </summary>
    internal class IdReWriter
    {
        internal void ReWriteIds(GherkinDocument gherkinDocument, IEnumerable<Pickle> pickles, IIdGenerator idGenerator, out GherkinDocument newGherkinDocument, out IEnumerable<Pickle> newPickles)
        {
            var gherkinDocumentIDStyleReWriter = new GherkinDocumentIDReWriter(idGenerator);
            newGherkinDocument = gherkinDocumentIDStyleReWriter.ReWriteIds(gherkinDocument);
            var idMap = gherkinDocumentIDStyleReWriter._IdMap;

            var pickleIDStyleReWriter = new PickleIDReWriter(idGenerator);
            newPickles = pickleIDStyleReWriter.ReWriteIds(pickles, idMap);
        }
    }
}
