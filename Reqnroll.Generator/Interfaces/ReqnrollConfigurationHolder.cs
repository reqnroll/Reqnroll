using System.Xml;
using Reqnroll.Configuration;

namespace Reqnroll.Generator.Interfaces;

public class ReqnrollConfigurationHolder : IReqnrollConfigurationHolder
{
    private readonly string _xmlString;

    public ConfigSource ConfigSource { get; }

    public string Content => _xmlString;

    public bool HasConfiguration => !string.IsNullOrEmpty(_xmlString);

    public ReqnrollConfigurationHolder()
    {
        ConfigSource = ConfigSource.Default;
        _xmlString = null;
    }

    public ReqnrollConfigurationHolder(ConfigSource configSource, string content)
    {
        ConfigSource = configSource;
        _xmlString = content;
    }

    public ReqnrollConfigurationHolder(XmlNode configXmlNode)
    {
        _xmlString = configXmlNode?.OuterXml;
        ConfigSource = ConfigSource.AppConfig;
    }
}