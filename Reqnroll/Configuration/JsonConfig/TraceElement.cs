using Reqnroll.BindingSkeletons;
using System;
using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class TraceElement
    {
        // legacy config
        [JsonPropertyName("traceTimings")]
        public bool TraceTimings { get; set; } = ConfigDefaults.TraceTimings;

        // legacy config
        [JsonPropertyName("minTracedDuration")]
        public TimeSpan MinTracedDuration { get; set; } = ConfigDefaults.MinTracedDurationAsTimeSpan;

        [JsonPropertyName("stepDefinitionSkeletonStyle")]
        public StepDefinitionSkeletonStyle StepDefinitionSkeletonStyle { get; set; } = ConfigDefaults.StepDefinitionSkeletonStyle;

        [JsonPropertyName("ColoredOutput")]
        public bool ColoredOutput { get; set; } = ConfigDefaults.ColoredOutput;

        // legacy config
        [JsonPropertyName("traceSuccessfulSteps")]
        public bool TraceSuccessfulSteps { get; set; } = ConfigDefaults.TraceSuccessfulSteps;
    }
}