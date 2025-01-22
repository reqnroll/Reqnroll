using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;
using System.Linq;

namespace Reqnroll.SystemTests.Plugins;

[TestClass]
public class NUnitRetryPluginTest : SystemTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.NUnit4;
        _projectsDriver.AddNuGetPackage("SpecFlow.Internal.Json", "1.0.8"); // NUnitRetry.ReqnrollPlugin 1.0.0 doesn't correctly specify its dependencies in the NuGet package, so we add it here manually.
        _projectsDriver.AddNuGetPackage("NUnitRetry.ReqnrollPlugin", "1.0.0");
    }

    [TestMethod]
    public void NUnitRetry_should_work_with_Reqnroll()
    {
        AddFeatureFileFromResource("NUnitRetryPlugin/NUnitRetryPluginTestFeature.feature");
        AddBindingClassFromResource("NUnitRetryPlugin/NUnitRetryPluginTestStepDefinitions.cs");

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
