using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;
using System;
using System.Runtime.InteropServices;

namespace Reqnroll.SystemTests.Portability;

/// <summary>
/// Supported .NET versions: https://docs.reqnroll.net/latest/installation/compatibility.html#net-versions
/// </summary>
[TestCategory("Portability")]
public abstract class PortabilityTestBase : SystemTestBase
{
    private void RunSkippableTest(Action test)
    {
        try
        {
            test();
        }
        catch (DotNetSdkNotInstalledException ex)
        {
            if (!new ConfigurationDriver().PipelineMode)
                Assert.Inconclusive(ex.ToString());
        }
    }

    [TestMethod]
    [DataRow(UnitTestProvider.MSTest)]
    [DataRow(UnitTestProvider.NUnit3)]
    [DataRow(UnitTestProvider.xUnit)]
    public void GeneratorAllIn_sample_can_be_handled(UnitTestProvider unitTestProvider)
    {
        RunSkippableTest(() =>
        {
            //TODO: Temporarily disabled tests until https://github.com/reqnroll/Reqnroll/issues/132 is resolved
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                unitTestProvider == UnitTestProvider.xUnit)
                Assert.Inconclusive("Temporarily disabled tests until https://github.com/reqnroll/Reqnroll/issues/132 is resolved");

            _testRunConfiguration.UnitTestProvider = unitTestProvider;

            PrepareGeneratorAllInSamples();

            ExecuteTests();

            ShouldAllScenariosPass();
        });
    }

    [TestMethod]
    [TestCategory("MsBuild")]
    [DataRow(UnitTestProvider.MSTest)]
    [DataRow(UnitTestProvider.NUnit3)]
    [DataRow(UnitTestProvider.xUnit)]
    public void GeneratorAllIn_sample_can_be_compiled_with_MsBuild(UnitTestProvider unitTestProvider)
    {
        RunSkippableTest(() =>
        {
            _testRunConfiguration.UnitTestProvider = unitTestProvider;

            PrepareGeneratorAllInSamples();
            _compilationDriver.SetBuildTool(BuildTool.MSBuild);
            _compilationDriver.CompileSolution();
        });
    }

    [TestMethod]
    [TestCategory("DotnetMSBuild")]
    [DataRow(UnitTestProvider.MSTest)]
    [DataRow(UnitTestProvider.NUnit3)]
    [DataRow(UnitTestProvider.xUnit)]
    public void GeneratorAllIn_sample_can_be_compiled_with_DotnetMSBuild(UnitTestProvider unitTestProvider)
    {
        RunSkippableTest(() =>
        {
            _testRunConfiguration.UnitTestProvider = unitTestProvider;

            PrepareGeneratorAllInSamples();
            _compilationDriver.SetBuildTool(BuildTool.DotnetMSBuild);

            _compilationDriver.CompileSolution();
        });
    }

    #region Test before/after test run hooks (.NET Framework version of Reqnroll is subscribed to assembly unload)
    [TestMethod]
    [DataRow(UnitTestProvider.MSTest)]
    [DataRow(UnitTestProvider.NUnit3)]
    [DataRow(UnitTestProvider.xUnit)]
    public void TestRun_hooks_are_executed(UnitTestProvider unitTestProvider)
    {
        RunSkippableTest(() =>
        {
            _testRunConfiguration.UnitTestProvider = unitTestProvider;

            AddSimpleScenario();
            AddPassingStepBinding();
            AddHookBinding("BeforeTestRun");
            AddHookBinding("AfterTestRun");

            ExecuteTests();

            _bindingDriver.AssertExecutedHooksEqual(
                "BeforeTestRun",
                "AfterTestRun");
            ShouldAllScenariosPass();
        });
    }
    #endregion
}
