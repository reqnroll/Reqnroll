using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    // legacy config
    public class BindingCultureElement
    {
        // legacy config
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}