using System.IO;
using System.Linq;
using System.Xml;
using TechTalk.SpecFlow.TestProjectGenerator.Factories;
using TechTalk.SpecFlow.TestProjectGenerator.Helpers;

namespace TechTalk.SpecFlow.TestProjectGenerator.Data
{
    public class NuGetConfigGenerator : XmlFileGeneratorBase
    {
        private readonly ProjectFileFactory _projectFileFactory = new ProjectFileFactory();

        public ProjectFile Generate(NuGetSource[] nuGetSources = null)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = GenerateDefaultXmlWriter(ms))
                {
                    writer.WriteStartElement("configuration");

                    WriteConfig(writer);

                    WritePackageSources(nuGetSources, writer);
                    WriteAPIKeys(writer, nuGetSources);

                    writer.WriteEndElement();
                    writer.Flush();

                    return _projectFileFactory.FromStream(ms, "nuget.config", "None");
                }
            }
        }

        private void WriteConfig(XmlWriter writer)
        {
            writer.WriteStartElement("config");

            writer.WriteStartElement("add");
            writer.WriteAttributeString("key", "dependencyversion");
            writer.WriteAttributeString("value", "Highest");
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        private void WriteAPIKeys(XmlWriter writer, NuGetSource[] nuGetSources)
        {
            writer.WriteStartElement("apikeys");

            if (nuGetSources != null)
            {
                foreach (var nuGetSource in nuGetSources.Where(ng => ng.APIKey.IsNotNullOrWhiteSpace()))
                {
                    writer.WriteStartElement("add");
                    writer.WriteAttributeString("key", nuGetSource.Key);
                    writer.WriteAttributeString("value", nuGetSource.APIKey);
                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();
        }

        private void WritePackageSources(NuGetSource[] nuGetSources, XmlWriter writer)
        {
            writer.WriteStartElement("packageSources");

            WriteNuGetSources(writer, nuGetSources);

            WriteNuGetOrgSource(writer);

            writer.WriteEndElement();
        }

        private void WriteNuGetSources(XmlWriter writer, NuGetSource[] nuGetSources)
        {
            foreach (var source in nuGetSources ?? new NuGetSource[0])
            {
                WriteNuGetSource(writer, source);
            }
        }

        private void WriteNuGetSource(XmlWriter writer, NuGetSource nuGetSource)
        {
            writer.WriteStartElement("add");
            writer.WriteAttributeString("key", nuGetSource.Key);
            writer.WriteAttributeString("value", nuGetSource.Value);
            writer.WriteEndElement();
        }

        private void WriteNuGetOrgSource(XmlWriter writer)
        {
            var nuGetOrg = new NuGetSource("Nuget.org", "https://api.nuget.org/v3/index.json");

            WriteNuGetSource(writer, nuGetOrg);
        }
    }
}
