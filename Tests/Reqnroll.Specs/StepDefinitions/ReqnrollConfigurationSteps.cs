using Reqnroll.Specs.Drivers;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.Specs.StepDefinitions
{
    [Binding]
    public class ReqnrollConfigurationSteps
    {
        private readonly ConfigurationDriver _configurationDriver;
        private readonly XmlConfigurationParserDriver _xmlConfigurationParserDriver;
        private readonly ConfigurationLoaderDriver _configurationLoaderDriver;
        private readonly TestSuiteSetupDriver _testSuiteSetupDriver;

        public ReqnrollConfigurationSteps(
            ConfigurationDriver configurationDriver,
            XmlConfigurationParserDriver xmlConfigurationParserDriver,
            ConfigurationLoaderDriver configurationLoaderDriver,
            TestSuiteSetupDriver testSuiteSetupDriver)
        {
            _configurationDriver = configurationDriver;
            _xmlConfigurationParserDriver = xmlConfigurationParserDriver;
            _configurationLoaderDriver = configurationLoaderDriver;
            _testSuiteSetupDriver = testSuiteSetupDriver;
        }

        [Given(@"the project has no reqnroll\.json configuration")]
        public void GivenTheProjectHasNoSpecflow_JsonConfiguration()
        {
            _configurationDriver.SetConfigurationFormat(ConfigurationFormat.None);
        }

        [Given(@"the project has no app\.config configuration")]
        public void GivenTheProjectHasNoApp_ConfigConfiguration()
        {
            _configurationDriver.SetConfigurationFormat(ConfigurationFormat.None);
        }

        [Given(@"there is a project with this reqnroll\.json configuration")]
        public void GivenThereIsAProjectWithThisReqnrollJsonConfiguration(string reqnrollJson)
        {
            _testSuiteSetupDriver.AddReqnrollJsonFromString(reqnrollJson);
        }

        [Given(@"there is a project with this app\.config configuration")]
        public void GivenThereIsAProjectWithThisApp_ConfigConfiguration(string multilineText)
        {
            _testSuiteSetupDriver.AddAppConfigFromString(multilineText);
        }


        [Given(@"the reqnroll configuration is")]
        public void GivenTheReqnrollConfigurationIs(string reqnrollSection)
        {
            var reqnrollConfiguration = _xmlConfigurationParserDriver.ParseReqnrollSection(reqnrollSection);
            _configurationLoaderDriver.SetFromReqnrollConfiguration(reqnrollConfiguration);
        }

        [Given(@"the project is configured to use the (.+) provider")]
        public void GivenTheProjectIsConfiguredToUseTheUnitTestProvider(string providerName)
        {
            _configurationDriver.SetUnitTestProvider(providerName);
        }

        [Given(@"Reqnroll is configured in the app\.config")]
        public void GivenReqnrollIsConfiguredInTheApp_Config()
        {
            _configurationDriver.SetConfigurationFormat(ConfigurationFormat.Config);
        }

        [Given(@"Reqnroll is configured in the reqnroll\.json")]
        public void GivenReqnrollIsConfiguredInTheReqnrollJson()
        {
            _configurationDriver.SetConfigurationFormat(ConfigurationFormat.Json);
        }
        
        [Given(@"obsoleteBehavior configuration value is set to (.*)")]
        public void GivenObsoleteBehaviorConfigurationValueIsSetTo(string obsoleteBehaviorValue)
        {
//          var configText = $@"<reqnroll>
//          <runtime obsoleteBehavior=""{obsoleteBehaviorValue}"" />
//          </reqnroll >";

//          GivenTheSpecflowConfigurationIs(configText);
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
