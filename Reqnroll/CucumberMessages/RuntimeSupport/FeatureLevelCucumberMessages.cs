using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.PayloadProcessing.Gherkin;
using System;
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
        public FeatureLevelCucumberMessages(Func<Source> source, Func<GherkinDocument> gherkinDocument, Func<IEnumerable<Pickle>> pickles, string location)
        {
            if (CucumberConfiguration.Current.Enabled)
            {
                Source = source;
                GherkinDocument = gherkinDocument;
                Pickles = pickles;
                Location = location;
            }
        }

        public string Location { get; }
        public Func<Io.Cucumber.Messages.Types.Source> Source { get; }
        public Func<Io.Cucumber.Messages.Types.GherkinDocument> GherkinDocument { get; }
        public Func<IEnumerable<Io.Cucumber.Messages.Types.Pickle>> Pickles { get; }
    }
}
