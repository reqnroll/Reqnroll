using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;
using System.Linq;

namespace Reqnroll.SystemTests.Plugins;

[TestClass]
public class XRetryPluginTest : SystemTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.xUnit;
        _projectsDriver.AddNuGetPackage("xRetry.Reqnroll", "1.0.0");
    }

    [TestMethod]
    public void XRetry_should_work_with_Reqnroll()
    {
        AddScenario(
            """
            @retry
            Scenario: Scenario with Retry
                When fail for first 2 times A
            """);
        AddScenario(
            """
            @retry
            Scenario Outline: Scenario outline with Retry
                When fail for first 2 times <label>
            Examples:
                | label |
                | B     |
                | C     |
            """);
        AddBindingClass(
            """
            using System.Collections.Generic;
            namespace XRetryPluginTest.StepDefinitions
            {
                [Binding]
                public class XRetryPluginTestStepDefinitions
                {
                    private static readonly Dictionary<string, int> RetriesByLabel = new Dictionary<string, int>();
                
                    [When("fail for first {int} times {word}")]
                    public void WhenFailForFirstTwoTimes(int retryCount, string label)
                    {
                        if (!RetriesByLabel.TryGetValue(label, out var retries))
                        {
                            retries = 0;
                        }
                        var failTest = retries < retryCount;
                        RetriesByLabel[label] = ++retries;
                        if (failTest)
                        {
                            Log.LogCustom("simulated-error", label);
                            throw new Exception($"simulated error for {label}");
                        }
                    }
                }
            }
            """);

        ExecuteTests();

        ShouldAllScenariosPass();

        var simulatedErrors = _bindingDriver.GetActualLogLines("simulated-error").ToList();
        simulatedErrors.Should().HaveCount(_preparedTests * 2); // two simulated error per test
    }

    [TestMethod]
    public void XRetry_should_work_with_Reqnroll_on_DotNetFramework_generation()
    {
        // compiling with MsBuild forces the generation to run with .NET Framework
        _compilationDriver.SetBuildTool(BuildTool.MSBuild);
        XRetry_should_work_with_Reqnroll();
    }
}
