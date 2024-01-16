using System.Configuration;

namespace Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.AppConfig
{
    public class StepAssemblyConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("assembly", IsRequired = true)]
        public string Assembly
        {
            get { return (string)this["assembly"]; }
            set { this["assembly"] = value; }
        }
    }
}