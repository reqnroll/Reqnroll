using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.SystemTests.Plugins;

[TestClass]
[TestCategory("Plugins")]
public class PluginPackagesTest : PluginsTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.TargetFramework = TargetFramework.Net100;
    }

    [TestMethod]
    public void Reqnroll_CustomPlugin_package_works()
    {
        _projectsDriver.AddNuGetPackage("Reqnroll.CustomPlugin", _currentVersionDriver.ReqnrollNuGetVersion);

        AddSimpleScenario();
        AddPassingStepBinding();

        ExecuteTests();

        ShouldAllScenariosPass();
    }

    [TestMethod]
    public void Reqnroll_SpecFlowCompatibility_package_works()
    {
        _projectsDriver.AddNuGetPackage("Reqnroll.SpecFlowCompatibility.ReqnrollPlugin", _currentVersionDriver.ReqnrollNuGetVersion);

        AddFeatureFile(
            """
            Feature: SpecFlow Compatibility
                Scenario: Using SpecFlow namespace
                    Given I use SpecFlow namespace
            """);

        AddBindingClass(
            """
            using TechTalk.SpecFlow;
            
            [Binding]
            public class SpecFlowCompatibilitySteps
            {
                [Given("I use SpecFlow namespace")]
                public void UseSpecFlowNamespace()
                {
                    global::Log.LogStep();
                }
            }
            """);

        ExecuteTests();

        ShouldAllScenariosPass();
    }
}