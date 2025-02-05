namespace Reqnroll.TestProjectGenerator.Driver
{
    public class JsonConfigurationLoaderDriver
    {
        private readonly ProjectsDriver _projectsDriver;
        private readonly ConfigurationFileDriver _configurationFileDriver;

        public JsonConfigurationLoaderDriver(ProjectsDriver projectsDriver, ConfigurationFileDriver configurationFileDriver)
        {
            _projectsDriver = projectsDriver;
            _configurationFileDriver = configurationFileDriver;
        }

        public void AddReqnrollJson(string reqnrollJson)
        {
            _configurationFileDriver.SetConfigurationFormat(ConfigurationFormat.None);
            _projectsDriver.AddFile("reqnroll.json", reqnrollJson);
        }
    }
}
