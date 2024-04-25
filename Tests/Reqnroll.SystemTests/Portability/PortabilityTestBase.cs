﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.SystemTests.Portability;

/// <summary>
/// Supported .NET versions: https://docs.reqnroll.net/latest/installation/compatibility.html#net-versions
/// </summary>
[TestCategory("Portability")]
public abstract class PortabilityTestBase : SystemTestBase
{
    [SkippableTestMethod(typeof(DotNetSdkNotInstalledException))]
    public void GeneratorAllIn_sample_can_be_handled()
    {
        PrepareGeneratorAllInSamples();

        ExecuteTests();

        ShouldAllScenariosPass();
    }

    [SkippableTestMethod(typeof(DotNetSdkNotInstalledException))]
    [TestCategory("MsBuild")]
    public void GeneratorAllIn_sample_can_be_compiled_with_MsBuild()
    {
        PrepareGeneratorAllInSamples();
        _compilationDriver.SetBuildTool(BuildTool.MSBuild);
        _compilationDriver.CompileSolution();
    }

    [SkippableTestMethod(typeof(DotNetSdkNotInstalledException))]
    [TestCategory("DotnetMSBuild")]
    public void GeneratorAllIn_sample_can_be_compiled_with_DotnetMSBuild()
    {
        PrepareGeneratorAllInSamples();
        _compilationDriver.SetBuildTool(BuildTool.DotnetMSBuild);

        _compilationDriver.CompileSolution();
    }

    #region Test before/after test run hooks (.NET Framework version of Reqnroll is subscribed to assembly unload)
    [SkippableTestMethod(typeof(DotNetSdkNotInstalledException))]
    public void TestRun_hooks_are_executed()
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
    }
    #endregion
}
