using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;
using System.Linq;

namespace Reqnroll.SystemTests.ExternalPlugins;

[TestClass]
public class NUnitRetryPluginTest : ExternalPluginsTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.NUnit4;
        _projectsDriver.AddNuGetPackage("NUnitRetry.ReqnrollPlugin", "1.0.100");
    }

    [TestMethod]
    public void NUnitRetry_should_work_with_Reqnroll()
    {
        AddFeatureFileFromResource("NUnitRetryPlugin/NUnitRetryPluginTestFeature.feature", resourceGroup: "ExternalPlugins");
        AddBindingClassFromResource("NUnitRetryPlugin/NUnitRetryPluginTestStepDefinitions.cs", resourceGroup: "ExternalPlugins");

        ExecuteTests();

        ShouldAllScenariosPass();

        var simulatedErrors = _bindingDriver.GetActualLogLines("simulated-error").ToList();
        simulatedErrors.Should().HaveCount(_preparedTests * 2); // two simulated error per test
    }

    [TestMethod]
    [TestCategory("MsBuild")]
    public void NUnitRetry_should_work_with_Reqnroll_on_DotNetFramework_generation()
    {
        // compiling with MsBuild forces the generation to run with .NET Framework
        _compilationDriver.SetBuildTool(BuildTool.MSBuild);
        NUnitRetry_should_work_with_Reqnroll();
    }
}
