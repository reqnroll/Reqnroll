using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class StepAssemblyElement
    {
        [JsonPropertyName("assembly")]
        public string Assembly { get; set; }
    }
}