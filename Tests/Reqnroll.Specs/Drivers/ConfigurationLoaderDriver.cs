using Reqnroll.Configuration;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.Specs.Drivers
{
    public class ConfigurationLoaderDriver
    {
        private readonly ConfigurationFileDriver _configurationFileDriver;
        private readonly SolutionDriver _solutionDriver;

        public ConfigurationLoaderDriver(ConfigurationFileDriver configurationFileDriver, SolutionDriver solutionDriver)
        {
            _configurationFileDriver = configurationFileDriver;
            _solutionDriver = solutionDriver;
        }

        public void SetFromReqnrollConfiguration(ReqnrollConfiguration reqnrollConfiguration)
        {
            var project = _solutionDriver.DefaultProject;

            foreach (string stepAssemblyName in reqnrollConfiguration.AdditionalStepAssemblies)
            {
                _configurationFileDriver.AddStepAssembly(project, new BindingAssembly(stepAssemblyName));
            }

            _configurationFileDriver.SetBindingCulture(project, reqnrollConfiguration.BindingCulture);
            _configurationFileDriver.SetFeatureLanguage(project, reqnrollConfiguration.FeatureLanguage);
        }
    }
}
