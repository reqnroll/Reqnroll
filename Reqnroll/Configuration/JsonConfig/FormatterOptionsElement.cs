using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class FormatterOptionsElement
    {
        [JsonPropertyName("outputFilePath")]
        public string OutputFilePath { get; set; }

        /// <summary>
        /// Captures any additional options not explicitly defined above.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalOptions { get; set; }
    }
}
