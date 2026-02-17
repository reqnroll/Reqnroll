using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Reqnroll.SystemTests.MsBuildIntegration;

[TestClass]
[TestCategory("MsBuildIntegration")]
public class MsBuildIntegrationTest : SystemTestBase
{
    public enum CodeBehindLocation
    {
        ProjectFolder,
        IntermediateOutputPath
    }

    private void AddStepBindings()
    {
        _projectsDriver.AddStepBinding("When", regex: "embedded messages resources are reported",
                                       """
                                       foreach (var resourceName in System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames())
                                       {
                                           global::Log.LogCustom("resource", resourceName);
                                       }
                                       """);
        _projectsDriver.AddStepBinding("When", regex: "the namespace is reported",
                                       """
                                       global::Log.LogCustom("namespace", new System.Diagnostics.StackTrace(true).GetFrames().FirstOrDefault(f => f.GetFileName()?.Contains(".feature") ?? false)?.GetMethod()?.DeclaringType?.Namespace);
                                       """);
    }

    private List<string> PrepareProject(CodeBehindLocation codeBehindLocation = CodeBehindLocation.ProjectFolder)
    {
        _solutionDriver.DefaultProject.UseIntermediateOutputPathForCodeBehind = codeBehindLocation == CodeBehindLocation.IntermediateOutputPath;

        var featureFiles = new List<string>();

        AddFeatureFile(
            """
            Feature: Feature A

            Scenario: Test Scenario
                When embedded messages resources are reported
            """);
        featureFiles.Add(_projectsDriver.LastFeatureFile.Path);

        AddFeatureFile(
            """
            Feature: Feature B

            Scenario: Test Scenario
                When embedded messages resources are reported
            """);
        featureFiles.Add(_projectsDriver.LastFeatureFile.Path);


        AddStepBindings();

        _compilationDriver.CompileSolution(logLevel: "n");
        foreach (string featureFile in featureFiles)
        {
            _compilationResultDriver.CompileResult.Output.Should().MatchRegex($@"\[Reqnroll\] Generated code-behind file: .*{featureFile}\.cs");
            _compilationResultDriver.CompileResult.Output.Should().MatchRegex($@"\[Reqnroll\] Generated messages file: .*{featureFile}\.ndjson");
        }

        return featureFiles;
    }

    private void CheckProjectConsistency(List<string> featureFiles)
    {
        ExecuteTests();

        // make sure that both tests were generated and included to the test assembly
        ShouldAllScenariosPass(2);

        // make sure the ndjson resource has been embedded for all feature files
        _bindingDriver.GetActualLogLines("resource")
                      .Distinct()
                      .Should()
                      .BeEquivalentTo(featureFiles.Select(ff => $"-> resource: {ff}.ndjson:StepBinding"));
    }

    [TestMethod]
    [DataRow(CodeBehindLocation.ProjectFolder)]
    [DataRow(CodeBehindLocation.IntermediateOutputPath)]
    public void Should_produce_all_outputs_on_first_build(CodeBehindLocation codeBehindLocation)
    {
        var featureFiles = PrepareProject(codeBehindLocation);

        CheckProjectConsistency(featureFiles);

        KeepPassingResults = true;
    }

    [TestMethod]
    [DataRow(CodeBehindLocation.ProjectFolder)]
    [DataRow(CodeBehindLocation.IntermediateOutputPath)]
    public void Should_detect_when_all_files_are_up_to_date(CodeBehindLocation codeBehindLocation)
    {
        var featureFiles = PrepareProject(codeBehindLocation);

        _compilationDriver.CompileSolution(logLevel: "bl");
        _compilationResultDriver.CompileResult.Output.Should().Contain("Skipping target \"CoreProcessReqnrollFeatureFilesInProject\" because all output files are up-to-date with respect to the input files.");
        _compilationResultDriver.CompileResult.Output.Should().NotContain("[Reqnroll] Generated code-behind file:");
        _compilationResultDriver.CompileResult.Output.Should().NotContain("[Reqnroll] Generated messages file:");

        // The 'CoreCompile' target is not skipped, because in the first compilation the
        // code-behind files were added to the 'Compile' item group in our target, but for the
        // second one they are added by the SDK as they are C# files in the project folder.
        // Although the files are identical, the 'CoreCompile' hash calculation also takes the
        // order into account so the 'CoreCompile' will be executed. This issue could be avoided
        // by removing and re-adding the items in the target, but since this is only a one-time
        // problem of projects that have never been compiled, the impact is minimal.
        // Once the code-behind files will be generated in the 'obj' folder, the problem will not occur anymore.
        // For now, we test this with a 3rd compilation below.

        CheckProjectConsistency(featureFiles);

        // 3rd compilation to verify that also 'CoreCompile' is skipped
        _compilationDriver.CompileSolution(logLevel: "n");
        _compilationResultDriver.CompileResult.Output.Should().Contain("Skipping target \"CoreCompile\" because all output files are up-to-date with respect to the input files.");
    }

