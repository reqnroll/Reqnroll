using System.Configuration;

namespace Reqnroll.SpecFlowCompatibility.AppConfig;

public class StepAssemblyCollection : ConfigurationElementCollection
{
    protected override ConfigurationElement CreateNewElement()
    {
        return new StepAssemblyConfigElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
        return ((StepAssemblyConfigElement)element).Assembly;
    }
}