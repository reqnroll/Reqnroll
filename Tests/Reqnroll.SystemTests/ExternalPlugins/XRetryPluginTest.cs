using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;
using System.Linq;

namespace Reqnroll.SystemTests.ExternalPlugins;

[TestClass]
public class XRetryPluginTest : ExternalPluginsTestBase
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
        AddFeatureFileFromResource("XRetryPlugin/XRetryPluginTestFeature.feature", resourceGroup: "ExternalPlugins");
        AddBindingClassFromResource("XRetryPlugin/XRetryPluginTestStepDefinitions.cs", resourceGroup: "ExternalPlugins");

        ExecuteTests();

        ShouldAllScenariosPass();

        var simulatedErrors = _bindingDriver.GetActualLogLines("simulated-error").ToList();
        simulatedErrors.Should().HaveCount(_preparedTests * 2); // two simulated error per test
    }

    [TestMethod]
    [TestCategory("MsBuild")]
    public void XRetry_should_work_with_Reqnroll_on_DotNetFramework_generation()
    {
        // compiling with MsBuild forces the generation to run with .NET Framework
        _compilationDriver.SetBuildTool(BuildTool.MSBuild);
        XRetry_should_work_with_Reqnroll();
    }
}
