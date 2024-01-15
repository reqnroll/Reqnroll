using System;
using System.Runtime.InteropServices;
using TechTalk.SpecFlow.TestProjectGenerator.Driver;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class Compiler
    {
        private readonly VisualStudioFinder _visualStudioFinder;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly IOutputWriter _outputWriter;

        public Compiler(VisualStudioFinder visualStudioFinder, TestProjectFolders testProjectFolders, IOutputWriter outputWriter)
        {
            _visualStudioFinder = visualStudioFinder;
            _testProjectFolders = testProjectFolders;
            _outputWriter = outputWriter;
        }

        public CompileResult Run(BuildTool buildTool, bool? treatWarningsAsErrors)
        {
            switch (buildTool)
            {
                case BuildTool.MSBuild:
                    return CompileWithMSBuild(treatWarningsAsErrors);
                case BuildTool.DotnetBuild:
                    return CompileWithDotnetBuild(treatWarningsAsErrors);
                case BuildTool.DotnetMSBuild:
                    return CompileWithDotnetMSBuild(treatWarningsAsErrors);
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildTool), buildTool, null);
            }
        }

        private CompileResult CompileWithMSBuild(bool? treatWarningsAsErrors)
        {
            string msBuildPath="";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                msBuildPath = _visualStudioFinder.FindMSBuild();
            }
            else
            {
                msBuildPath = "msbuild";
            }

            _outputWriter.WriteLine($"Invoke MsBuild from {msBuildPath}");

            var processHelper = new ProcessHelper();
            string argumentsFormat = $"{GetWarningAsErrorParameter(treatWarningsAsErrors)} -restore -binaryLogger:;ProjectImports=None -nodeReuse:false -v:m \"{_testProjectFolders.PathToSolutionFile}\"";

            var msBuildProcess = processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, msBuildPath, argumentsFormat);

            return new CompileResult(msBuildProcess.ExitCode, msBuildProcess.CombinedOutput);
        }

        private string GetWarningAsErrorParameter(bool? treatWarningsAsErrors)
        {
            return treatWarningsAsErrors is true ? "-warnaserror" : "";
        }

        private CompileResult CompileWithDotnetBuild(bool? treatWarningsAsErrors)
        {
            _outputWriter.WriteLine("Invoking dotnet build");

            var processHelper = new ProcessHelper();

            string argumentsFormat = $"build {GetWarningAsErrorParameter(treatWarningsAsErrors)} --no-cache -binaryLogger:;ProjectImports=None -nodeReuse:false -nologo -v:m \"{_testProjectFolders.PathToSolutionFile}\"";
            var dotnetBuildProcessResult = processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, "dotnet", argumentsFormat);

            return new CompileResult(dotnetBuildProcessResult.ExitCode, dotnetBuildProcessResult.CombinedOutput);
        }

        private CompileResult CompileWithDotnetMSBuild(bool? treatWarningsAsErrors)
        {
            _outputWriter.WriteLine($"Invoking dotnet msbuild");

            var processHelper = new ProcessHelper();

            // execute dotnet restore
            processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, "dotnet", "restore -binaryLogger:;ProjectImports=None -nodeReuse:false -nologo");

            string argumentsFormat = $@"msbuild {GetWarningAsErrorParameter(treatWarningsAsErrors)} -binaryLogger:;ProjectImports=None -nodeReuse:false -nologo -v:m ""{_testProjectFolders.PathToSolutionFile}""";
            var msBuildProcess = processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, "dotnet", argumentsFormat);

            return new CompileResult(msBuildProcess.ExitCode, msBuildProcess.CombinedOutput);
        }
    }
}
