using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Reqnroll.Configuration;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Interfaces;

namespace Reqnroll.Generator.Project
{
    public class ProjectReader : IReqnrollProjectReader
    {
        private readonly IGeneratorConfigurationProvider _configurationLoader;
        private readonly ProjectLanguageReader _languageReader;

        public ProjectReader(IGeneratorConfigurationProvider configurationLoader, ProjectLanguageReader languageReader)
        {
            _configurationLoader = configurationLoader;
            _languageReader = languageReader;
        }

        public ReqnrollProject ReadReqnrollProject(string projectFilePath, string rootNamespace)
        {
            try
            {
                var projectFolder = Path.GetDirectoryName(projectFilePath);

                var reqnrollProject = new ReqnrollProject();
                reqnrollProject.ProjectSettings.ProjectFolder = projectFolder;
                reqnrollProject.ProjectSettings.ProjectName = Path.GetFileNameWithoutExtension(projectFilePath);
                reqnrollProject.ProjectSettings.DefaultNamespace = rootNamespace;
                reqnrollProject.ProjectSettings.ProjectPlatformSettings.Language = _languageReader.GetLanguage(projectFilePath);

      
                reqnrollProject.ProjectSettings.ConfigurationHolder = GetReqnrollConfigurationHolder(projectFolder);

                if (reqnrollProject.ProjectSettings.ConfigurationHolder != null)
                {
                    reqnrollProject.Configuration = _configurationLoader.LoadConfiguration(reqnrollProject.ProjectSettings.ConfigurationHolder);
                }

                return reqnrollProject;
            }
            catch (Exception e)
            {
                throw new Exception("Error when reading project file.", e);
            }
        }

        private ReqnrollConfigurationHolder GetReqnrollConfigurationHolder(string projectFolder)
        {
            string jsonConfigPath = Path.Combine(projectFolder, "reqnroll.json");
            if (File.Exists(jsonConfigPath))
            {
                var configFileContent = File.ReadAllText(jsonConfigPath);
                return new ReqnrollConfigurationHolder(ConfigSource.Json, configFileContent);
            }

            string appConfigPath = Path.Combine(projectFolder, "app.config");
            if (File.Exists(appConfigPath))
            {
                var configFilePath = Path.Combine(projectFolder, appConfigPath);
                var configFileContent = File.ReadAllText(configFilePath);
                return GetConfigurationHolderFromFileContent(configFileContent);
            }

            return new ReqnrollConfigurationHolder();
        }

        private static ReqnrollConfigurationHolder GetConfigurationHolderFromFileContent(string configFileContent)
        {
            try
            {
                var configDocument = new XmlDocument();
                configDocument.LoadXml(configFileContent);

                return new ReqnrollConfigurationHolder(configDocument.SelectSingleNode("/configuration/reqnroll"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "Config load error");
                return new ReqnrollConfigurationHolder();
            }
        }
    }
}