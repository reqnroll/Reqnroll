using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMesssages
{
    public class FeatureLevelCucumberMessages
    {
        public FeatureLevelCucumberMessages(string source, string gkerkinDocument, string pickles)
        {
            Source = source;
            GherkinDocument = gkerkinDocument;
            Pickles = pickles;
        }

        public string Source { get; set; }
        public string GherkinDocument { get; set; }
        public string Pickles { get; set; }
    }
}
