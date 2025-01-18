using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class Dependency
    {
        [JsonPropertyName("type")]
        public string ImplementationType { get; set; }
        [JsonPropertyName("as")]
        public string InterfaceType { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}