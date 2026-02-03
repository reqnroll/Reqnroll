using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class FormattersElement
    {
        [JsonPropertyName("html")]
        public FormatterOptionsElement Html { get; set; }

        [JsonPropertyName("message")]
        public FormatterOptionsElement Message { get; set; }

        /// <summary>
        /// Captures any additional/custom formatters not explicitly defined above.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalFormatters { get; set; }
    }
}
