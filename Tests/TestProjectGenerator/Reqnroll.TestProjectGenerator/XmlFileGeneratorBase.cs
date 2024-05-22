using System.IO;
using System.Text;
using System.Xml;

namespace Reqnroll.TestProjectGenerator
{
    public abstract class XmlFileGeneratorBase
    {
        protected virtual XmlWriterSettings GetXmlWriterSettings() => new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            CloseOutput = false
        };

        protected virtual XmlWriter GenerateDefaultXmlWriter(string targetFilePath)
        {
            return XmlWriter.Create(targetFilePath, GetXmlWriterSettings());
        }

        protected virtual XmlWriter GenerateDefaultXmlWriter(Stream targetStream)
        {
            return XmlWriter.Create(targetStream, GetXmlWriterSettings());
        }
    }
}
