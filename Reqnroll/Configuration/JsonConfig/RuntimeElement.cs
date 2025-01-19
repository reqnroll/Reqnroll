using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class RuntimeElement
    {
        [JsonPropertyName("stopAtFirstError")]
        public bool StopAtFirstError { get; set; } = ConfigDefaults.StopAtFirstError;

        [JsonPropertyName("missingOrPendingStepsOutcome")]
        public MissingOrPendingStepsOutcome MissingOrPendingStepsOutcome { get; set; } = ConfigDefaults.MissingOrPendingStepsOutcome;

        [JsonPropertyName("obsoleteBehavior")]
        public ObsoleteBehavior ObsoleteBehavior { get; set; } = ConfigDefaults.ObsoleteBehavior;

        [JsonPropertyName("dependencies")]
        public List<Dependency> Dependencies { get; set; }
    }
}