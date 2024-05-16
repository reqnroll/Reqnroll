using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

//using Newtonsoft.Json;

namespace Reqnroll.Configuration.JsonConfig
{
    public class RuntimeElement
    {
        [DataMember(Name = "stopAtFirstError")]
        [DefaultValue(ConfigDefaults.StopAtFirstError)]
        public bool StopAtFirstError { get; set; }

        [DataMember(Name = "missingOrPendingStepsOutcome")]
        [DefaultValue(ConfigDefaults.MissingOrPendingStepsOutcome)]
        public MissingOrPendingStepsOutcome MissingOrPendingStepsOutcome { get; set; }

        [DataMember(Name = "obsoleteBehavior")]
        [DefaultValue(ConfigDefaults.ObsoleteBehavior)]
        public ObsoleteBehavior ObsoleteBehavior { get; set; }

        [DataMember(Name = "enableCucumberStepDefinitionBindings")]
        [DefaultValue(ConfigDefaults.EnableCucumberStepDefinitionBindings)]
        public bool EnableCucumberStepDefinitionBindings { get; set; }

        [DataMember(Name = "dependencies")]
        public List<Dependency> Dependencies { get; set; }
    }
}