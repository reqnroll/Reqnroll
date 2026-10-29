using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.SystemTests.MsBuildIntegration;

/// <summary>
/// Tests for legacy (non-SDK-style) project files, where the code-behind files are not included
/// in the compilation implicitly (see https://github.com/reqnroll/Reqnroll/issues/1126).
/// These tests need the Visual Studio MSBuild, because the .NET SDK does not resolve the assembly
/// references of NuGet packages in legacy projects.
/// </summary>
[TestClass]
[TestCategory("NetFramework")]
[TestCategory("MsBuild")]
public class LegacyProjectFormatTest : SystemTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.ProjectFormat = ProjectFormat.Old;
        _testRunConfiguration.TargetFramework = TargetFramework.Net481;
        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.CSharp73;
    }

    [TestMethod]
    public void Should_warn_when_code_behind_files_are_not_listed_in_the_project_file()
    {
        AddSimpleScenario();
        _projectsDriver.AddPassingStepBinding();

        _compilationDriver.SetBuildTool(BuildTool.MSBuild);
        _compilationDriver.CompileSolution();

        _compilationResultDriver.CompileResult.Output.Should().MatchRegex($@"warning : The generated code-behind file '.*{_projectsDriver.LastFeatureFile.Path}\.cs' is not included in the compilation");
    }

    [TestMethod]
    public void Should_discover_and_execute_tests_when_code_behind_files_are_generated_to_intermediate_output_path()
    {
        _solutionDriver.DefaultProject.UseIntermediateOutputPathForCodeBehind = true;
        AddSimpleScenario();
        _projectsDriver.AddPassingStepBinding();

        _compilationDriver.SetBuildTool(BuildTool.MSBuild);
        ExecuteTests();

        _compilationResultDriver.CompileResult.Output.Should().NotContain("is not included in the compilation");
        ShouldAllScenariosPass();
    }
}
