using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class LanguageElement
    {
        [JsonPropertyName("feature")]
        public string Feature { get; set; } = ConfigDefaults.FeatureLanguage;

        [JsonPropertyName("binding")]
        public string Binding { get; set; }
    }
}