using System.Collections.Generic;

namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
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

        public void AddSpecFlowJson(string specFlowJson)
        {
            _configurationDriver.SetConfigurationFormat(ConfigurationFormat.None);
            _projectsDriver.AddFile("specflow.json", specFlowJson);
        }
    }
}
