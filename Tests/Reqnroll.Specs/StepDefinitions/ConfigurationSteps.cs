using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.Specs.StepDefinitions
{
    [Binding]
    public class ConfigurationSteps
    {
        private readonly ConfigurationFileDriver _configurationFileDriver;
        private readonly CompilationResultDriver _compilationResultDriver;

        public ConfigurationSteps(ConfigurationFileDriver configurationFileDriver, CompilationResultDriver compilationResultDriver)
        {
            _configurationFileDriver = configurationFileDriver;
            _compilationResultDriver = compilationResultDriver;
        }

        [Then(@"the app\.config is used for configuration")]
        public void ThenTheApp_ConfigIsUsedForConfiguration()
        {
            _compilationResultDriver.CheckSolutionShouldUseAppConfig();
        }

        [Then(@"the reqnroll\.json is used for configuration")]
        public void ThenTheReqnroll_JsonIsUsedForConfiguration()
        {
            _compilationResultDriver.CheckSolutionShouldUseReqnrollJson();
        }

        [Given(@"the feature language is '(.*)'")]
        public void GivenTheFeatureLanguageIs(string featureLanguage)
        {
            _configurationFileDriver.SetFeatureLanguage(featureLanguage);
        }
    }
}
