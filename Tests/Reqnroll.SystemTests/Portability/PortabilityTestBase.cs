using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        PrepareGeneratorAllInSamples();

        ExecuteTests();

        ShouldAllScenariosPass();
    }

    [TestMethod]
    [DataRow(BuildTool.MSBuild)]
    [DataRow(BuildTool.DotnetMSBuild)]
    public void GeneratorAllIn_sample_can_be_compiled_with_MsBuild(BuildTool buildTool)
    {
        PrepareGeneratorAllInSamples();
        _compilationDriver.SetBuildTool(buildTool);

        _compilationDriver.CompileSolution();
    }

    //TODO: test different outcomes: success, failure, pending, undefined, ignored (scenario & scenario outline)
    //TODO: test async steps (async steps are executed in order)
    //TODO: test before/after test run hooks (.NET Framework version of Reqnroll is subscribed to assembly unload)
}
