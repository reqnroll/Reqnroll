using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Tracing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Reqnroll
{
    [DebuggerDisplay("{Title}")]
    public class FeatureInfo
    {
        public string[] Tags { get; private set; }
        public ProgrammingLanguage GenerationTargetLanguage { get; private set; }

        public string FolderPath { get; private set; }

        public string Title { get; private set; }
        public string Description { get; private set; }
        public CultureInfo Language { get; private set; }


        /// <summary>
        /// This property holds the cucumber messages at the feature level created by the test class generator; populated when the FeatureStartedEvent is fired. Used internally.
        /// </summary>
        internal IFeatureLevelCucumberMessages FeatureCucumberMessages { get; set; }

        public FeatureInfo(CultureInfo language, string folderPath, string title, string description, params string[] tags)
            : this(language, folderPath, title, description, ProgrammingLanguage.CSharp, tags)
        {
        }

        public FeatureInfo(CultureInfo language, string folderPath, string title, string description, ProgrammingLanguage programmingLanguage, params string[] tags)
        {
            if (language.IsNeutralCulture)
            {
                //TODO: as Gherkin parser does not provide the default specific culture for neutral languages (eg. 'sv' -> 'sv-SE'), this code will be required and has to be extended
                // for backwards compatibility (execution of files that were generated with pre 1.3)
                language = LanguageHelper.GetSpecificCultureInfo(language);
            }

            Language = language;
            FolderPath = folderPath;
            Title = title;
            Description = description;
            GenerationTargetLanguage = programmingLanguage;
            Tags = tags ?? Array.Empty<string>();
        }

        public FeatureInfo(CultureInfo language, string folderPath, string title, string description, ProgrammingLanguage programmingLanguage, string[] tags, IFeatureLevelCucumberMessages featureLevelCucumberMessages = null)
            : this(language, folderPath, title, description, programmingLanguage, tags)
        {
            FeatureCucumberMessages = featureLevelCucumberMessages;
        }

        public string GetPickleIndexFromTestRow(string featureName, string scenarioOutlineName, IEnumerable<string> tags, ICollection rowValues)
        {
            return FeatureCucumberMessages.GetPickleIndexFromTestRow(featureName, scenarioOutlineName, tags, rowValues);
        }
    }
}