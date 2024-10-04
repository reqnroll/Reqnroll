using System.Collections.Generic;
using System.Text.Json;

namespace Reqnroll.CucumberMessages.RuntimeSupport
{
    public class FeatureLevelCucumberMessages
    {
        public FeatureLevelCucumberMessages(string serializedSourceMessage, string serializedgherkinDocument, string serializedPickles, string location)
        {
            GherkinDocument = System.Text.Json.JsonSerializer.Deserialize<Gherkin.CucumberMessages.Types.GherkinDocument>(serializedgherkinDocument);
            //TODO: make the type of IDGenerator configurable
            Source = JsonSerializer.Deserialize<Gherkin.CucumberMessages.Types.Source>(serializedSourceMessage);
            Pickles = JsonSerializer.Deserialize<IEnumerable<Gherkin.CucumberMessages.Types.Pickle>>(serializedPickles);
            Location = location;
            PickleJar = new PickleJar(Pickles);
        }

        public string Location { get; }
        public Gherkin.CucumberMessages.Types.Source Source { get; }
        public Gherkin.CucumberMessages.Types.GherkinDocument GherkinDocument { get; }
        public IEnumerable<Gherkin.CucumberMessages.Types.Pickle> Pickles { get; }
        public PickleJar PickleJar { get; }

    }
}
