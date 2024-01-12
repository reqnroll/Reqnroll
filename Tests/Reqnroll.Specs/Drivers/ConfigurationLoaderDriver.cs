using Reqnroll.Configuration;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.Specs.Drivers
{
    public class ConfigurationLoaderDriver
    {
        private readonly ConfigurationDriver _configurationDriver;
        private readonly SolutionDriver _solutionDriver;

        public ConfigurationLoaderDriver(ConfigurationDriver configurationDriver, SolutionDriver solutionDriver)
        {
            _configurationDriver = configurationDriver;
            _solutionDriver = solutionDriver;
        }

        public void SetFromReqnrollConfiguration(ReqnrollConfiguration reqnrollConfiguration)
        {
            var project = _solutionDriver.DefaultProject;

            foreach (string stepAssemblyName in reqnrollConfiguration.AdditionalStepAssemblies)
            {
                _configurationDriver.AddStepAssembly(project, new StepAssembly(stepAssemblyName));
            }

            _configurationDriver.SetBindingCulture(project, reqnrollConfiguration.BindingCulture);
            _configurationDriver.SetFeatureLanguage(project, reqnrollConfiguration.FeatureLanguage);
        }
    }
}
