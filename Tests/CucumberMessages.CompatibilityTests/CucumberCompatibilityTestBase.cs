using FluentAssertions;
using Moq;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.EnvironmentAccess;
using Reqnroll.SystemTests;
using Reqnroll.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests
{
    public class CucumberCompatibilityTestBase : SystemTestBase
    {
        protected override void TestCleanup()
        {
            // TEMPORARY: this is in place so that SystemTestBase.TestCleanup does not run (which deletes the generated code)
        }

        protected void EnableCucumberMessages()
        {
            Environment.SetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBERMESSAGES_ENABLE_ENVIRONMENT_VARIABLE, "true");
            Environment.SetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_ACTIVE_OUTPUT_PROFILE_ENVIRONMENT_VARIABLE, "LOCAL");
        }

        protected void DisableCucumberMessages()
        {
            Environment.SetEnvironmentVariable("REQNROLL_CUCUMBER_MESSAGES_ENABLED", "false");
        }
        protected void AddBindingClassFromResource(string fileName, string? prefix = null, Assembly? assemblyToLoadFrom = null)
        {
            var bindingCLassFileContent = _testFileManager.GetTestFileContent(fileName, prefix, assemblyToLoadFrom);
            AddBindingClass(bindingCLassFileContent);
        }

        protected void ShouldAllScenariosPend(int? expectedNrOfTestsSpec = null)
        {
            int expectedNrOfTests = ConfirmAllTestsRan(expectedNrOfTestsSpec);
            _vsTestExecutionDriver.LastTestExecutionResult.Pending.Should().Be(expectedNrOfTests, "all tests should pend");
        }

        protected void AddBinaryFilesFromResource(string scenarioName, string prefix, Assembly assembly)
        {
            foreach (var fileName in GetTestBinaryFileNames(scenarioName, prefix, assembly))
            {
                var content = _testFileManager.GetTestFileContent(fileName, $"{prefix}.{scenarioName}", assembly);
                _projectsDriver.AddFile(fileName, content);
            }
        }

        protected IEnumerable<string> GetTestBinaryFileNames(string scenarioName, string prefix, Assembly? assembly)
        {
            var testAssembly = assembly ?? Assembly.GetExecutingAssembly();
            string prefixToRemove = $"{prefix}.{scenarioName}.";
            return testAssembly.GetManifestResourceNames()
                           .Where(rn => !rn.EndsWith(".feature") && !rn.EndsWith(".cs") && !rn.EndsWith(".feature.ndjson")  && rn.StartsWith(prefixToRemove))
                           .Select(rn => rn.Substring(prefixToRemove.Length));
        }

        protected void CucumberMessagesAddConfigurationFile(string configFileName)
        {
            var configFileContent = File.ReadAllText(configFileName);
            _projectsDriver.AddFile(configFileName, configFileContent);
        }

        protected static string ActualsResultLocationDirectory()
        {
            //var configFileLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "CucumberMessages.configuration.json");

            //var config = System.Text.Json.JsonSerializer.Deserialize<ConfigurationDTO>(File.ReadAllText(configFileLocation), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var tracerMock = new Mock<ITraceListener>();
            var env = new EnvironmentWrapper();
            CucumberConfiguration configuration = new CucumberConfiguration(tracerMock.Object, env);
            var resolvedconfiguration = configuration.ResolveConfiguration();
            var resultLocation = Path.Combine(resolvedconfiguration.BaseDirectory, resolvedconfiguration.OutputDirectory);
            return resultLocation;
        }

    }
}