    [TestMethod]
    [DataRow(CodeBehindLocation.ProjectFolder)]
    [DataRow(CodeBehindLocation.IntermediateOutputPath)]
    public void Should_detect_when_only_feature_files_changed(CodeBehindLocation codeBehindLocation)
    {
        var featureFiles = PrepareProject(codeBehindLocation);

        // we change the first feature file in alphabetical order, because in case of issues,
        // that is moved to the end of the lists so there is more likely to catch problems
        featureFiles.Sort();
        var changedFeatureFile = featureFiles[0];
        var notChangedFeatureFile = featureFiles[1];
        string changedFileFullPath = Path.Combine(_testProjectFolders.PathToSolutionDirectory, _solutionDriver.DefaultProject.ProjectName, changedFeatureFile);
        var fileContent = File.ReadAllText(changedFileFullPath);
        // make a relevant change
        var changedContent = fileContent.Replace("Test Scenario", "Test Scenario - changed at " + DateTime.Now.Ticks);
        changedContent.Should().NotBe(fileContent);
        File.WriteAllText(changedFileFullPath, changedContent);

        _compilationDriver.CompileSolution(logLevel: "n");
        // the changed feature file should be re-processed
        _compilationResultDriver.CompileResult.Output.Should().MatchRegex($@"\[Reqnroll\] Generated code-behind file: .*{changedFeatureFile}\.cs");
        _compilationResultDriver.CompileResult.Output.Should().MatchRegex($@"\[Reqnroll\] Generated messages file: .*{changedFeatureFile}\.ndjson");

        // the not changed feature file should be skipped
        _compilationResultDriver.CompileResult.Output.Should().NotMatchRegex($@"\[Reqnroll\] Generated code-behind file: .*{notChangedFeatureFile}\.cs");
        _compilationResultDriver.CompileResult.Output.Should().NotMatchRegex($@"\[Reqnroll\] Generated messages file: .*{notChangedFeatureFile}\.ndjson");

        CheckProjectConsistency(featureFiles);

        // Any subsequent compilation makes everything up-to-date again
        _compilationDriver.CompileSolution(logLevel: "n");
        _compilationResultDriver.CompileResult.Output.Should().Contain("Skipping target \"CoreProcessReqnrollFeatureFilesInProject\" because all output files are up-to-date with respect to the input files.");
        _compilationResultDriver.CompileResult.Output.Should().Contain("Skipping target \"CoreCompile\" because all output files are up-to-date with respect to the input files.");
    }

