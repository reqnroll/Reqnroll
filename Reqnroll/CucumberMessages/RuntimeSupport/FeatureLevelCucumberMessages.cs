using Gherkin.CucumberMessages.Types;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.PayloadProcessing.Gherkin;
using System.Collections.Generic;
using System.Text.Json;

namespace Reqnroll.CucumberMessages.RuntimeSupport
{
    /// <summary>
    /// This class is used at Code Generation time to provide serialized representations of the Source, GherkinDocument, and Pickles 
    /// to be used at runtime.
    /// </summary>
    public class FeatureLevelCucumberMessages
    {
        public FeatureLevelCucumberMessages(string serializedSourceMessage, string serializedGherkinDocument, string serializedPickles, string location)
        {
            Source = JsonSerializer.Deserialize<Gherkin.CucumberMessages.Types.Source>(serializedSourceMessage);
            var gherkinDocument = System.Text.Json.JsonSerializer.Deserialize<Gherkin.CucumberMessages.Types.GherkinDocument>(serializedGherkinDocument);
            var pickles = JsonSerializer.Deserialize<IEnumerable<Gherkin.CucumberMessages.Types.Pickle>>(serializedPickles);
            ReWriteIds(gherkinDocument, pickles, out var newGherkinDocument, out var newPickles);

            GherkinDocument = newGherkinDocument;
            Pickles = newPickles;
            Location = location;
            PickleJar = new PickleJar(Pickles);
        }

        public string Location { get; }
        public Gherkin.CucumberMessages.Types.Source Source { get; }
        public Gherkin.CucumberMessages.Types.GherkinDocument GherkinDocument { get; }
        public IEnumerable<Gherkin.CucumberMessages.Types.Pickle> Pickles { get; }
        public PickleJar PickleJar { get; }

        private void ReWriteIds(GherkinDocument gherkinDocument, IEnumerable<Pickle> pickles, out GherkinDocument newGherkinDocument, out IEnumerable<Pickle> newPickles)
        {
            var targetIdStyle = CucumberConfiguration.Current.IDGenerationStyle;
            var gherkinDocumentIDStyleReWriter = new GherkinDocumentIDStyleReWriter();
            newGherkinDocument = gherkinDocumentIDStyleReWriter.ReWriteIds(gherkinDocument, targetIdStyle);
            var idMap = gherkinDocumentIDStyleReWriter.IdMap;

            var pickleIDStyleReWriter = new PickleIDStyleReWriter();
            newPickles = pickleIDStyleReWriter.ReWriteIds(pickles, idMap, targetIdStyle);
        }

    }
}
