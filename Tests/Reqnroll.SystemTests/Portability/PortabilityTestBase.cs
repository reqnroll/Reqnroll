using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator.Driver;
using System.Runtime.InteropServices;

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
        PrepareGeneratorAllInSamples();

        ExecuteTests();

        ShouldAllScenariosPass();
    }

    [TestMethod]
    [TestCategory("MsBuild")]
    public void GeneratorAllIn_sample_can_be_compiled_with_MsBuild()
    {
        PrepareGeneratorAllInSamples();
        _compilationDriver.SetBuildTool(BuildTool.MSBuild);

        _compilationDriver.CompileSolution();
    }

    [TestMethod]
    [TestCategory("DotnetMSBuild")]
    public void GeneratorAllIn_sample_can_be_compiled_with_DotnetMSBuild()
    {
        PrepareGeneratorAllInSamples();
        _compilationDriver.SetBuildTool(BuildTool.DotnetMSBuild);

        _compilationDriver.CompileSolution();
    }

    //TODO: test different outcomes: success, failure, pending, undefined, ignored (scenario & scenario outline)
    //TODO: test async steps (async steps are executed in order)
    //TODO: test before/after test run hooks (.NET Framework version of Reqnroll is subscribed to assembly unload)
}
