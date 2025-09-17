using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;
using System;
using System.Runtime.InteropServices;
using Reqnroll.TestProjectGenerator.Data;
using System.Collections.Generic;

namespace Reqnroll.SystemTests.Portability;

/// <summary>
/// Supported .NET versions: https://docs.reqnroll.net/latest/installation/compatibility.html#net-versions
/// </summary>
[TestCategory("Portability")]
public abstract class PortabilityTestBase : SystemTestBase
{
    protected bool SkipTUnit { get; set; }

    public static IEnumerable<object[]> GetAllUnitTestProviders()
    {
        return [
            [UnitTestProvider.MSTest],
            [UnitTestProvider.NUnit3],
            [UnitTestProvider.xUnit],
            [UnitTestProvider.xUnit3],
            [UnitTestProvider.TUnit],
        ];
    }

    private void RunSkippableTest(Action test)
    {
        // Mono is not officially supported by xUnit v2 that we use to test. 
        // See https://xunit.net/docs/v3-alpha#v2-changes
        // Related: https://github.com/reqnroll/Reqnroll/issues/132
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            && _testRunConfiguration is { UnitTestProvider: UnitTestProvider.xUnit, TargetFramework: TargetFramework.Net462 or TargetFramework.Net472 })
            Assert.Inconclusive("Disabled because xUnit v2 is not supported on Mono");

        if (_testRunConfiguration is { TargetFramework: TargetFramework.Net462, UnitTestProvider: UnitTestProvider.xUnit3 })
            Assert.Inconclusive("Disabled because xUnit3 is not supported on .NET 4.6.2");

        try
        {
            test();
        }
        catch (DotNetSdkNotInstalledException ex)
        {
            if (!new ConfigurationDriver().PipelineMode)
                Assert.Inconclusive(ex.ToString());
            else
                throw;
        }
    }

    [TestMethod]
    [DynamicData(nameof(GetAllUnitTestProviders), DynamicDataSourceType.Method)]
    public void GeneratorAllIn_sample_can_be_handled(UnitTestProvider unitTestProvider)
    {
        if (unitTestProvider == UnitTestProvider.TUnit && SkipTUnit)
        {
            Assert.IsTrue(true);
            return;
        }

        _testRunConfiguration.UnitTestProvider = unitTestProvider;
        RunSkippableTest(() =>
        {
            PrepareGeneratorAllInSamples();

            ExecuteTests();

            ShouldAllScenariosPass();
        });
    }

    [TestMethod]
    [TestCategory("MsBuild")]
    [DynamicData(nameof(GetAllUnitTestProviders), DynamicDataSourceType.Method)]
    public void GeneratorAllIn_sample_can_be_compiled_with_MsBuild(UnitTestProvider unitTestProvider)
    {
        if (unitTestProvider == UnitTestProvider.TUnit && SkipTUnit)
        {
            Assert.IsTrue(true);
            return;
        }

        _testRunConfiguration.UnitTestProvider = unitTestProvider;
        RunSkippableTest(() =>
        {
            PrepareGeneratorAllInSamples();
            _compilationDriver.SetBuildTool(BuildTool.MSBuild);
            _compilationDriver.CompileSolution();
        });
    }

    [TestMethod]
    [TestCategory("DotnetMSBuild")]
    [DynamicData(nameof(GetAllUnitTestProviders), DynamicDataSourceType.Method)]
    public void GeneratorAllIn_sample_can_be_compiled_with_DotnetMSBuild(UnitTestProvider unitTestProvider)
    {
        if (unitTestProvider == UnitTestProvider.TUnit && SkipTUnit)
        {
            Assert.IsTrue(true);
            return;
        }

        _testRunConfiguration.UnitTestProvider = unitTestProvider;
        RunSkippableTest(() =>
        {
            PrepareGeneratorAllInSamples();
            _compilationDriver.SetBuildTool(BuildTool.DotnetMSBuild);

            _compilationDriver.CompileSolution();
        });
    }

    #region Test before/after test run hooks (.NET Framework version of Reqnroll is subscribed to assembly unload)
    [TestMethod]
    [DynamicData(nameof(GetAllUnitTestProviders), DynamicDataSourceType.Method)]
    public void TestRun_hooks_are_executed(UnitTestProvider unitTestProvider)
    {
        if (unitTestProvider == UnitTestProvider.TUnit && SkipTUnit)
        {
            Assert.IsTrue(true);
            return;
        }

        _testRunConfiguration.UnitTestProvider = unitTestProvider;
        RunSkippableTest(() =>
        {
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
