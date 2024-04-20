using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.SystemTests.Portability;

/// <summary>
/// Supported .NET versions: https://docs.reqnroll.net/latest/installation/compatibility.html#net-versions
/// </summary>
[TestCategory("Portability")]
public abstract class PortabilityTestBase : SystemTestBase
{
    [TestMethod]
    public void GeneratorAllIn_sample_can_be_handled()
    {
        try
        {
            PrepareGeneratorAllInSamples();

            ExecuteTests();

            ShouldAllScenariosPass();
        }
        catch (DotNetSdkNotInstalledException exception)
        {
            Assert.Inconclusive(exception.Message);
        }
    }

    [TestMethod]
    [TestCategory("MsBuild")]
    public void GeneratorAllIn_sample_can_be_compiled_with_MsBuild()
    {
        try
        {
            PrepareGeneratorAllInSamples();
            _compilationDriver.SetBuildTool(BuildTool.MSBuild);
            _compilationDriver.CompileSolution();
        }
        catch (DotNetSdkNotInstalledException exception)
        {
            Assert.Inconclusive(exception.Message);
        }
    }

    [TestMethod]
    [TestCategory("DotnetMSBuild")]
    public void GeneratorAllIn_sample_can_be_compiled_with_DotnetMSBuild()
    {
        try
        {
            PrepareGeneratorAllInSamples();
            _compilationDriver.SetBuildTool(BuildTool.DotnetMSBuild);

            _compilationDriver.CompileSolution();
        }
        catch (DotNetSdkNotInstalledException exception)
        {
            Assert.Inconclusive(exception.Message);
        }
    }

    #region Test before/after test run hooks (.NET Framework version of Reqnroll is subscribed to assembly unload)
    [TestMethod]
    public void TestRun_hooks_are_executed()
    {
        try
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
        catch (DotNetSdkNotInstalledException exception)
        {
            Assert.Inconclusive(exception.Message);
        }
    }
    #endregion
}
