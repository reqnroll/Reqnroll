using Reqnroll.Specs.Drivers;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;
using ConfigurationDriver = Reqnroll.TestProjectGenerator.Driver.ConfigurationDriver;

namespace Reqnroll.Specs.StepDefinitions
{
    [Binding]
    public class ReqnrollConfigurationSteps
    {
        private readonly ConfigurationDriver _configurationDriver;
        private readonly JsonConfigurationParserDriver _jsonConfigurationParserDriver;
        private readonly ConfigurationLoaderDriver _configurationLoaderDriver;
        private readonly TestSuiteSetupDriver _testSuiteSetupDriver;

        public ReqnrollConfigurationSteps(
            ConfigurationDriver configurationDriver,
            JsonConfigurationParserDriver jsonConfigurationParserDriver,
            ConfigurationLoaderDriver configurationLoaderDriver,
            TestSuiteSetupDriver testSuiteSetupDriver)
        {
            _configurationDriver = configurationDriver;
            _jsonConfigurationParserDriver = jsonConfigurationParserDriver;
            _configurationLoaderDriver = configurationLoaderDriver;
            _testSuiteSetupDriver = testSuiteSetupDriver;
        }

        [Given(@"the project has no reqnroll\.json configuration")]
        public void GivenTheProjectHasNoReqnroll_JsonConfiguration()
        {
            _configurationDriver.SetConfigurationFormat(ConfigurationFormat.None);
        }

        [Given(@"there is a project with this reqnroll\.json configuration")]
        public void GivenThereIsAProjectWithThisReqnrollJsonConfiguration(string reqnrollJson)
        {
            _testSuiteSetupDriver.AddReqnrollJsonFromString(reqnrollJson);
        }

        [Given(@"the reqnroll configuration is")]
        public void GivenTheReqnrollConfigurationIs(string reqnrollJsonConfig)
        {
            var reqnrollConfiguration = _jsonConfigurationParserDriver.ParseReqnrollSection(reqnrollJsonConfig);
            _configurationLoaderDriver.SetFromReqnrollConfiguration(reqnrollConfiguration);
        }

        [Given(@"the project is configured to use the (.+) provider")]
        public void GivenTheProjectIsConfiguredToUseTheUnitTestProvider(string providerName)
        {
            _configurationDriver.SetUnitTestProvider(providerName);
        }

        [Given(@"Reqnroll is configured in the reqnroll\.json")]
        public void GivenReqnrollIsConfiguredInTheReqnrollJson()
        {
            _configurationDriver.SetConfigurationFormat(ConfigurationFormat.Json);
        }
        
        [Given(@"obsoleteBehavior configuration value is set to (.*)")]
        public void GivenObsoleteBehaviorConfigurationValueIsSetTo(string obsoleteBehaviorValue)
        {
            _configurationDriver.SetRuntimeObsoleteBehavior(obsoleteBehaviorValue);
        }

        [Given(@"row testing is (.+)")]
        public void GivenRowTestingIsRowTest(bool enabled)
        {
            _configurationDriver.SetIsRowTestsAllowed(enabled);
        }

        [Given(@"the type '(.*)' is registered as '(.*)' in Reqnroll runtime configuration")]
        public void GivenTheTypeIsRegisteredAsInReqnrollRuntimeConfiguration(string typeName, string interfaceName)
        {
            _configurationDriver.AddRuntimeRegisterDependency(typeName, interfaceName);
        }
        
        [Given(@"there is a scenario")]
        public void GivenThereIsAScenario()
        {
            _testSuiteSetupDriver.AddScenarios(1);
        }
    }
}
