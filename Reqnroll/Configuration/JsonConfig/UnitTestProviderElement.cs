using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class UnitTestProviderElement
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "NUnit";
    }
}