    [TestMethod]
    [DataRow(CodeBehindLocation.ProjectFolder)]
    [DataRow(CodeBehindLocation.IntermediateOutputPath)]
    public void Should_detect_when_only_feature_files_changed_in_a_way_that_code_behind_not_changed(CodeBehindLocation codeBehindLocation)
    {
        var featureFiles = PrepareProject(codeBehindLocation);

        var changedFeatureFile = featureFiles[0];
        var notChangedFeatureFile = featureFiles[1];
        string changedFileFullPath = Path.Combine(_testProjectFolders.PathToSolutionDirectory, _solutionDriver.DefaultProject.ProjectName, changedFeatureFile);
        var fileContent = File.ReadAllText(changedFileFullPath);
        // make a non-relevant change (the code-behind will be exactly the same)
        var changedContent = fileContent.Replace("Feature A", "Feature A     "); // whitespaces in feature name are trimmed
        changedContent.Should().NotBe(fileContent);
        File.WriteAllText(changedFileFullPath, changedContent);

        _compilationDriver.CompileSolution(logLevel: "n");
        // the changed feature file should be re-processed
        _compilationResultDriver.CompileResult.Output.Should().MatchRegex($@"\[Reqnroll\] Generated code-behind file: .*{changedFeatureFile}\.cs");
        _compilationResultDriver.CompileResult.Output.Should().MatchRegex($@"\[Reqnroll\] Generated messages file: .*{changedFeatureFile}\.ndjson");

        // the not changed feature file should be skipped
        _compilationResultDriver.CompileResult.Output.Should().NotMatchRegex($@"\[Reqnroll\] Generated code-behind file: .*{notChangedFeatureFile}\.cs");
        _compilationResultDriver.CompileResult.Output.Should().NotMatchRegex($@"\[Reqnroll\] Generated messages file: .*{notChangedFeatureFile}\.ndjson");

        CheckProjectConsistency(featureFiles);

        // Any subsequent compilation makes everything up-to-date again
        _compilationDriver.CompileSolution(logLevel: "n");
        _compilationResultDriver.CompileResult.Output.Should().Contain("Skipping target \"CoreProcessReqnrollFeatureFilesInProject\" because all output files are up-to-date with respect to the input files.");
        _compilationResultDriver.CompileResult.Output.Should().Contain("Skipping target \"CoreCompile\" because all output files are up-to-date with respect to the input files.");
    }

    [TestMethod]
    public void Should_support_linked_files()
    {
        _solutionDriver.DefaultProject.UseIntermediateOutputPathForCodeBehind = true;

        var featureFile = "LinkedFeature.feature";
        var linkTargetFolder = "TargetFolder";
        var featureFileContent = """
                                 Feature: Feature A

                                 Scenario: Test Scenario
                                     When the namespace is reported
                                     And embedded messages resources are reported
                                 """;

        _solutionDriver.DefaultProject.AddFile(new ProjectFile( Path.Combine("..", "SomeFolder",featureFile), "ReqnrollFeatureFile", featureFileContent,
            CopyToOutputDirectory.DoNotCopy, 
            new Dictionary<string, string> { { "Link", Path.Combine(linkTargetFolder, featureFile) }}));

        AddStepBindings();

        _compilationDriver.CompileSolution(logLevel: "n");

        _compilationResultDriver.CompileResult.Output.Should().MatchRegex($@"\[Reqnroll\] Generated code-behind file: .*{featureFile}\.cs");
        _compilationResultDriver.CompileResult.Output.Should().MatchRegex($@"\[Reqnroll\] Generated messages file: .*{featureFile}\.ndjson");

        ExecuteTests();

        // make sure that both tests were generated and included to the test assembly
        ShouldAllScenariosPass(1);

        // make sure namespace of the generated test is what we specified as 'Link'
        _bindingDriver.GetActualLogLines("namespace")
                      .Distinct()
                      .Should()
                      .BeEquivalentTo($"-> namespace: {_solutionDriver.DefaultProject.ProjectName}.{linkTargetFolder}:StepBinding");

        // make sure the ndjson resource has been embedded for all feature files
        _bindingDriver.GetActualLogLines("resource")
                      .Distinct()
                      .Should()
                      .BeEquivalentTo($"-> resource: {linkTargetFolder}/{featureFile}.ndjson:StepBinding");

    }

    [TestMethod]
    public void Should_copy_config_file_to_output_folder()
    {
        AddSimpleScenario();
        _projectsDriver.AddPassingStepBinding();

        _compilationDriver.CompileSolution(logLevel: "n");

        string expectedConfigFilePath = Path.Combine(_testProjectFolders.PathToSolutionDirectory, _solutionDriver.DefaultProject.ProjectName, "bin", "Debug", _solutionDriver.DefaultProject.TargetFrameworkMoniker, "reqnroll.json");
        File.Exists(expectedConfigFilePath).Should().BeTrue($"the config file should be copied to the output folder '{expectedConfigFilePath}'");
    }
}