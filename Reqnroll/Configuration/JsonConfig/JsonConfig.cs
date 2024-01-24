using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class JsonConfig
    {
        [DataMember(Name = "language")]
        public LanguageElement Language { get; set; }

        // legacy config
        [DataMember(Name = "bindingCulture")]
        public BindingCultureElement BindingCulture { get; set; }

        [DataMember(Name = "runtime")]
        public RuntimeElement Runtime { get; set; }

        [DataMember(Name = "generator")]
        public GeneratorElement Generator { get; set; }

        [DataMember(Name = "trace")]
        public TraceElement Trace { get; set; }

        // legacy config
        [DataMember(Name = "stepAssemblies")]
        public List<StepAssemblyElement> StepAssemblies { get; set; }

        [DataMember(Name = "bindingAssemblies")]
        public List<StepAssemblyElement> BindingAssemblies { get; set; }
    }
}