using System.Collections.Generic;

namespace Reqnroll.TestProjectGenerator.Driver
{
    public class JsonConfigurationLoaderDriver
    {
        private readonly ProjectsDriver _projectsDriver;
        private readonly ConfigurationDriver _configurationDriver;

        public JsonConfigurationLoaderDriver(ProjectsDriver projectsDriver, ConfigurationDriver configurationDriver)
        {
            _projectsDriver = projectsDriver;
            _configurationDriver = configurationDriver;
        }

        public void AddReqnrollJson(string reqnrollJson)
        {
            _configurationDriver.SetConfigurationFormat(ConfigurationFormat.None);
            _projectsDriver.AddFile("reqnroll.json", reqnrollJson);
        }
    }
}
