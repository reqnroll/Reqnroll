using System;
using System.Diagnostics;
using System.Globalization;
using Reqnroll.CucumberMessages.RuntimeSupport;
using Reqnroll.Tracing;

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


        // This holds the cucumber messages at the feature level created by the test class generator; populated when the FeatureStartedEvent is fired
        public FeatureLevelCucumberMessages FeatureCucumberMessages { get; set; }
        // This holds the unique identifier for the tracker instance that is being used to generate cucumber messages for this Test Case
        public string CucumberMessages_PickleId { get; set; }

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
    }
}