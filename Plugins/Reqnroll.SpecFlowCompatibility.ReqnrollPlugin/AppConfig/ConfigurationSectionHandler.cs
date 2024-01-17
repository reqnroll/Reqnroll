using System.Configuration;
using System.IO;
using System.Xml;
using Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.AppConfig;

// ReSharper disable once CheckNamespace
namespace Reqnroll.SpecFlowCompatibility.ReqnrollPlugin;

public class ConfigurationSectionHandler : ConfigurationSection
{
    [ConfigurationProperty("language", IsRequired = false, DefaultValue = null)]
    public LanguageConfigElement Language
    {
        get => (LanguageConfigElement)this["language"];
        set => this["language"] = value;
    }

    [ConfigurationProperty("bindingCulture", IsRequired = false)]
    public BindingCultureConfigElement BindingCulture
    {
        get => (BindingCultureConfigElement)this["bindingCulture"];
        set => this["bindingCulture"] = value;
    }

    [ConfigurationProperty("generator", IsRequired = false)]
    public GeneratorConfigElement Generator
    {
        get => (GeneratorConfigElement)this["generator"];
        set => this["generator"] = value;
    }

    [ConfigurationProperty("runtime", IsRequired = false)]
    public RuntimeConfigElement Runtime
    {
        get => (RuntimeConfigElement)this["runtime"];
        set => this["runtime"] = value;
    }

    [ConfigurationProperty("trace", IsRequired = false)]
    public TraceConfigElement Trace
    {
        get => (TraceConfigElement)this["trace"];
        set => this["trace"] = value;
    }

    [ConfigurationProperty("stepAssemblies", IsDefaultCollection = false, IsRequired = false)]
    [ConfigurationCollection(typeof(StepAssemblyCollection), AddItemName = "stepAssembly")]
    public StepAssemblyCollection StepAssemblies
    {
        get => (StepAssemblyCollection)this["stepAssemblies"];
        set => this["stepAssemblies"] = value;
    }

    public static ConfigurationSectionHandler CreateFromXml(string xmlContent)
    {
        ConfigurationSectionHandler section = new ConfigurationSectionHandler();
        section.Init();
        section.Reset(null!);
        using (var reader = new XmlTextReader(new StringReader(xmlContent.Trim())))
        {
            section.DeserializeSection(reader);
        }
        section.ResetModified();
        return section;
    }

    public static ConfigurationSectionHandler CreateFromXml(XmlNode xmlContent)
    {
        ConfigurationSectionHandler section = new ConfigurationSectionHandler();
        section.Init();
        section.Reset(null!);
        using (var reader = new XmlNodeReader(xmlContent))
        {
            section.DeserializeSection(reader);
        }
        section.ResetModified();
        return section;
    }
}