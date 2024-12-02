using System;
using System.Xml;
using Reqnroll.Configuration;

namespace Reqnroll.Generator.Interfaces
{
    public class ReqnrollConfigurationHolder : IReqnrollConfigurationHolder
    {
        private readonly string xmlString;

        public ConfigSource ConfigSource { get; }

        public string Content => xmlString;

        public bool HasConfiguration => !string.IsNullOrEmpty(xmlString);

        public ReqnrollConfigurationHolder()
        {
            ConfigSource = ConfigSource.Default;
            xmlString = null;
        }

        public ReqnrollConfigurationHolder(ConfigSource configSource, string content)
        {
            ConfigSource = configSource;
            xmlString = content;
        }

        public ReqnrollConfigurationHolder(XmlNode configXmlNode)
        {
            xmlString = configXmlNode?.OuterXml;
            ConfigSource = ConfigSource.AppConfig;
        }
    }

}