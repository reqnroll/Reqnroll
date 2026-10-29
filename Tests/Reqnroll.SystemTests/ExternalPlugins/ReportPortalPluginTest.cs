using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.SystemTests.ExternalPlugins;

[TestClass]
public class ReportPortalPluginTest : ExternalPluginsTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.MSTest;
        _projectsDriver.AddNuGetPackage("ReportPortal.Reqnroll", "1.5.0");
    }

    [TestMethod]
    public void ReportPortal_agent_business_integration_smoke_should_pass_when_compatible()
    {
        AddFeatureFileFromResource("ReportPortalPlugin/ReportPortalPluginTestFeature.feature", resourceGroup: "ExternalPlugins");
        // Enable plugin with minimal configuration; it should initialize without external calls in local environment
        AddContentFileFromResource("ReportPortalPlugin/ReportPortal.config.json", resourceGroup: "ExternalPlugins");
        AddPassingStepBinding();

        ExecuteTests();
        
        ConfirmAllTestsRan();
        ShouldFinishWithoutTestExecutionWarnings();

        // The integration should not produce constructor-mismatch errors
        _vsTestExecutionDriver.LastTestExecutionResult.Output.Should().NotContain("MissingMethodException");
        _vsTestExecutionDriver.LastTestExecutionResult.TrxOutput.Should().NotContain("MissingMethodException");

        // Some hint that the plugin has participated in the run (console/TRX usually contains 'ReportPortal')
        _vsTestExecutionDriver.CheckAnyOutputContainsText("ReportPortal");
        
        ShouldAllScenariosPass();
    }

    [TestMethod]
    [TestCategory("MsBuild")]
    public void ReportPortal_agent_business_integration_smoke_should_pass_when_compatible_on_DotNetFramework_generation()
    {
        // compiling with MsBuild forces the generation to run with .NET Framework
        _compilationDriver.SetBuildTool(BuildTool.MSBuild);
        ReportPortal_agent_business_integration_smoke_should_pass_when_compatible();
    }
}
