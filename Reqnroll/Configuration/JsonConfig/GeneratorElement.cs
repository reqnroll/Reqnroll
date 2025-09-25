using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class GeneratorElement
    {
        [JsonPropertyName("allowDebugGeneratedFiles")]
        public bool AllowDebugGeneratedFiles { get; set; } = ConfigDefaults.AllowDebugGeneratedFiles;

        [JsonPropertyName("allowRowTests")]
        public bool AllowRowTests { get; set; } = ConfigDefaults.AllowRowTests;

        [JsonPropertyName("addNonParallelizableMarkerForTags")]
        public List<string> AddNonParallelizableMarkerForTags { get; set; }

        [JsonPropertyName("disableFriendlyTestNames")]
        public bool DisableFriendlyTestNames { get; set; } = ConfigDefaults.DisableFriendlyTestNames;
    }
}