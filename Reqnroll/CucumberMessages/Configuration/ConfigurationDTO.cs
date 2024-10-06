using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Reqnroll.CucumberMessages.Configuration
{
    /// <summary>
    /// These classes holds configuration information from a configuration source.
    /// Each configuration source may provide one or more Profiles (such as Dev or Prod). 
    /// The default profile is always named 'DEFAULT'.
    /// These are JSON serialized and correspond to the json schema in CucumberMessages-config-schema.json
    /// </summary>
    public class ConfigurationDTO
    {

        public bool FileOutputEnabled { get; set; }
        public string ActiveProfileName { get; set; }
        public List<Profile> Profiles { get; set; }

        public Profile ActiveProfile => Profiles.Where(p => p.ProfileName == ActiveProfileName).FirstOrDefault();

        public ConfigurationDTO() : this(true) { }
        public ConfigurationDTO(bool enabled) : this(enabled, "DEFAULT", new List<Profile>()) { }
        public ConfigurationDTO(bool enabled, string activeProfile, List<Profile> profiles)
        {
            FileOutputEnabled = enabled;
            ActiveProfileName = activeProfile;
            Profiles = profiles;
        }

    }

    public class Profile
    {
        public string ProfileName { get; set; }
        public string BasePath { get; set; }
        public string OutputDirectory { get; set; }
        public string OutputFileName { get; set; }
        public IDGenerationStyle IDGenerationStyle { get; set; }

        public Profile(string profileName, string basePath, string outputDirectory, string outputFileName, IDGenerationStyle idGenerationStyle )
        {
            ProfileName = string.IsNullOrEmpty(profileName) ? "DEFAULT" : profileName;
            BasePath = basePath ?? "";
            OutputDirectory = outputDirectory ?? "";
            OutputFileName = outputFileName ?? "";
            IDGenerationStyle = idGenerationStyle;
        }

    }
}

