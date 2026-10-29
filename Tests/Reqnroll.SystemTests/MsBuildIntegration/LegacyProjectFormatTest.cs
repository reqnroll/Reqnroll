using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.SystemTests.MsBuildIntegration;

/// <summary>
/// Tests for legacy (non-SDK-style) project files, where the code-behind files are not listed
/// in the project file explicitly (see https://github.com/reqnroll/Reqnroll/issues/1126).
/// </summary>
[TestClass]
[TestCategory("NetFramework")]
public class LegacyProjectFormatTest : SystemTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.ProjectFormat = ProjectFormat.Old;
        _testRunConfiguration.TargetFramework = TargetFramework.Net481;
        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.CSharp73;
    }

    private void Should_discover_and_execute_tests(BuildTool buildTool, bool useIntermediateOutputPathForCodeBehind)
    {
        _solutionDriver.DefaultProject.UseIntermediateOutputPathForCodeBehind = useIntermediateOutputPathForCodeBehind;
        AddSimpleScenarioAndOutline();
        _projectsDriver.AddPassingStepBinding();

        _compilationDriver.SetBuildTool(buildTool);
        ExecuteTests();

        ShouldAllScenariosPass();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Should_discover_and_execute_tests_with_dotnet_build(bool useIntermediateOutputPathForCodeBehind)
    {
        Should_discover_and_execute_tests(BuildTool.DotnetBuild, useIntermediateOutputPathForCodeBehind);
    }

    [TestMethod]
    [TestCategory("MsBuild")]
    // Temporarily ignored until https://github.com/reqnroll/Reqnroll/issues/1128 is fixed
    [Ignore("Temporarily ignored until #1128 is fixed")]
    [DataRow(false)]
    [DataRow(true)]
    public void Should_discover_and_execute_tests_with_MsBuild(bool useIntermediateOutputPathForCodeBehind)
    {
        Should_discover_and_execute_tests(BuildTool.MSBuild, useIntermediateOutputPathForCodeBehind);
    }
}
