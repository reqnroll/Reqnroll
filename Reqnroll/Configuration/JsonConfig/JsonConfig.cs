using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class JsonConfig
    {
        [JsonPropertyName("language")]
        public LanguageElement Language { get; set; }

        // legacy config
        [JsonPropertyName("bindingCulture")]
        public BindingCultureElement BindingCulture { get; set; }

        [JsonPropertyName("runtime")]
        public RuntimeElement Runtime { get; set; }

        [JsonPropertyName("generator")]
        public GeneratorElement Generator { get; set; }

        [JsonPropertyName("trace")]
        public TraceElement Trace { get; set; }

        // legacy config
        [JsonPropertyName("stepAssemblies")]
        public List<StepAssemblyElement> StepAssemblies { get; set; }

        [JsonPropertyName("bindingAssemblies")]
        public List<StepAssemblyElement> BindingAssemblies { get; set; }
    }
}