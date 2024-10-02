using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.RuntimeSupport
{
    public class FeatureLevelCucumberMessages
    {
        public FeatureLevelCucumberMessages(string source, string gkerkinDocument, string pickles)
        {
            Source = source;
            GherkinDocument = gkerkinDocument;
            Pickles = pickles;
            PickleJar = new PickleJar(pickles);
        }

        public string Source { get; }
        public string GherkinDocument { get; }
        public string Pickles { get; }
        public PickleJar PickleJar { get; }
    }
}
