using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Reqnroll.TestProjectGenerator.ConfigurationModel;
using Reqnroll.TestProjectGenerator.ConfigurationModel.Dependencies;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Extensions;
using Reqnroll.TestProjectGenerator.Helpers;

namespace Reqnroll.TestProjectGenerator.Factories.ConfigurationGenerator
{
    public class AppConfigGenerator : XmlFileGeneratorBase, IConfigurationGenerator
    {
        private readonly CurrentVersionDriver _currentVersionDriver;
        private readonly ProjectFileFactory _projectFileFactory = new ProjectFileFactory();

        public AppConfigGenerator(CurrentVersionDriver currentVersionDriver)
        {
            _currentVersionDriver = currentVersionDriver;
        }

        public ProjectFile Generate(Configuration configuration)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = GenerateDefaultXmlWriter(ms))
                {
                    writer.WriteStartElement("configuration");

                    WriteConfigSections(writer, configuration.AppConfigSection);

                    WriteAppSettings(writer, configuration);

                    WriteReqnroll(writer, configuration);

                    writer.WriteEndElement();
                    writer.Flush();

                    return _projectFileFactory.FromStream(ms, "app.config", "None", Encoding.UTF8);
                }
            }
        }

        private void WriteAppSettings(XmlWriter writer, Configuration configuration)
        {
            writer.WriteStartElement("appSettings");

            foreach (var configurationAppSetting in configuration.AppSettings)
            {
                writer.WriteStartElement("add");
                writer.WriteAttributeString("key", configurationAppSetting.key);
                writer.WriteAttributeString("value", configurationAppSetting.value);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void WriteReqnroll(XmlWriter writer, Configuration configuration)
        {
            writer.WriteStartElement("reqnroll");

            if (configuration.BindingCulture != null)
            {
                WriteBindingCulture(writer, configuration.BindingCulture);
            }

            if (configuration.FeatureLanguage != null)
            {
                WriteLanguage(writer, configuration.FeatureLanguage);
            }

            if (configuration.Generator.IsValueCreated)
            {
                WriteGenerator(writer, configuration.Generator.Value);
            }

            if (configuration.Runtime.IsValueCreated)
            {
                WriteRuntime(writer, configuration.Runtime.Value);
            }

            WriteStepAssemblies(writer, configuration.BindingAssemblies);
            
            writer.WriteEndElement();
        }

        private void WriteBindingCulture(XmlWriter writer, CultureInfo bindingCulture)
        {
            writer.WriteStartElement("bindingCulture");
            writer.WriteAttributeString("name", bindingCulture.Name);
            writer.WriteEndElement();
        }

        private void WriteConfigSections(XmlWriter writer, IEnumerable<AppConfigSection> appConfigSections)
        {
            writer.WriteStartElement("configSections");

            foreach (var appConfigSection in appConfigSections)
            {
                WriteConfigSection(writer, appConfigSection);
            }

            writer.WriteEndElement();
        }

        private void WriteConfigSection(XmlWriter writer, AppConfigSection appConfigSection)
        {
            writer.WriteStartElement("section");
            writer.WriteAttributeString("name", appConfigSection.Name);
            writer.WriteAttributeString("type", appConfigSection.Type);
            writer.WriteEndElement();
        }

        private void WriteUnitTestProvider(XmlWriter writer, string unitTestProvider)
        {
            writer.WriteStartElement("unitTestProvider");
            writer.WriteAttributeString("name", unitTestProvider);
            writer.WriteEndElement();
        }

        private void WriteLanguage(XmlWriter writer, CultureInfo featureLanguage)
        {
            writer.WriteStartElement("language");
            writer.WriteAttributeString("feature", featureLanguage.Name);
            writer.WriteEndElement();
        }

        private void WriteStepAssemblies(XmlWriter writer, IEnumerable<BindingAssembly> stepAssemblies)
        {
            if (stepAssemblies is null) return;
            writer.WriteStartElement("stepAssemblies");
            foreach (var stepAssembly in stepAssemblies)
            {
                WriteStepAssembly(writer, stepAssembly);
            }

            writer.WriteEndElement();
        }

        private void WriteStepAssembly(XmlWriter writer, BindingAssembly stepAssembly)
        {
            writer.WriteStartElement("stepAssembly");
            writer.WriteAttributeString("assembly", stepAssembly.Assembly);
            writer.WriteEndElement();
        }

        private void WritePlugins(XmlWriter writer, IEnumerable<ReqnrollPlugin> plugins)
        {
            if (plugins is null) return;
            writer.WriteStartElement("plugins");
            foreach (var plugin in plugins)
            {
                WritePlugin(writer, plugin);
            }

            writer.WriteEndElement();
        }

        private void WritePlugin(XmlWriter writer, ReqnrollPlugin plugin)
        {
            writer.WriteStartElement("add");
            writer.WriteAttributeString("name", plugin.Name);

            if (!string.IsNullOrEmpty(plugin.Path))
                writer.WriteAttributeString("path", plugin.Path);

            if (plugin.Type != (ReqnrollPluginType.Generator | ReqnrollPluginType.Runtime))
                writer.WriteAttributeString("type", plugin.Type.ToPluginTypeString());

            writer.WriteEndElement();
        }

        private void WriteGenerator(XmlWriter writer, Generator generator)
        {
            writer.WriteStartElement("generator");

            writer.WriteAttributeString("allowDebugGeneratedFiles", ToXmlString(generator.AllowDebugGeneratedFiles));
            writer.WriteAttributeString("allowRowTests", ToXmlString(generator.AllowRowTests));
            writer.WriteAttributeString("generateAsyncTests", ToXmlString(generator.GenerateAsyncTests));
            writer.WriteAttributeString("path", generator.Path);

            if (generator.Dependencies.Count > 0)
            {
                writer.WriteStartElement("dependencies");
                WriteDependencies(writer, generator.Dependencies);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void WriteRuntime(XmlWriter writer, Runtime runtime)
        {
            writer.WriteStartElement("runtime");

            writer.WriteAttributeString("stopAtFirstError", ToXmlString(runtime.StopAtFirstError));
            writer.WriteAttributeString("missingOrPendingStepsOutcome", runtime.MissingOrPendingStepsOutcome.ToString());

            if (runtime.ObsoleteBehavior.IsNotNullOrWhiteSpace())
            {
                writer.WriteAttributeString("obsoleteBehavior", runtime.ObsoleteBehavior);
            }

            if (runtime.Dependencies.Count > 0)
            {
                writer.WriteStartElement("dependencies");
                WriteDependencies(writer, runtime.Dependencies);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void WriteDependencies(XmlWriter writer, IEnumerable<IDependency> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                switch (dependency)
                {
                    case RegisterDependency registerDependency:
                        WriteRegisterDependency(writer, registerDependency);
                        break;
                    case null: throw new InvalidOperationException("null is not supported as dependency.");
                    default: throw new NotSupportedException($"Dependency type {dependency.GetType()} is not supported.");
                }
            }
        }

        private void WriteRegisterDependency(XmlWriter writer, RegisterDependency dependency)
        {
            writer.WriteStartElement("register");

            writer.WriteAttributeString("type", dependency.Type);
            writer.WriteAttributeString("as", dependency.As);

            if (dependency.Name.IsNotNullOrWhiteSpace())
            {
                writer.WriteAttributeString("name", dependency.Name);
            }

            writer.WriteEndElement();
        }

        private string ToXmlString(bool value) => value ? "true" : "false";
    }
}
