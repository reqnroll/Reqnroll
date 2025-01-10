using Reqnroll.Plugins;
using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class PluginElement
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("parameters")]
        public string Parameters { get; set; }

        [JsonPropertyName("type")]
        public PluginType Type { get; set; }
    }
}