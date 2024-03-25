using System.Configuration;
using System.Linq;
using Reqnroll.Configuration;

namespace Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.AppConfig;
public class ContainerRegistrationElementCollection : ConfigurationElementCollection
{
    protected override ConfigurationElement CreateNewElement()
    {
        return new ContainerRegistrationConfigElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
        var registrationConfigElement = ((ContainerRegistrationConfigElement)element);
        string elementKey = registrationConfigElement.Interface;
        if (registrationConfigElement.Name != null)
            elementKey = elementKey + "/" + registrationConfigElement.Name;
        return elementKey;
    }

    public void Add(string implementationType, string interfaceType, string name = null)
    {
        BaseAdd(new ContainerRegistrationConfigElement
        {
            Implementation = implementationType,
            Interface = interfaceType,
            Name = name
        });
    }

    public DependencyConfigurationCollection ToDependencyConfigurationCollection()
    {
        var collection = new DependencyConfigurationCollection();
        foreach (var configElement in this.OfType<ContainerRegistrationConfigElement>())
        {
            collection.Add(configElement.Implementation, configElement.Interface, configElement.Name);
        }

        return collection;
    }
}

public class ContainerRegistrationConfigElement : ConfigurationElement
{
    [ConfigurationProperty("as", IsRequired = true)]
    public string Interface
    {
        get { return (string)this["as"]; }
        set { this["as"] = value; }
    }

    [ConfigurationProperty("type", IsRequired = true)]
    public string Implementation
    {
        get { return (string)this["type"]; }
        set { this["type"] = value; }
    }

    [ConfigurationProperty("name", IsRequired = false, DefaultValue = null)]
    public string Name
    {
        get { return (string)this["name"]; }
        set { this["name"] = value; }
    }
}
