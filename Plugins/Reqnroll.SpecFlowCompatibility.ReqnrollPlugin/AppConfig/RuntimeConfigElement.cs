using System.Configuration;
using Reqnroll.Configuration;

namespace Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.AppConfig
{
    public class RuntimeConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("dependencies", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        [ConfigurationCollection(typeof(ContainerRegistrationElementCollection), AddItemName = "register")]
        public ContainerRegistrationElementCollection Dependencies
        {
            get { return (ContainerRegistrationElementCollection)this["dependencies"]; }
            set { this["dependencies"] = value; }
        }

        [ConfigurationProperty("stopAtFirstError", DefaultValue = ConfigDefaults.StopAtFirstError, IsRequired = false)]
        public bool StopAtFirstError
        {
            get { return (bool)this["stopAtFirstError"]; }
            set { this["stopAtFirstError"] = value; }
        }

        [ConfigurationProperty("missingOrPendingStepsOutcome", DefaultValue = ConfigDefaults.MissingOrPendingStepsOutcome, IsRequired = false)]
        public MissingOrPendingStepsOutcome MissingOrPendingStepsOutcome
        {
            get { return (MissingOrPendingStepsOutcome)this["missingOrPendingStepsOutcome"]; }
            set { this["missingOrPendingStepsOutcome"] = value; }
        }

        [ConfigurationProperty("obsoleteBehavior", DefaultValue = ConfigDefaults.ObsoleteBehavior, IsRequired = false)]
        public ObsoleteBehavior ObsoleteBehavior
        {
            get { return (ObsoleteBehavior)this["obsoleteBehavior"]; }
            set { this["obsoleteBehavior"] = value; }
        }
    }
}