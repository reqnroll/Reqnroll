using System.Configuration;

namespace Reqnroll.SpecFlowCompatibility.AppConfig;

public partial class GeneratorConfigElement
{
    public class TagElement : ConfigurationElement
    {
        [ConfigurationProperty("value", DefaultValue = "", IsRequired = false)]
        public string Value
        {
            get { return (string)this["value"]; }
            set { this["value"] = value; }
        }
    }
}